# Releasing

`main` is the only long-lived branch. Changes reach it through pull requests, merge commits only, and
every pull request and push to `main` must pass the `build` check (`.github/workflows/build.yml`: restore,
build the library and every example, pack). Merging publishes nothing.

A release is a **GitHub Release** whose tag is the version, published by `tomasr78`:

```bash
gh release create v3.1.19 --repo ConvertAPI/convertapi-library-dotnet --target main --title v3.1.19 --generate-notes
```

That runs `.github/workflows/publish.yml` in the `nuget` environment, which:

1. refuses a Release published or re-run by anyone else;
2. checks the tag is SemVer (`v3.1.19`, `v3.2.0-preview.1`), that the GitHub pre-release tick matches
   the version, that a stable version is a commit on `main`, and that nuget.org has never had that version;
3. builds and packs that commit with `-p:Version={tag without v}`, failing on any high or critical
   advisory in the dependency graph;
4. pushes `ConvertApi.{version}.nupkg` and its `.snupkg` to nuget.org and waits until nuget.org serves it.

**Pre-releases** work the same way and may come from any branch:

```bash
gh release create v3.2.0-preview.1 --repo ConvertAPI/convertapi-library-dotnet --target my-branch --prerelease --title v3.2.0-preview.1 --notes "Preview of ..."
```

nuget.org versions are permanent. A bad version can only be unlisted (and, if needed, deprecated) on
nuget.org, then fixed by releasing the next version. Never delete and re-create a release tag.

The version in `ConvertApi.csproj` is a `0.0.0-local` placeholder; pull request builds pack
`0.0.0-ci.{run}.g{sha}` as a downloadable artifact so a change can be tried before it is released.
Update `CHANGELOG.md` in the pull request that changes behaviour.
