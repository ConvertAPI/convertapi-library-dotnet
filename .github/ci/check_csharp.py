#!/usr/bin/env python3
"""Runs the C# examples and the C# code in README.md against the ConvertAPI API.

CI only: the Examples workflow (.github/workflows/examples.yml) calls it. MSBuild imports ci-host.targets
into every program that job builds, which compiles in CiHost.cs: the programs run as people copy them,
and the files they upload or get back are deleted from the server when they exit.

    check_csharp.py examples [NAME ...]       run every project in Examples/, or the named ones (built beforehand)
    check_csharp.py readme-build --work DIR   make a console project of each C# block in README.md and build it
    check_csharp.py readme-run --work DIR     run the quick start and the "... in C#" task blocks

The runs need CONVERTAPI_TOKEN and convert real files on that token's account: about 40 conversions for
all of them. A failed run is tried again up to twice, CI_RETRY_DELAY seconds apart (30 by default), because
the API can answer 503 for a moment. Only the Python 3 standard library is used. To run it on your
machine, from the repository root:

    export CONVERTAPI_TOKEN=your-api-token
    export CustomAfterMicrosoftCommonTargets="$PWD/.github/ci/ci-host.targets"
    dotnet build ConvertApi.sln -c Release && python3 .github/ci/check_csharp.py examples
"""

import argparse
import json
import os
import re
import secrets
import shutil
import subprocess
import sys
import tempfile
import time
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
EXAMPLES = REPO / "Examples"
SAMPLES = EXAMPLES / "TestFiles"
README = REPO / "README.md"
LIBRARY = REPO / "ConvertApi" / "ConvertApi.csproj"
CONFIGURATION = "Release"

ATTEMPTS = 3
RETRY_DELAY = int(os.environ.get("CI_RETRY_DELAY", "30"))
RUN_TIMEOUT = 300  # seconds for one run of one program

# CiHost.cs prints this line before Main. A run without it was built without CiHost.cs, so nothing
# deleted its files from the server.
CI_LINE = "[ci] ConvertApi.ApiBaseUri = "
# ... and this one when the program exits.
DELETED_LINE = re.compile(r"^\[ci\] Deleted (\d+) of (\d+) files from the server$", re.MULTILINE)

# The examples catch ConvertApiException, print it and still exit with 0, so these lines mark a failed call.
ERROR_LINE = re.compile(
    r"^(Status Code:|Response:|Unhandled exception|Failed to |Call failed:|Reload failed:|Conversion failed)",
    re.MULTILINE,
)

# An example that shows error handling passes only when it prints the error it is written to provoke.
EXPECTED_OUTPUT = {
    # Sends a Word file to the PDF split converter, which the API rejects with 400 Bad Request.
    "ExceptionHandling": re.compile(r"^Status Code: BadRequest$", re.MULTILINE),
}

# Account fields that must never reach the public log, also as quoted JSON keys ("Secret": ...).
# The token itself is checked by value.
SECRET_FIELD = re.compile(r"""(?i)\b(secret|api[ _-]?key)["']?\s*[=:]""")

# Account details that RetrieveUserInformation prints. They are not secrets, but they stay out of the public log.
ACCOUNT_FIELD = re.compile(r"""^\s*["']?(FullName|Email|ConversionsTotal|ConversionsConsumed|SecondsLeft)["']?\s*[=:]""")

# README.md names its input files document.docx, page.html and so on; the runs copy these samples in under those names.
SAMPLE_FOR_EXTENSION = {
    ".docx": "test.docx",
    ".html": "test.html",
    ".pdf": "test.pdf",
    ".pptx": "test.pptx",
    ".xlsx": "test.xlsx",
}

# The README blocks that run against the API: the quick start program and the blocks under the
# "... in C#" task headings. Every other block is only built.
QUICK_START_HEADING = "Quick start"
RUN_HEADING = re.compile(r" in C#$")

