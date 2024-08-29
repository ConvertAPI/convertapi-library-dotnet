# ConvertAPI C# Client
## Overview

The ConvertAPI library provides a simple and efficient way to integrate the [ConvertAPI](https://www.convertapi.com) service into your .NET Framework, .NET Core, and .NET projects. ConvertAPI enables you to easily convert various file formats by leveraging its robust API. Whether you need to convert documents, images, spreadsheets, or other file types, the ConvertAPI library streamlines the process with minimal code and maximum efficiency.

## Features

- **Wide Range of Conversions:** Convert documents, images, spreadsheets, and more between numerous formats, including PDF, DOCX, JPG, PNG, XLSX, PPTX, HTML, CSV, TXT, and others. Perform specialized PDF manipulations such as merging, encrypting, splitting, repairing, and decrypting PDF files. Key conversions include Office to PDF, PDF to Word, PDF to PowerPoint, and PDF to Excel.
- **Ease of Integration:** Simple and intuitive API that allows quick setup and integration into your .NET applications.
- **Asynchronous Support:** Perform conversions asynchronously to ensure your application remains responsive.
- **Customizable Options:** Configure various conversion options to tailor the output to your specific needs.
- **Reliable and Secure:** Built on ConvertAPI's robust infrastructure, ensuring reliable and secure file conversions.

All supported file conversions and manipulations can be found at [ConvertAPI API](https://www.convertapi.com/api).

## Installation

Run this line from Package Manager Console:

```csharp
Install-Package ConvertApi
```

## Usage

### Configuration

#### Set credentials

You can get your credentials at https://www.convertapi.com/a/auth

```csharp
ConvertApi convertApi = new ConvertApi("your-api-secret");
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
  var convertApi = new ConvertApi("your-api-secret");  
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

This is the minimum to convert a file using the ConvertAPI client, but you can do much more with the ConvertAPI .NET library. Note that you should replace `your-api-secret` with the secret you obtained in item two of the prerequisites.

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI .NET Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refere to the [LICENSE](https://github.com/ConvertAPI/convertapi-dotnet/blob/master/LICENSE) file for more information.
