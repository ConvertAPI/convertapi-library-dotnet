# ConvertAPI C# Client
## Overview

The ConvertAPI library provides a simple and efficient way to integrate the [ConvertAPI](https://www.convertapi.com) service into your .NET Framework, .NET Core, and .NET projects. ConvertAPI enables you to easily convert various file formats by leveraging its robust API. Whether you need to convert documents, images, spreadsheets, or other file types, the ConvertAPI library streamlines the process with minimal code and maximum efficiency.

## Features

Below is an overview of the key functionalities:

### Document Conversion

* **Office to PDF**: Convert Word (DOC, DOCX), Excel (XLS, XLSX), and PowerPoint (PPT, PPTX) documents to PDF or PDF/A formats. [Example](#office-to-pdf)
* **PDF to Office**: Transform PDFs into editable Word, Excel, and PowerPoint documents. [Example](#pdf-to-office)
* **HTML/Web to PDF**: Convert HTML files or web pages to PDF using a headless Chrome browser. [Example](#htmlweb-to-pdf)
* **Image to PDF**: Convert various image formats (e.g., JPG, PNG, BMP) to PDF. [Example](#image-to-pdf)
* **PDF to Image**: Convert PDFs to image formats such as JPG, PNG, TIFF, SVG, and WebP. [Example](#pdf-to-image)
* **eBook Conversion**: Convert DJVU and other eBook formats to PDF or images. [Example](#ebook-conversion)
* **Email Conversion**: Convert EML and MSG email files to PDF or image formats. [Example](#email-conversion)

### Document Processing & Transformation

* **Merge PDF**: Combine multiple PDF documents into one PDF. [Example](#merge-documents)
* **Split PDF**: Split PDF into several parts, by page ranges, patterns, bookmarks and etc. [Example](#split-pdf)
* **Rotate**: Rotate pages in PDF. [Example](#rotate-pages)
* **Delete Pages**: Remove unwanted pages from PDF. [Example](#delete-pages)
* **Watermarking**: Add text or image watermarks to PDF and images. [Example](#add-watermark)
* **Flatten PDFs**: Flatten PDF layers to prevent further editing. [Example](#flatten-pdf)
* **Repair Documents**: Recover corrupted or damaged PDF and DOCX files. [Example](#repair-pdf)
* **Rasterize PDFs**: Convert vector PDFs into raster images. [Example](#rasterize-pdf)

### Security & Optimization

* **Password Protection**: Encrypt PDF and Office documents with passwords and AES-256 encryption. [Example](#password-protect-pdf)
* **Remove Protection**: Unlock password-protected PDFs. [Example](#remove-pdf-password)
* **Redact PDF**: Automatically detect and redact sensitive information in PDF files using AI. [Example](#redact-pdf)
* **Compress PDF**: Reduce PDF sizes by up to 90% without compromising quality. [Example](#compress-pdf)
* **PDF/A Conversion**: Convert PDFs to PDF/A format for long-term archiving. [Example](#convert-to-pdfa)
* **Metadata Management**: Edit or remove metadata from PDF documents. [Example](#remove-metadata)

### Data Extraction & OCR

* **Text Extraction**: Extract text content from PDFs, with optional OCR for scanned documents. [Example](#extract-text-from-pdf)
* **Table Extraction**: Extract tabular data from PDFs into CSV or Excel formats. [Example](#extract-tables-from-pdf)
* **Image Extraction**: Extract images from PDFs into various image formats. [Example](#extract-images-from-pdf)
* **Form Data Extraction**: Extract form field data from PDFs into FDF format. [Example](#extract-form-data)
* **Email Attachments**: Extract attachments from EML and MSG email files. [Example](#extract-email-attachments)

### Advanced Tools

* **Document Generation**: Create DOCX or PDF documents dynamically using templates and JSON data. [Example](#generate-document-from-template)
* **Format Comparison**: Compare DOCX documents to identify differences in content and formatting. [Example](#compare-documents)
* **ZIP Archiving**: Create ZIP archives from multiple files, with optional password protection. [Example](#create-zip-archive)

For a complete list of supported conversions and tools, visit the [ConvertAPI API Reference](https://www.convertapi.com/api).

## Compatibility
The ConvertAPI .NET library is compatible with a wide range of platforms that support .NET Standard 2.0+ or .NET Framework 4.0+. Supported environments include:

- .NET Core 5, 6, 7, 8, 9, and above
- .NET Standard 2.0 and above
- .NET Framework 4.0, 4.6.1, 4.7.2, 4.8.1 and above
- Windows, Linux, macOS
- Microsoft Azure, Azure App Service, Azure Functions
- Xamarin (iOS, macOS, Android)
- Universal Windows Platform (UWP)
- Web, Console, and Desktop applications

## Installation

Run this line from Package Manager Console:

```csharp
Install-Package ConvertApi
```

## Usage

### Configuration

#### Set credentials

You can get your credentials at https://www.convertapi.com/a/authentication

```csharp
ConvertApi convertApi = new ConvertApi("api_token");
```

#### Set conversion location (optional)

You can choose from multiple conversion locations, including the EU API location for GDPR compliance. However, this selection is optional as ConvertAPI automatically detects the nearest server using GEO DNS. For more details, visit [ConvertAPI Servers Location](https://www.convertapi.com/doc/servers-location).

```csharp
ConvertApi.ApiBaseUri = "https://v2.convertapi.com";
```

### File conversion

Example to convert file to PDF. All supported formats and options can be found 
[here](https://www.convertapi.com/api).

#### Office to PDF

Convert Word, Excel, and PowerPoint documents to PDF:

```csharp
var result = await convertApi.ConvertAsync("docx", "pdf",
    new ConvertApiFileParam(@"C:\files\document.docx"));
await result.SaveFilesAsync(@"C:\output\");
```

#### PDF to Office

Convert PDF to editable Word, Excel, or PowerPoint formats:

```csharp
var result = await convertApi.ConvertAsync("pdf", "docx",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### HTML/Web to PDF

Convert HTML files or web pages to PDF:

```csharp
var result = await convertApi.ConvertAsync("web", "pdf",
    new ConvertApiParam("Url", "https://example.com"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Image to PDF

Convert images (e.g., JPG, PNG) to PDF:

```csharp
var result = await convertApi.ConvertAsync("jpg", "pdf",
    new ConvertApiFileParam(@"C:\files\image.jpg"));
await result.SaveFilesAsync(@"C:\output\");
```

#### PDF to Image

Convert PDF to image formats (e.g., JPG, PNG):

```csharp
var result = await convertApi.ConvertAsync("pdf", "jpg",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### eBook Conversion

Convert eBook formats (e.g., EPUB) to PDF:

```csharp
var result = await convertApi.ConvertAsync("epub", "pdf",
    new ConvertApiFileParam(@"C:\files\book.epub"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Email Conversion

Convert EML or MSG email files to PDF:

```csharp
var result = await convertApi.ConvertAsync("eml", "pdf",
    new ConvertApiFileParam(@"C:\files\email.eml"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Merge Documents

Merge multiple PDF files into one:

```csharp
var result = await convertApi.ConvertAsync("pdf", "merge",
    new ConvertApiFileParam(@"C:\files\file1.pdf"),
    new ConvertApiFileParam(@"C:\files\file2.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Split PDF

Split a PDF into individual pages:

```csharp
var result = await convertApi.ConvertAsync("pdf", "split",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Rotate Pages

Rotate pages in a PDF document:

```csharp
var result = await convertApi.ConvertAsync("pdf", "rotate",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("RotatePage", "90"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Delete Pages

Delete specific pages from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "delete-pages",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("PageRange", "2,4"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Add Watermark

Add a text watermark to a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "watermark",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("Text", "Confidential"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Flatten PDF

Flatten a PDF to prevent further editing:

```csharp
var result = await convertApi.ConvertAsync("pdf", "flatten",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Repair PDF

Repair a corrupted PDF file:

```csharp
var result = await convertApi.ConvertAsync("pdf", "repair",
    new ConvertApiFileParam(@"C:\files\corrupted.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Rasterize PDF

Convert a PDF to a rasterized image:

```csharp
var result = await convertApi.ConvertAsync("pdf", "rasterize",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Password Protect PDF

Add password protection to a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "protect",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("UserPassword", "secure123"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Remove PDF Password

Remove password protection from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "unprotect",
    new ConvertApiFileParam(@"C:\files\protected.pdf"),
    new ConvertApiParam("Password", "secure123"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Redact PDF

Redact sensitive information in a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "redact",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("Preset", "gdpr"));   
await result.SaveFilesAsync(@"C:\output\");
```

#### Compress PDF

Compress a PDF file to reduce its size:

```csharp
var result = await convertApi.ConvertAsync("pdf", "compress",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("Preset", "web"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Convert to PDF/A

Convert a PDF to PDF/A format for archiving:

```csharp
var result = await convertApi.ConvertAsync("pdf", "pdfa",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("PdfaVersion", "pdfa2"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Remove Metadata

Remove metadata from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "pdf",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("PdfTitle", "' '"),
    new ConvertApiParam("PdfSubject", "' '"),
    new ConvertApiParam("PdfAuthor", "' '"),
    new ConvertApiParam("PdfCreator", "' '"),
    new ConvertApiParam("PdfKeywords", "' '"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Extract Text from PDF

Extract text content from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "txt",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Extract Tables from PDF

Extract tables from a PDF into Excel:

```csharp
var result = await convertApi.ConvertAsync("pdf", "xlsx",
    new ConvertApiFileParam(@"C:\files\document.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Extract Images from PDF

Extract images from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "extract-images",
    new ConvertApiFileParam(@"C:\files\document.pdf"),
    new ConvertApiParam("ImageOutputFormat", "png"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Extract Form Data

Extract form data from a PDF:

```csharp
var result = await convertApi.ConvertAsync("pdf", "fdf-extract",
    new ConvertApiFileParam(@"C:\files\form.pdf"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Extract Email Attachments

Extract attachments from an email file:

```csharp
var result = await convertApi.ConvertAsync("email", "extract",
    new ConvertApiFileParam(@"C:\files\email.eml"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Generate Document from Template

Generate a document from a template using JSON data:

```csharp
var result = await convertApi.ConvertAsync("template", "pdf",
    new ConvertApiFileParam(@"C:\templates\template.docx"),
    new ConvertApiParam("BindingMethod", "placeholders"),
    new ConvertApiParam("JsonPayload", "[{\"Name\": \"name\", \"Value\": \"John\", \"Type\": \"string\"}]"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Compare Documents

Compare two DOCX documents:

```csharp
var result = await convertApi.ConvertAsync("docx", "compare",
    new ConvertApiFileParam(@"C:\files\doc1.docx"),
    new ConvertApiFileParam("CompareFile", @"C:\files\doc2.docx"));
await result.SaveFilesAsync(@"C:\output\");
```

#### Create ZIP Archive

Create a ZIP archive from multiple files:

```csharp
var result = await convertApi.ConvertAsync("any", "zip",
    new ConvertApiFileParam(@"C:\files\file1.pdf"),
    new ConvertApiFileParam(@"C:\files\file2.docx"));
await result.SaveFilesAsync(@"C:\output\");
```

### Other result operations:

```csharp
// save all result files to folder
result.SaveFilesAsync(@"\result\");

// get result files
ProcessedFile[] files = result.Files;

// get conversion cost
int cost = result.ConversionCost; 
```

#### Convert file url

```csharp
ConvertApiResponse result = await convertApi.ConvertAsync("pptx", "pdf",
   new ConvertApiFileParam(new Uri("https://cdn.convertapi.com/cara/testfiles/presentation.pptx"))
);
```

#### Additional conversion parameters

ConvertAPI accepts extra conversion parameters depending on converted formats. All conversion 
parameters and explanations can be found [here](https://www.convertapi.com/conversions).

```csharp
ConvertApiResponse result = await convertApi.ConvertAsync("pdf", "jpg",
   new ConvertApiFileParam(@"c:\source\test.pdf"),
   new ConvertApiParam("ScaleImage","true"),
   new ConvertApiParam("ScaleProportions","true"),
   new ConvertApiParam("ImageHeight","300"),
   new ConvertApiParam("ImageWidth","300")
);
```

### User information

You can always check the remaining conversions amount and other account information by fetching [user information](https://www.convertapi.com/doc/user).

```csharp
ConvertApiUser user = await convert.GetUserAsync();
int conversionsTotal = user.ConversionsTotal;
int conversionsConsumed = user.ConversionsConsumed;
```

### More examples

You can find more advanced examples in the [examples](https://github.com/ConvertAPI/convertapi-dotnet/tree/master/Examples) folder.

#### Converting your first file, complete example:

ConvertAPI is designed to make converting files super easy. The following snippet shows how easy it is to get started. Let's convert the WORD DOCX file to PDF:

```csharp
try
{
  var convertApi = new ConvertApi("api_token");  
  var conversionTask = await convertApi.ConvertAsync("docx", "pdf", 
      new ConvertApiFileParam(@"c:\source\test.docx")
  );
  var fileSaved = await conversionTask.Files.SaveFilesAsync(@"c:\");
}
//Catch exceptions from asynchronous methods
catch (ConvertApiException e)
{
  Console.WriteLine("Status Code: " + e.StatusCode);
  Console.WriteLine("Response: " + e.Response);

   if (e.StatusCode == HttpStatusCode.Unauthorized)
       Console.WriteLine("Secret is not provided or no additional seconds left in the account to proceed conversion. More information https://www.convertapi.com/a");
}
```

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI .NET Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refere to the [LICENSE](https://github.com/ConvertAPI/convertapi-dotnet/blob/master/LICENSE) file for more information.