# A complete README program has this using directive. Any other C# block is a fragment.
PROGRAM_START = re.compile(r"^using ConvertApiDotNet;", re.MULTILINE)

# The local file names a block uploads: new ConvertApiFileParam("document.pdf") or ("CompareFile", "revised.docx").
FILE_PARAM = re.compile(r'new ConvertApiFileParam\(\s*(?:"\w+"\s*,\s*)?"([^"]+)"\s*\)')


# ---------------------------------------------------------------------------------------------------------
# Output for the GitHub Actions log


def escape_data(text):
    return text.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")


def escape_property(text):
    return escape_data(text).replace(":", "%3A").replace(",", "%2C")


def annotate(level, message, title=None, file=None, line=None):
    """Prints an error, warning or notice annotation, which GitHub also shows on the run's summary page."""
    props = []
    if file:
        props.append(f"file={escape_property(file)}")
    if line:
        props.append(f"line={line}")
    if title:
        props.append(f"title={escape_property(title)}")
    print(f"::{level} {','.join(props)}::{escape_data(message)}" if props else f"::{level}::{escape_data(message)}")


def print_program_output(text, token):
    """Prints a program's output with workflow commands switched off, so nothing it prints is run as one."""
    if token:
        text = text.replace(token, "***")
    lines = text.splitlines()
    if len(lines) > 60:
        lines = lines[:40] + [f"... {len(lines) - 60} more lines ..."] + lines[-20:]
    marker = secrets.token_hex(16)
    print(f"::stop-commands::{marker}")
    for line in lines:
        line = "".join(ch if ch.isprintable() or ch == "\t" else "?" for ch in line)
        account = ACCOUNT_FIELD.match(line)
        if SECRET_FIELD.search(line):
            line = "(line hidden: it names a secret field)"
        elif account:
            line = f"{account.group(1)}=(hidden in this public log)"
        print("    " + (line if len(line) <= 300 else line[:300] + " ..."))
    print(f"::{marker}::")


def write_summary(markdown):
    path = os.environ.get("GITHUB_STEP_SUMMARY")
    if path:
        with open(path, "a", encoding="utf-8") as f:
            f.write(markdown + "\n")


def table_cell(text):
    return str(text).replace("|", "\\|").replace("\n", " ")


# ---------------------------------------------------------------------------------------------------------
# Running a program


def require_token():
    if not os.environ.get("CONVERTAPI_TOKEN"):
        annotate("error", "CONVERTAPI_TOKEN is not set, so nothing can run against the API")
        sys.exit(1)
    return os.environ["CONVERTAPI_TOKEN"]


def program_output(text):
    """The program's own output, without the lines CiHost.cs adds."""
    return "\n".join(line for line in text.splitlines() if not line.startswith("[ci] "))


def find_leaks(text, token):
    leaks = []
    if token and token in text:
        leaks.append("the output contains the API token")
    match = SECRET_FIELD.search(text)
    if match:
        leaks.append(f"the output prints a secret field ({match.group(1)})")
    return leaks


def run_with_retries(label, command, cwd, check, token, env=None, reset=None):
    """Runs a program until check() finds no problem, at most ATTEMPTS times. Returns (passed, attempts, problems)."""
    problems = []
    for attempt in range(1, ATTEMPTS + 1):
        if reset:
            reset()
        started = time.monotonic()
        try:
            completed = subprocess.run(
                command, cwd=cwd, env=env, stdin=subprocess.DEVNULL,
                stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=RUN_TIMEOUT,
            )
            exit_code, raw = completed.returncode, completed.stdout
        except subprocess.TimeoutExpired as e:
            exit_code, raw = None, e.stdout or b""
        seconds = time.monotonic() - started
        text = raw.decode("utf-8", "replace")

        leaks = find_leaks(text, token)
        problems = leaks or check(exit_code, text)
        if not problems:
            print(f"::group::{label}: passed ({seconds:.1f} s{f', attempt {attempt}' if attempt > 1 else ''})")
            print_program_output(text, token)
            print("::endgroup::")
            # Not a failure: the API deletes what is left after 3 hours anyway.
            deleted = DELETED_LINE.search(text)
            if not deleted or deleted.group(1) != deleted.group(2):
                annotate("warning", f"{label}: {deleted.group(0)[5:] if deleted else 'CiHost.cs did not report deleting its files'}",
                         title="Files left on the server")
            return True, attempt, []

        print(f"{label}: FAILED on attempt {attempt} of {ATTEMPTS} ({seconds:.1f} s): {'; '.join(problems)}")
        print_program_output(text, token)
        # A leak is in the code, not in the API, so another attempt would only print it again.
        if leaks or attempt == ATTEMPTS:
            break
        annotate("warning", f"{label} failed on attempt {attempt}: {'; '.join(problems)}. Trying again in {RETRY_DELAY} s.",
                 title="Retrying a failed run")
        time.sleep(RETRY_DELAY)
    return False, attempt, problems


