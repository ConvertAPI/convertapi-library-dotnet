UploadOnceReuseTwice

This example demonstrates how to upload a file once and reuse it for two conversions using ConvertAPI .NET SDK.

Input
- Local DOCX: Examples/TestFiles/test.docx

Conversions (single upload reused)
- DOCX to PDF
- DOCX to PNG

How it works
- Program.cs explicitly uploads once by calling `GetUploadedFileAsync()` on `new ConvertApiFileParam(sourceFile)` to obtain a `FileId`.
- The same `FileId` is passed to `ConvertAsync` twice via `new ConvertApiParam("File", uploaded.FileId)`, so the file is not re-uploaded.

Run steps
1) Set the CONVERTAPI_TOKEN environment variable to your API token (get one at https://www.convertapi.com/a/authentication).
2) From the repository root, run `dotnet run --project Examples/UploadOnceReuseTwice`. Or open ConvertApi.sln, set UploadOnceReuseTwice as the startup project and run it.
3) Output files are saved to your system temp folder; paths are printed to console.

Related docs
- https://www.convertapi.com/docx-to-pdf
- https://www.convertapi.com/docx-to-png