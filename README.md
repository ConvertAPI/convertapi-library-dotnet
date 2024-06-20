# ConvertAPI NuGet Package
## Overview

The ConvertAPI NuGet package provides a simple and efficient way to integrate the [ConvertAPI](https://www.convertapi.com) service into your .NET applications. ConvertAPI enables you to easily convert various file formats by leveraging its robust API. Whether you need to convert documents, images, spreadsheets, or other file types, the ConvertAPI NuGet package streamlines the process with minimal code and maximum efficiency.

## Features

- **Wide Range of Conversions:** Convert documents, images, spreadsheets, and more between numerous formats, including PDF, DOCX, JPG, PNG, XLSX, PPTX, HTML, CSV, TXT, and others. Perform specialized PDF manipulations such as merging, encrypting, splitting, repairing, and decrypting PDF files. Key conversions include Office to PDF, PDF to Word, PDF to PowerPoint, and PDF to Excel.
- **Ease of Integration:** Simple and intuitive API that allows quick setup and integration into your .NET applications.
- **Asynchronous Support:** Perform conversions asynchronously to ensure your application remains responsive.
- **Customizable Options:** Configure various conversion options to tailor the output to your specific needs.
- **Reliable and Secure:** Built on ConvertAPI's robust infrastructure, ensuring reliable and secure file conversions.

All supported file conversions and manipulations can be found at [ConvertAPI API](https://www.convertapi.com/api).

## Installation

Install the ConvertAPI package via NuGet Package Manager:

```sh
Install-Package ConvertApi
```

Or use the .NET CLI:

```sh
dotnet add package ConvertApi
```

## Getting Started

1. **Set up ConvertAPI:**
   Sign up for a ConvertAPI account and obtain your API secret.

2. **Configure the API client:**
   Initialize the API client in your .NET application with your secret key.

```csharp
using ConvertApiDotNet;

var convertApi = new ConvertApi("your-api-secret");
```

3. **Perform a conversion:**
   Use the client to convert files by specifying the source and target formats.

```csharp
var convertApi = new ConvertApi("your-api-secret");
var convertResult = await convertApi.ConvertAsync("pdf", "docx", new ConvertApiFileParam("file", "path/to/your/file.pdf"));

var convertedFile = convertResult.Files[0];
await convertedFile.SaveFileAsync("path/to/save/converted/file.docx");
```

## Documentation

For more detailed documentation, visit the [ConvertAPI Documentation](https://www.convertapi.com/doc).

## Support

If you encounter any issues or have questions, please visit the [ConvertAPI Support](https://www.convertapi.com/support) page for assistance.

## License

The ConvertAPI NuGet package is licensed under the [MIT License](https://opensource.org/licenses/MIT).

Integrate ConvertAPI into your .NET applications today and enjoy seamless and efficient file conversions with minimal effort!