def common_problems(exit_code, text):
    problems = []
    if exit_code is None:
        problems.append(f"timed out after {RUN_TIMEOUT} s")
    elif exit_code != 0:
        problems.append(f"exit code {exit_code}")
    if CI_LINE not in text:
        problems.append("it did not print the [ci] line: .github/ci/CiHost.cs was not compiled in")
    return problems


# ---------------------------------------------------------------------------------------------------------
# Examples


def run_examples(args):
    token = require_token()
    projects = sorted(p for p in EXAMPLES.glob("*/*.csproj") if not args.names or p.stem in args.names)
    if not projects or (args.names and len(projects) != len(set(args.names))):
        annotate("error", f"No example projects found in {EXAMPLES} for {' '.join(args.names) or 'any name'}")
        return 1

    results = []
    with tempfile.TemporaryDirectory(prefix="examples-") as scratch:
        # The examples save their results in the temp folder; keep them in this run's scratch folder.
        env = dict(os.environ, TMPDIR=scratch + os.sep)
        for csproj in projects:
            name = csproj.stem
            expected = EXPECTED_OUTPUT.get(name)

            def check(exit_code, text, expected=expected):
                problems = common_problems(exit_code, text)
                if expected:
                    if not expected.search(text):
                        problems.append(f"it did not print the error it is written to show ({expected.pattern})")
                else:
                    error = ERROR_LINE.search(text)
                    if error:
                        line = text[error.start():].splitlines()[0]
                        problems.append(f"it printed an error: {line[:200]}")
                return problems

            # Each example runs from an empty folder: it finds its sample files next to the program itself.
            cwd = tempfile.mkdtemp(prefix=f"{name}-", dir=scratch)
            command = ["dotnet", "run", "--project", str(csproj), "-c", CONFIGURATION, "--no-build"]
            passed, attempts, problems = run_with_retries(name, command, cwd, check, token, env=env)
            results.append((name, passed, attempts, problems))

    failed = [r for r in results if not r[1]]
    for name, _, _, problems in failed:
        annotate("error", f"{name}: {'; '.join(problems)}", title="Example failed",
                 file=f"Examples/{name}/Program.cs")
    print(f"\nExamples: {len(results) - len(failed)} of {len(results)} passed against the API.")

    rows = [f"| {name} | {'passed' if passed else 'FAILED: ' + table_cell('; '.join(problems))} | {attempts} |"
            for name, passed, attempts, problems in results]
    write_summary("### Examples\n\n| Example | Result | Attempts |\n|---|---|---|\n" + "\n".join(rows) + "\n")
    return 1 if failed else 0


# ---------------------------------------------------------------------------------------------------------
# README.md


