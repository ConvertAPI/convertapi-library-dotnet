# ConvertAPI C# SDK for .NET

Convert Word, Excel, PowerPoint and HTML to PDF, turn PDF into Word, JPG or PNG, and extract text from, compress, merge, split or protect PDF files in C# with the [ConvertAPI](https://www.convertapi.com) REST API.

[![NuGet version](https://img.shields.io/nuget/v/ConvertApi.svg)](https://www.nuget.org/packages/ConvertApi) [![NuGet downloads](https://img.shields.io/nuget/dt/ConvertApi.svg)](https://www.nuget.org/packages/ConvertApi) [![Build](https://img.shields.io/github/actions/workflow/status/ConvertAPI/convertapi-library-dotnet/build.yml?branch=main&label=build)](https://github.com/ConvertAPI/convertapi-library-dotnet/actions/workflows/build.yml) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/LICENSE)

**Free trial: 250 conversions, no credit card.** [Get your API token](https://www.convertapi.com/a/signup?ReturnUrl=%2Fa%2Fauthentication&utm_source=github&utm_medium=referral&utm_campaign=dotnet-sdk&utm_content=readme)

## Quick start

Create a console app and add the package:

```bash
dotnet new console -o ConvertDemo
cd ConvertDemo
dotnet add package ConvertApi
```

Set your API token as an environment variable:

```bash
export CONVERTAPI_TOKEN=your-api-token      # macOS and Linux
```

```powershell
$env:CONVERTAPI_TOKEN = "your-api-token"    # Windows PowerShell
```

Replace `Program.cs` with this program. It converts a sample Word document to PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("docx", "pdf",
    new ConvertApiFileParam(new Uri("https://cdn.convertapi.com/cara/testfiles/document.docx")));
var files = await result.SaveFilesAsync(".");
Console.WriteLine($"Saved {files[0].FullName}");
```

Run `dotnet run`, and `document.pdf` appears in the current folder.

- **.NET 10 without a project:** save the program as `convert.cs`, add `#:package ConvertApi@*` as its first line, and run `dotnet run convert.cs`.
- **.NET Framework 4.6.2 or later:** install with `Install-Package ConvertApi`, add `using System;` and `using System.Threading.Tasks;`, and put the other lines inside `static async Task Main()`, because C# 7.3 has no top-level statements.
- **ASP.NET Core:** keep the token in user secrets with `dotnet user-secrets init` and `dotnet user-secrets set ConvertApi:Token your-api-token`, then create the client with `new ConvertApi(builder.Configuration["ConvertApi:Token"])`.

## Tasks

Each task links to its C# code in this README and, where there is one, to a complete example project.

| Task | Example project |
|---|---|
| [Convert Word (DOCX) to PDF](#convert-word-docx-to-pdf-in-c) | [ConvertWordToPdfAndPng](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertWordToPdfAndPng/Program.cs) |
| [Convert HTML to PDF: a file, a string or a web page](#convert-html-to-pdf-in-c) | [FromStreamToStream](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/FromStreamToStream/Program.cs), [ConvertUrlToPdf](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertUrlToPdf/Program.cs) |
| [Convert Excel (XLSX) to PDF](#convert-excel-to-pdf-in-c) | |
| [Convert PDF to Word (DOCX)](#convert-pdf-to-word-in-c) | [ConvertPdfToDocx](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToDocx/Program.cs) |
| [Convert PDF to JPG or PNG](#convert-pdf-to-jpg-or-png-in-c) | [CreatePdfThumbnail](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/CreatePdfThumbnail/Program.cs) |
| [Extract text from PDF](#extract-text-from-pdf-in-c) | |
| [Compress PDF](#compress-pdf-in-c) | |
| [Convert HTML to Word (DOCX)](#convert-html-to-docx-in-c) | |
| [Convert PowerPoint (PPTX) to PDF](#powerpoint-to-pdf) | [ConvertRemoteFile](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertRemoteFile/Program.cs) |
| [Convert an image to PDF](#image-to-pdf) | |
| [Convert PDF to PowerPoint (PPTX)](#pdf-to-powerpoint) | [ConvertPdfToPptx](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToPptx/Program.cs) |
| [Convert an eBook (EPUB) to PDF](#ebook-conversion) | |
| [Convert an email (EML, MSG) to PDF](#email-conversion) | |
| [Merge PDF files](#merge-documents) | [MergeWordDocsToPdf](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/MergeWordDocsToPdf/Program.cs), [SplitAndMergePdf](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/SplitAndMergePdf/Program.cs) |
| [Split a PDF](#split-pdf) | [SplitPDF](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/SplitPDF/Program.cs) |
| [Rotate PDF pages](#rotate-pages) | |
| [Delete PDF pages](#delete-pages) | |
| [Add a watermark to a PDF](#add-watermark) | [WatermarkPdf](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/WatermarkPdf/Program.cs) |
| [Flatten a PDF](#flatten-pdf) | |
| [Repair a PDF](#repair-pdf) | |
| [Rasterize a PDF](#rasterize-pdf) | |
| [Password-protect a PDF](#password-protect-pdf) | |
| [Remove a PDF password](#remove-pdf-password) | |
| [Redact a PDF](#redact-pdf) | |
| [Convert PDF to PDF/A](#convert-to-pdfa) | [ConvertPdfToPdfA](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToPdfA/Program.cs) |
| [Remove PDF metadata](#remove-metadata) | |
| [Extract tables from PDF to Excel](#extract-tables-from-pdf) | [ConvertPdfToXlsx](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToXlsx/Program.cs) |
| [Extract images from PDF](#extract-images-from-pdf) | |
| [Extract PDF form data](#extract-form-data) | |
| [Extract email attachments](#extract-email-attachments) | |
| [Generate a document from a template](#generate-document-from-template) | |
| [Compare Word documents](#compare-documents) | |
| [Create a ZIP archive](#create-zip-archive) | [Workflow](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/Workflow/Program.cs) |
| [Save, stream and delete result files](#work-with-the-result) | [DeleteFiles](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/DeleteFiles/Program.cs) |
| [Handle errors and retries](#handle-errors) | [ExceptionHandling](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ExceptionHandling/Program.cs), [HandleRetry](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/HandleRetry/Program.cs) |
| [Check your remaining conversions](#user-information) | [RetrieveUserInformation](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/RetrieveUserInformation/Program.cs) |

For every supported format and conversion, see the [ConvertAPI API reference](https://www.convertapi.com/api).

## Usage

The conversion snippets below are complete programs: paste one into `Program.cs`, put the input file in the project folder, and run `dotnet run`. Results are saved in the `output` folder. Visual Studio runs the program in the working directory set in the project's debug launch profile; by default that is the build output folder, such as `bin\Debug\net8.0`. Put the input file there, or use its full path. The `output` folder is created there too.

### Set the API token

Every request needs an API token. [Get your API token](https://www.convertapi.com/a/signup?ReturnUrl=%2Fa%2Fauthentication&utm_source=github&utm_medium=referral&utm_campaign=dotnet-sdk&utm_content=readme): new users start a free trial, and existing customers who sign in land on their API tokens.

Keep the token out of your source code. The snippets read it from the `CONVERTAPI_TOKEN` environment variable:

```csharp
var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
```

### Set the conversion location (optional)

By default, each request goes to the nearest ConvertAPI server. To keep files in one region, for example in the EU, set the API address once, before the first conversion. All addresses are listed in [server locations](https://www.convertapi.com/docs/servers-location).

```csharp
ConvertApi.ApiBaseUri = "https://eu-v2.convertapi.com";
```

### Convert Word (DOCX) to PDF in C#

Convert a Word document to PDF. For a DOC file, keep `"docx"` and pass the DOC file name.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("docx", "pdf",
    new ConvertApiFileParam("document.docx"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

The PDF is saved as `output/document.pdf`.

### Convert HTML to PDF in C#

Convert an HTML file to PDF. Only the HTML file is uploaded, so link images and stylesheets by absolute URL or put them inline.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("html", "pdf",
    new ConvertApiFileParam("page.html"),
    new ConvertApiParam("PageSize", "a4"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

Convert an HTML string, such as an invoice your app builds, to PDF:

```csharp
using System.Text;
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var html = "<h1>Invoice 1001</h1><p>Total: 99.00 USD</p>";
var result = await convertApi.ConvertAsync("html", "pdf",
    new ConvertApiFileParam(new MemoryStream(Encoding.UTF8.GetBytes(html)), "invoice.html"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

Convert a web page to PDF by its URL:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("html", "pdf",
    new ConvertApiParam("Url", "https://www.example.com"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Convert Excel to PDF in C#

Convert an Excel workbook (XLSX or XLS) to PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("xlsx", "pdf",
    new ConvertApiFileParam("spreadsheet.xlsx"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Convert PDF to Word in C#

Convert a PDF to an editable Word document (DOCX):

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "docx",
    new ConvertApiFileParam("document.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Convert PDF to JPG or PNG in C#

Convert every page of a PDF to a JPG image. For PNG images, use `"png"` in place of `"jpg"`.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "jpg",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("ImageResolution", 150));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

Each page becomes one image: `document.jpg` for the first page, then `document-2.jpg`, `document-3.jpg` and so on. To make a thumbnail of the first page, see [additional conversion parameters](#additional-conversion-parameters).

### Extract text from PDF in C#

Extract the text of a PDF into a string. Scanned pages are read with OCR when needed.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "txt",
    new ConvertApiFileParam("document.pdf"));
using var reader = new StreamReader(await result.Files[0].FileStreamAsync());
var text = await reader.ReadToEndAsync();
Console.WriteLine(text);
```

### Compress PDF in C#

Make a PDF smaller. The `Preset` parameter accepts `lossless`, `text`, `archive`, `web`, `ebook` and `printer`.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "compress",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("Preset", "web"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Convert HTML to DOCX in C#

Convert an HTML file to an editable Word document:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("html", "docx",
    new ConvertApiFileParam("page.html"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### More conversions and PDF tools

#### PowerPoint to PDF

Convert a PowerPoint presentation (PPTX or PPT) to PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pptx", "pdf",
    new ConvertApiFileParam("presentation.pptx"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Image to PDF

Convert a JPG image to PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("jpg", "pdf",
    new ConvertApiFileParam("image.jpg"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### PDF to PowerPoint

Convert a PDF to a PowerPoint presentation (PPTX):

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "pptx",
    new ConvertApiFileParam("document.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### eBook conversion

Convert an EPUB eBook to PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("epub", "pdf",
    new ConvertApiFileParam("book.epub"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Email conversion

Convert an EML email to PDF. For an Outlook MSG file, keep `"eml"` and pass the MSG file name.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("eml", "pdf",
    new ConvertApiFileParam("email.eml"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Merge documents

Merge several PDF files into one PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "merge",
    new ConvertApiFileParam("first.pdf"),
    new ConvertApiFileParam("second.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Split PDF

Split a PDF into one file per page:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "split",
    new ConvertApiFileParam("document.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Rotate pages

Rotate the pages of a PDF by 90 degrees:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "rotate",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("Angle", 90));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Delete pages

Delete pages 2 and 4 from a PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "delete-pages",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("PageRange", "2,4"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Add watermark

Add a text watermark to a PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "text-watermark",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("Text", "Confidential"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Flatten PDF

Flatten a PDF and its form fields, so they can no longer be edited:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "flatten",
    new ConvertApiFileParam("form.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Repair PDF

Repair a damaged PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "repair",
    new ConvertApiFileParam("damaged.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Rasterize PDF

Convert a vector PDF into a raster PDF, with every page stored as an image:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "rasterize",
    new ConvertApiFileParam("document.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Password protect PDF

Protect a PDF with a password that is needed to open it:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "protect",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("UserPassword", "secure123"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Remove PDF password

Remove the password from a protected PDF:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "unprotect",
    new ConvertApiFileParam("protected.pdf"),
    new ConvertApiParam("Password", "secure123"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Redact PDF

Find and redact sensitive data in a PDF with AI, here with the `gdpr` preset:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "redact",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("Preset", "gdpr"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Convert to PDF/A

Convert a PDF to PDF/A-2b for long-term archiving:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "pdfa",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("PdfaVersion", "pdfA2b"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Remove metadata

Remove the title, subject, author, creator and keywords from a PDF. A space in single quotes clears a field.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "pdf",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("PdfTitle", "' '"),
    new ConvertApiParam("PdfSubject", "' '"),
    new ConvertApiParam("PdfAuthor", "' '"),
    new ConvertApiParam("PdfCreator", "' '"),
    new ConvertApiParam("PdfKeywords", "' '"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Extract tables from PDF

Extract the tables of a PDF into an Excel workbook (XLSX):

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "xlsx",
    new ConvertApiFileParam("document.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Extract images from PDF

Save the images inside a PDF as PNG files:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "extract-images",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("ImageOutputFormat", "png"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Extract form data

Export the form field values of a PDF to an FDF file:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "fdf-extract",
    new ConvertApiFileParam("form.pdf"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Extract email attachments

Save the attachments of an EML or MSG email:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("email", "extract",
    new ConvertApiFileParam("email.eml"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Generate document from template

Fill a Word template with JSON data and save the result as PDF. Here `template.docx` contains the placeholder `{{name}}`.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("template", "pdf",
    new ConvertApiFileParam("template.docx"),
    new ConvertApiParam("BindingMethod", "placeholders"),
    new ConvertApiParam("JsonPayload", "[{\"Name\": \"name\", \"Value\": \"John\", \"Type\": \"string\"}]"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Compare documents

Compare two Word documents. The result is a DOCX that marks the differences as tracked changes.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("docx", "compare",
    new ConvertApiFileParam("original.docx"),
    new ConvertApiFileParam("CompareFile", "revised.docx"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

#### Create ZIP archive

Put several files into one ZIP archive:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("file", "zip",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiFileParam("document.docx"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Work with the result

`ConvertAsync` returns the conversion cost and the result files. Each file is a `ConvertApiFile` with a name, a size and a download URL. Save the files, read them as streams, and delete them from the server when you are done:

```csharp
using ConvertApiDotNet;
using ConvertApiDotNet.Model;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "jpg",
    new ConvertApiFileParam("document.pdf"));

Console.WriteLine($"Conversion cost: {result.ConversionCost}");
ConvertApiFile[] files = result.Files;
foreach (var file in files)
    Console.WriteLine($"{file.FileName}: {file.FileSize} bytes");

// Save every file to a folder, or one file under a name you choose
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
await files[0].SaveFileAsync(Path.Combine("output", "first-page.jpg"));

// Read a file as a stream, for example to upload it elsewhere
using var stream = await files[0].FileStreamAsync();

// Delete the uploaded and the result files from the server now, not after 3 hours
await result.DeleteFilesAsync();
```

### Convert a file from a URL

Pass a `Uri` to convert a file that is already online, without downloading it first:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pptx", "pdf",
    new ConvertApiFileParam(new Uri("https://cdn.convertapi.com/cara/testfiles/presentation.pptx")));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Additional conversion parameters

Each conversion accepts extra parameters, listed on its page in the [API reference](https://www.convertapi.com/api), for example [PDF to JPG](https://www.convertapi.com/pdf-to-jpg). This program makes a thumbnail of the first page of a PDF that fits in 300 by 300 pixels:

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var result = await convertApi.ConvertAsync("pdf", "jpg",
    new ConvertApiFileParam("document.pdf"),
    new ConvertApiParam("PageRange", "1"),
    new ConvertApiParam("ImageWidth", 300),
    new ConvertApiParam("ImageHeight", 300),
    new ConvertApiParam("FitToSize", "true"));
Directory.CreateDirectory("output");
await result.SaveFilesAsync("output");
```

### Handle errors

When the API returns an error, the SDK throws a `ConvertApiException`. Its `StatusCode` says what went wrong:

- **401 Unauthorized:** the API token is missing or invalid. Check `CONVERTAPI_TOKEN`, or [get your token](#set-the-api-token).
- **403 Forbidden:** the account has no conversions left, or it is suspended.
- **503 Service Unavailable:** the service is temporarily unavailable. Wait a few seconds and retry, as the [HandleRetry](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/HandleRetry/Program.cs) example does.

For other codes, `Response` holds the error message from the API. If `CONVERTAPI_TOKEN` is not set, the `ConvertApi` constructor throws an `ArgumentNullException` before any request is sent.

```csharp
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
try
{
    var result = await convertApi.ConvertAsync("docx", "pdf",
        new ConvertApiFileParam("document.docx"));
    Directory.CreateDirectory("output");
    await result.SaveFilesAsync("output");
}
catch (ConvertApiException e)
{
    Console.WriteLine($"ConvertAPI returned {(int)e.StatusCode} {e.StatusCode}: {e.Response}");
}
```

### User information

Check how many conversions your account has used. See [user information](https://www.convertapi.com/docs/user-information) for all fields.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));
var user = await convertApi.GetUserAsync();
Console.WriteLine($"Conversions used: {user.ConversionsConsumed} of {user.ConversionsTotal}");
```

### More examples

Each example is a complete console project in the `Examples` folder of this repository. To run one, set the `CONVERTAPI_TOKEN` environment variable and follow the steps in [Examples/README.md](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/README.md).

- [ConvertPdfToDocx/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToDocx/Program.cs) - Convert a PDF to an editable Word DOCX document
- [ConvertPdfToPdfA/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToPdfA/Program.cs) - Convert a PDF to PDF/A with an embedded invoice XML
- [ConvertPdfToPptx/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToPptx/Program.cs) - Convert a PDF to a PowerPoint PPTX presentation
- [ConvertPdfToXlsx/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertPdfToXlsx/Program.cs) - Convert a PDF to an Excel XLSX spreadsheet
- [ConvertRemoteFile/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertRemoteFile/Program.cs) - Convert a remote file (a PPTX at a URL) to PDF
- [ConvertUrlToPdf/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertUrlToPdf/Program.cs) - Convert a web page URL to PDF
- [ConvertWordToPdfAndPng/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConvertWordToPdfAndPng/Program.cs) - Convert one Word DOCX file to PDF and to PNG without uploading it twice
- [ConverterCatalogDemo/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ConverterCatalogDemo/Program.cs) - List the available converters and their details with the converter catalog
- [CreatePdfThumbnail/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/CreatePdfThumbnail/Program.cs) - Turn the first page of a PDF into a JPG thumbnail
- [DeleteFiles/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/DeleteFiles/Program.cs) - Delete the uploaded and result files from the server after a conversion
- [ExceptionHandling/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/ExceptionHandling/Program.cs) - Catch a ConvertAPI exception and print its status code and response
- [FromStreamToStream/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/FromStreamToStream/Program.cs) - Convert HTML from a string stream and get the PDF back as a stream
- [GetConverterInfoDemo/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/GetConverterInfoDemo/Program.cs) - Read a converter's accepted formats and parameters, for example to build a dynamic UI
- [HandleRetry/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/HandleRetry/Program.cs) - Retry a conversion when the service returns 503 (service unavailable)
- [MergeWordDocsToPdf/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/MergeWordDocsToPdf/Program.cs) - Convert Word and PowerPoint files to PDF and merge them into one PDF
- [RetrieveUserInformation/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/RetrieveUserInformation/Program.cs) - Retrieve your ConvertAPI account information
- [SplitAndMergePdf/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/SplitAndMergePdf/Program.cs) - Split a PDF and merge its first and last pages into a new PDF
- [SplitPDF/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/SplitPDF/Program.cs) - Split a PDF by bookmarks, page patterns, text and page ranges
- [UploadOnceReuseTwice/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/UploadOnceReuseTwice/Program.cs) - Upload a DOCX file once and reuse it for PDF and PNG conversions
- [WatermarkPdf/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/WatermarkPdf/Program.cs) - Add a text watermark and a PDF overlay to a PDF
- [Workflow/Program.cs](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/Workflow/Program.cs) - Chain two conversions: PDF to JPG images, then the images to a ZIP

## Compatibility

The package targets .NET Standard 2.0, so it runs on:

- .NET 5 to .NET 10 and later, and .NET Core 2.0 or later
- .NET Framework 4.6.2 or later (4.7.2 or later is recommended, because older versions cannot use .NET Standard 2.0 packages reliably)
- Libraries that target .NET Standard 2.0 or later
- Windows, Linux and macOS, including Azure App Service and Azure Functions
- Console, web and desktop applications

## Security, data and support

- Uploaded and converted files are stored for at most 3 hours, then deleted automatically. To delete them as soon as you have the result, call `await result.DeleteFilesAsync();`, as the [DeleteFiles](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/Examples/DeleteFiles/Program.cs) example does.
- Data is always encrypted at rest and in transit.
- To keep processing in one region, such as the EU, [set the conversion location](#set-the-conversion-location-optional). All locations are listed in [server locations](https://www.convertapi.com/docs/servers-location).
- ISO 27001, SOC 2, GDPR and HIPAA: see [ConvertAPI security and compliance](https://www.convertapi.com/compliance). Live availability: [ConvertAPI status](https://status.convertapi.com).
- Bugs and requests for this SDK: [GitHub issues](https://github.com/ConvertAPI/convertapi-library-dotnet/issues). Questions about the API or your account: [ConvertAPI support](https://www.convertapi.com/support).

## License

The ConvertAPI .NET library is licensed under the [MIT license](https://opensource.org/license/mit). See the [LICENSE](https://github.com/ConvertAPI/convertapi-library-dotnet/blob/main/LICENSE) file for more information.
