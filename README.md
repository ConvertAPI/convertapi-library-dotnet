# ConvertAPI C# Client
## Overview

The ConvertAPI library provides a simple and efficient way to integrate the [ConvertAPI](https://www.convertapi.com) service into your .NET Framework, .NET Core, and .NET projects. ConvertAPI enables you to easily convert various file formats by leveraging its robust API. Whether you need to convert documents, images, spreadsheets, or other file types, the ConvertAPI library streamlines the process with minimal code and maximum efficiency.

## Features

Below is an overview of the key functionalities:

### Document Conversion

* **Office to PDF**: Convert Word (DOC, DOCX), Excel (XLS, XLSX), and PowerPoint (PPT, PPTX) documents to PDF or PDF/A formats.
* **PDF to Office**: Transform PDFs into editable Word, Excel, and PowerPoint documents.
* **HTML/Web to PDF**: Convert HTML files or web pages to PDF using a headless Chrome browser.
* **Image to PDF**: Convert various image formats (e.g., JPG, PNG, BMP) to PDF.
* **PDF to Image**: Convert PDFs to image formats such as JPG, PNG, TIFF, SVG, and WebP.
* **eBook Conversion**: Convert DJVU and other eBook formats to PDF or images.
* **Email Conversion**: Convert EML and MSG email files to PDF or image formats.

### Document Processing & Transformation

* **Merge & Split PDF**: Combine multiple PDF document into one or split a PDF into several parts.
* **Rotate & Delete Pages**: Rotate pages or remove unwanted pages from PDF.
* **Watermarking**: Add text or image watermarks to PDF and images.
* **Flatten PDFs**: Flatten PDF layers to prevent further editing.
* **Repair Documents**: Recover corrupted or damaged PDF and DOCX files.
* **Rasterize PDFs**: Convert vector PDFs into raster images.

### Security & Optimization

* **Password Protection**: Encrypt PDF and Office documents with passwords and AES-256 encryption.
* **Remove Protection**: Unlock password-protected PDFs.
* **Redact PDF**: Automatically detect and redact sensitive information in PDF files using AI.
* **Compress PDF**: Reduce PDF sizes by up to 90% without compromising quality.
* **PDF/A Conversion**: Convert PDFs to PDF/A format for long-term archiving.
* **Metadata Management**: Edit or remove metadata from PDF documents.

### Data Extraction & OCR

* **Text Extraction**: Extract text content from PDFs, with optional OCR for scanned documents.
* **Table Extraction**: Extract tabular data from PDFs into CSV or Excel formats.
* **Image Extraction**: Extract images from PDFs into various image formats.
* **Form Data Extraction**: Extract form field data from PDFs into FDF format.
* **Email Attachments**: Extract attachments from EML and MSG email files.

### Advanced Tools

* **Document Generation**: Create DOCX or PDF documents dynamically using templates and JSON data.
* **Format Comparison**: Compare DOCX documents to identify differences in content and formatting.
* **ZIP Archiving**: Create ZIP archives from multiple files, with optional password protection.
* **Asynchronous Processing**: Handle large or time-consuming conversions asynchronously.

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

```csharp
ConvertApiResponse result = await convertApi.ConvertAsync("docx", "pdf",
   new ConvertApiFileParam(@"c:\source\test.docx")   
);

// save to file
 var fileInfo = await result.SaveFileAsync(@"\result\test.pdf");
```

Other result operations:

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