def extract_blocks(markdown):
    """Returns the C# code blocks of a Markdown file with their first line number and the heading above them."""
    blocks, heading, lines, i = [], "", markdown.splitlines(), 0
    while i < len(lines):
        fence = re.match(r"^\s*(`{3,}|~{3,})\s*([^\s`]*)", lines[i])
        if fence:
            marker, language = fence.group(1), fence.group(2).lower()
            end = i + 1
            while end < len(lines) and not lines[end].strip().startswith(marker):
                end += 1
            if language in ("csharp", "cs", "c#"):
                blocks.append({
                    "index": len(blocks) + 1,
                    "line": i + 2,  # README.md line of the block's first line of code
                    "heading": heading,
                    "code": "\n".join(lines[i + 1:end]) + "\n",
                })
            i = end + 1
            continue
        # A closing run of # needs a space before it, so the # of "in C#" stays.
        title = re.match(r"^#{1,6}\s+(.*?)(?:\s+#+)?\s*$", lines[i])
        if title:
            heading = title.group(1)
        i += 1
    return blocks


def code_lines(code):
    return [line for line in code.splitlines() if line.strip() and not line.strip().startswith("//")]


def splice(fragment, quick_start):
    """Puts a fragment into the quick start program. Returns (program, how) or (None, why it does not fit)."""
    lines = quick_start.rstrip("\n").splitlines()
    fragment_lines = fragment.rstrip("\n").splitlines()
    first = code_lines(fragment)[0].strip() if code_lines(fragment) else ""

    declared = re.match(r"var (\w+)\s*=", first)
    if declared:
        name = declared.group(1)
        for start, line in enumerate(lines):
            if re.match(rf"\s*var {name}\s*=", line):
                end = start
                while end < len(lines) - 1 and not lines[end].rstrip().endswith(";"):
                    end += 1
                program = lines[:start] + fragment_lines + lines[end + 1:]
                return "\n".join(program) + "\n", f"replaces the quick start's `var {name}` statement"
        return None, f"declares `{name}`, which the quick start program does not declare"

    # Settings such as ConvertApi.ApiBaseUri go before the quick start's first statement.
    if code_lines(fragment) and all(re.match(r"ConvertApi\.\w+\s*=.*;$", line.strip()) for line in code_lines(fragment)):
        last_using = max(i for i, line in enumerate(lines) if line.startswith("using "))
        program = lines[:last_using + 1] + [""] + fragment_lines + lines[last_using + 1:]
        return "\n".join(program) + "\n", "goes before the quick start's first statement"

    return None, "it is neither a statement of the quick start program nor a ConvertApi setting"


def slug(text):
    return re.sub(r"[^a-z0-9]+", "-", text.lower()).strip("-")[:40] or "block"


