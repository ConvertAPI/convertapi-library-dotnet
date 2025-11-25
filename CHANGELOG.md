# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [3.1.6] — 2025-11-12

### Added
- Add `UploadOnceReuseTwice` example project for reusing uploaded files in multiple conversions (08dede5)
- Add `GetConverterInfoDemo` example and implement `GetConverterInfo` method (c9500df)
- Add `DeleteFiles` example and implement `DeleteFilesAsync` method (dd7e5d7)

### Changed
- Refactor `ConvertApiFiles` to `ConvertApiFile` across the codebase for clarity and consistency (46e77ef, 721b5ec)
- Upgrade all example projects to .NET 6 for performance and modern SDK features (c0428d3)
- Rename `GetValueAsync` to `GetUploadedFileAsync` (mark old name obsolete) and update `UploadOnceReuseTwice` example (868e16d)
- Enhance `ConvertApiFileParam` constructor validation (4557675)
- Make `UploadOnceReuseTwice` example more robust with dynamic test file path resolution (a4a6e4b)
- Make `CopyToAsync` awaitable in `ConvertApiExtension` for improved asynchronous handling (9839cf5)

### Merged
- Merge pull request #65 from `ConvertAPI/develop` (86f0220)
- Merge remote-tracking branch `origin/develop` into `develop` (5ef1683)

<!--
Compare links can be added once tags are in place, e.g.:
[3.1.6]: https://github.com/ConvertAPI/convertapi-dotnet/compare/v3.1.5...v3.1.6
-->
