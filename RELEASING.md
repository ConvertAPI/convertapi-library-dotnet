# Releasing

`main` is the only long-lived branch. Changes reach it through pull requests, merge commits only, and
every pull request and push to `main` must pass the `build` check (`.github/workflows/build.yml`: restore,
build the library, its unit tests and every example, run the unit tests, pack). Merging publishes nothing.

A release is a **GitHub Release** whose tag is the version, published by `tomasr78`:

```bash
gh release create v3.1.19 --repo ConvertAPI/convertapi-library-dotnet --target main --title v3.1.19 --generate-notes
```

That runs `.github/workflows/publish.yml` in the `nuget` environment, which:

1. refuses a Release published or re-run by anyone else;
2. checks the tag is SemVer (`v3.1.19`, `v3.2.0-preview.1`), that the GitHub pre-release tick matches
   the version, that a stable version is a commit on `main`, and that nuget.org has never had that version;
3. builds and packs that commit with `-p:Version={tag without v}`, failing on any high or critical
   advisory in the dependency graph. The package's readme is the repository's `README.md`, with
   `utm_source=github` in its links rewritten to `utm_source=nuget`;
4. pushes `ConvertApi.{version}.nupkg` and its `.snupkg` to nuget.org and waits until nuget.org serves it.

**Pre-releases** work the same way and may come from any branch:

```bash
gh release create v3.2.0-preview.1 --repo ConvertAPI/convertapi-library-dotnet --target my-branch --prerelease --title v3.2.0-preview.1 --notes "Preview of ..."
```

## Release owner

**TODO: name the one person who owns each release** (runs the checklist, publishes, answers for the
notes). Until that is decided, note that `publish.yml` only accepts a Release published by `tomasr78`.

## Before each release

- [ ] `CHANGELOG.md`: move the `[Unreleased]` entries under `## [x.y.z] - YYYY-MM-DD` and leave an empty
      `[Unreleased]` above it.
- [ ] Preview the README as nuget.org will show it: download the `package` artifact of the latest `build`
      run on `main`, open `README.md` inside the `.nupkg` (it is a zip), and check that it renders and that
      its links work. Check the icon, description and tags in the `.nuspec` next to it.
- [ ] Bump every pinned SDK version to the new one: install lines or `Version="..."` in the README, the
      examples and any snippet that names a version.
- [ ] Check that the C# quick start on https://www.convertapi.com/integrations/libraries/csharp still
      matches the README (install command, token, first conversion). If it does not, ask for the page to
      be updated before announcing the release.
- [ ] Write the release notes with the template below, then publish.

## Release notes template

```markdown
## What's new

- <the change a user notices first, one line>
- <next change>

## Fixed

- <bug, who it affected, what happens now>

## Upgrading

<"Drop-in: no code changes." or what to change, with a short snippet.>

Full changelog: https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/CHANGELOG.md
Install: `dotnet add package ConvertApi --version x.y.z`
```

Pass it with `--notes-file notes.md` instead of `--generate-notes`.

## Mistakes

nuget.org versions are permanent. A bad version can only be unlisted (and, if needed, deprecated) on
nuget.org, then fixed by releasing the next version. Never delete and re-create a release tag.

The version in `ConvertApi.csproj` is a `0.0.0-local` placeholder; pull request builds pack
`0.0.0-ci.{run}.g{sha}` as a downloadable artifact so a change can be tried before it is released.
Update `CHANGELOG.md` in the pull request that changes behaviour.