def run_quiet(command, cwd):
    completed = subprocess.run(command, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    if completed.returncode != 0:
        print(completed.stdout)
        raise SystemExit(f"{' '.join(command)} failed with exit code {completed.returncode}")
    return completed.stdout


def readme_build(args):
    work = Path(args.work).resolve()
    if work.exists():
        shutil.rmtree(work)
    (work / "projects").mkdir(parents=True)
    # The same SDK as the repository, although the projects live outside it.
    shutil.copy(REPO / "global.json", work / "global.json")

    blocks = extract_blocks(README.read_text(encoding="utf-8"))
    if not blocks:
        annotate("error", "README.md has no csharp code blocks", file="README.md")
        return 1
    quick_start = next((b for b in blocks if b["heading"] == QUICK_START_HEADING and PROGRAM_START.search(b["code"])), None)
    if quick_start is None:
        annotate("error", f"README.md has no complete program under the heading '{QUICK_START_HEADING}'", file="README.md")
        return 1

    # Each project starts from what `dotnet new console` makes and references the library source in this
    # repository, so README.md is checked against the code the next release ships.
    run_quiet(["dotnet", "new", "console", "--name", "Template", "--output", str(work / "template"), "--no-restore"], cwd=work)
    template = (work / "template" / "Template.csproj").read_text(encoding="utf-8")
    end = template.rindex("</Project>")
    csproj = (template[:end].rstrip() + "\n\n  <ItemGroup>\n"
              f'    <ProjectReference Include="{LIBRARY}" />\n'
              "  </ItemGroup>\n\n</Project>\n")

    entries, skipped = [], []
    for block in blocks:
        if PROGRAM_START.search(block["code"]):
            program, kind, how = block["code"], "program", ""
        else:
            program, how = splice(block["code"], quick_start["code"])
            kind = "fragment"
            if program is None:
                skipped.append((block, how))
                continue
        name = f"b{block['index']:02d}-{slug(block['heading'])}"
        folder = work / "projects" / name
        folder.mkdir()
        (folder / f"{name}.csproj").write_text(csproj, encoding="utf-8")
        (folder / "Program.cs").write_text(program, encoding="utf-8")
        run = kind == "program" and (block is quick_start or bool(RUN_HEADING.search(block["heading"])))
        entries.append({
            "name": name, "line": block["line"], "heading": block["heading"], "kind": kind, "how": how,
            "run": run, "inputs": FILE_PARAM.findall(program) if run else [],
            "saves_files": bool(re.search(r"\.SaveFiles?Async\(", program)),
        })

    solution = "<Solution>\n" + "".join(
        f'  <Project Path="projects/{e["name"]}/{e["name"]}.csproj" />\n' for e in entries) + "</Solution>\n"
    (work / "readme.slnx").write_text(solution, encoding="utf-8")

    print(f"README.md has {len(blocks)} C# blocks: {sum(e['kind'] == 'program' for e in entries)} complete programs, "
          f"{sum(e['kind'] == 'fragment' for e in entries)} fragments spliced into the quick start program, "
          f"{len(skipped)} fragments skipped.")
    for e in entries:
        if e["kind"] == "fragment":
            print(f"  README.md line {e['line']} ({e['heading']}): the fragment {e['how']}")
    print("Skipped fragments:" + ("" if skipped else " none"))
    for block, why in skipped:
        print(f"  README.md line {block['line']} ({block['heading']}): skipped, {why}")
        annotate("notice", f"Not built: {why}", title="README fragment skipped", file="README.md", line=block["line"])

    print(f"::group::dotnet build of {len(entries)} README projects")
    build = subprocess.run(["dotnet", "build", "readme.slnx", "-c", CONFIGURATION, "-nologo", "-tl:off"], cwd=work,
                           stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    print(build.stdout)
    print("::endgroup::")

    # file(line,column): error CS0103: message [project.csproj], printed once per error and again in the recap
    error_line = re.compile(r"^\s*(?P<file>[^\n]+?)\((?P<line>\d+),(?P<column>\d+)\): error (?P<code>\w+): "
                            r"(?P<message>.*?) \[(?P<project>[^\]\n]+)\]$", re.MULTILINE)
    errors = {}
    for match in error_line.finditer(build.stdout):
        project = Path(match.group("project").split("::")[0]).stem
        errors.setdefault(project, set()).add((int(match.group("line")), match.group("code"), match.group("message")))

    failed = 0
    for e in entries:
        built = any((work / "projects" / e["name"] / "bin" / CONFIGURATION).glob(f"*/{e['name']}.dll"))
        e["built"] = built
        if built:
            continue
        failed += 1
        found = sorted(errors.get(e["name"], set()))
        if not found:
            annotate("error", "The program made from this block did not build; see the dotnet build output",
                     title="README block does not build", file="README.md", line=e["line"])
        for program_line, code, message in found:
            # A complete program is the block verbatim, so its line numbers map onto README.md.
            line = e["line"] + program_line - 1 if e["kind"] == "program" else e["line"]
            annotate("error", f"{code}: {message}", title="README block does not build", file="README.md", line=line)

    (work / "blocks.json").write_text(json.dumps({"blocks": entries}, indent=2), encoding="utf-8")

    rows = [f"| {e['line']} | {table_cell(e['heading'])} | {e['kind']}{' (' + table_cell(e['how']) + ')' if e['how'] else ''} | "
            f"{'built' if e['built'] else 'FAILED'} |" for e in entries]
    rows += [f"| {b['line']} | {table_cell(b['heading'])} | fragment | skipped: {table_cell(why)} |" for b, why in skipped]
    write_summary("### C# code in README.md\n\n| Line | Section | Block | Build |\n|---|---|---|---|\n" + "\n".join(rows) + "\n")

    print(f"\nREADME.md: {len(entries) - failed} of {len(entries)} projects built.")
    if build.returncode != 0 and not failed:
        annotate("error", "dotnet build failed, although every README project produced its program")
        return 1
    return 1 if failed else 0


def project_files(folder):
    return {p for p in folder.rglob("*") if p.is_file() and not ({"bin", "obj"} & set(p.relative_to(folder).parts))}


def readme_run(args):
    token = require_token()
    work = Path(args.work).resolve()
    if not (work / "blocks.json").exists():
        annotate("error", f"{work / 'blocks.json'} is missing: run readme-build first")
        return 1
    entries = [e for e in json.loads((work / "blocks.json").read_text(encoding="utf-8"))["blocks"] if e["run"]]
    results = []
    for e in entries:
        label = f"README.md line {e['line']} ({e['heading']})"
        folder = work / "projects" / e["name"]
        if not e["built"]:
            results.append((e, False, 0, ["it did not build"]))
            continue

        missing = []
        for name in sorted(set(e["inputs"])):
            sample = SAMPLE_FOR_EXTENSION.get(Path(name).suffix.lower())
            if sample is None or not (SAMPLES / sample).exists():
                missing.append(name)
            else:
                shutil.copy(SAMPLES / sample, folder / name)
        if missing:
            results.append((e, False, 0, [f"no sample file in Examples/TestFiles for {', '.join(missing)}"]))
            continue

        before = project_files(folder)

        def reset(folder=folder, before=before):
            # A clean folder for each attempt, so only this attempt's results count.
            for path in project_files(folder) - before:
                path.unlink()
            shutil.rmtree(folder / "output", ignore_errors=True)

        def check(exit_code, text, folder=folder, before=before, saves_files=e["saves_files"]):
            problems = common_problems(exit_code, text)
            if saves_files:
                if not [p for p in project_files(folder) - before if p.stat().st_size > 0]:
                    problems.append("it saved no file")
            elif not program_output(text).strip():
                problems.append("it printed nothing")
            return problems

        command = ["dotnet", "run", "--no-build", "-c", CONFIGURATION]
        passed, attempts, problems = run_with_retries(label, command, folder, check, token, reset=reset)
        results.append((e, passed, attempts, problems))

    failed = [r for r in results if not r[1]]
    for e, _, _, problems in failed:
        annotate("error", "; ".join(problems), title="README block failed against the API", file="README.md", line=e["line"])
    print(f"\nREADME.md: {len(results) - len(failed)} of {len(results)} programs passed against the API.")

    rows = [f"| {e['line']} | {table_cell(e['heading'])} | {'passed' if passed else 'FAILED: ' + table_cell('; '.join(problems))} | {attempts} |"
            for e, passed, attempts, problems in results]
    write_summary("### README.md programs\n\n| Line | Section | Result | Attempts |\n|---|---|---|---|\n"
                  + "\n".join(rows) + "\n")
    return 1 if failed or not results else 0


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)
    examples = commands.add_parser("examples", help="run every project in Examples/")
    examples.add_argument("names", nargs="*", help="run only these examples, by folder name")
    examples.set_defaults(handler=run_examples)
    for name, handler, text in (("readme-build", readme_build, "make and build a project of each C# block in README.md"),
                                ("readme-run", readme_run, "run the quick start and the task blocks")):
        command = commands.add_parser(name, help=text)
        command.add_argument("--work", required=True, help="folder for the generated projects, outside the repository")
        command.set_defaults(handler=handler)
    args = parser.parse_args()
    sys.stdout.reconfigure(line_buffering=True)
    return args.handler(args)


if __name__ == "__main__":
    sys.exit(main())
