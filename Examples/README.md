# ConvertAPI .NET SDK examples

Each folder is a small console app that shows one task with the ConvertAPI .NET SDK. The examples build against the SDK source in this repository, so you need the .NET 10 SDK.

## Run an example

1. Get your API token at https://www.convertapi.com/a/authentication
2. Put it in the `CONVERTAPI_TOKEN` environment variable:

   ```bash
   export CONVERTAPI_TOKEN=your-api-token      # macOS and Linux
   ```

   ```powershell
   $env:CONVERTAPI_TOKEN = "your-api-token"    # Windows PowerShell
   ```

3. From the repository root, run an example by its folder name:

   ```bash
   dotnet run --project Examples/CreatePdfThumbnail
   ```

The example prints where it saved the result, usually in your temp folder. Most examples then wait for Enter before they exit.

In Visual Studio or Rider, set the example as the startup project. The IDE only sees the variable if it was set before the IDE started: on Windows, `setx CONVERTAPI_TOKEN your-api-token` sets it for programs you start afterwards.

## Sample files

The input files are in `TestFiles` (SplitPDF has its own in `SplitPDF/files`). Each example's `.csproj` copies the files it uses next to the program when it builds, and `Program.cs` finds them through `AppContext.BaseDirectory`, so the examples run from any working directory. To convert your own file, put its path in `Program.cs` instead.
