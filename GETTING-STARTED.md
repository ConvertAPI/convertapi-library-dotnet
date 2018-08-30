#### 1. Install the ConvertAPI library from NuGet

```
PM> Install-Package ConvertApi
```

#### 2.a. Simple conversion methods

```csharp
//Import
using ConvertApiDotNet;

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Excel to PDF API https://www.convertapi.com/xlsx-to-pdf
convertApi.ConvertFile(@"c:\test.xlsx", @"c:\sheet.pdf"));

//Web to PDF API https://www.convertapi.com/web-to-pdf
convertApi.ConvertUrl("https://www.google.com", @"c:\google.pdf"));

//Remote Word to PDF API https://www.convertapi.com/docx-to-pdf
convertApi.ConvertRemoteFile("https://cdn.convertapi.com/cara/testfiles/document.docx", @"c:\document.pdf"));
```

#### 2.b. Convert local file

```csharp
//Import
using ConvertApiDotNet;

//Convert Word document
const string sourceFile = @"c:\test.docx";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Set input and output formats and pass file parameter. 
//Word to PDF API. Read more https://www.convertapi.com/docx-to-pdf
var convertToPdf = convertApi.ConvertAsync("docx", "pdf", new ConvertApiFileParam(sourceFile));
//Save PDF file 
convertToPdf.Result.SaveFiles(@"c:\output");
```

#### 2.c. Convert remote file and set additional parameters

```csharp
//Import
using ConvertApiDotNet;

//Convert PowerPoint document
var sourceFile = new Uri("https://github.com/Baltsoft/CDN/raw/master/cara/testfiles/presentation2.pptx");

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Set input and output formats and pass file parameter. 
//PowerPoint to PNG API. Read more https://www.convertapi.com/pptx-to-png
var createThumbnails = convertApi.ConvertAsync("pptx", "png", 
    new ConvertApiFileParam(sourceFile)
    new ConvertApiParam("ScaleImage", "true"),
    new ConvertApiParam("ScaleProportions", "true"),
    new ConvertApiParam("ImageHeight", "500"),
    new ConvertApiParam("ImageWidth", "500")
);
//Save PNG files
createThumbnails.Result.SaveFiles(@"c:\output");
```

#### 2.d. Convert from a stream

```csharp
//Import
using ConvertApiDotNet;

//Convert html code
const string htmlString = "<!DOCTYPE html><html><body><h1>My First Heading</h1><p>My first paragraph.</p></body></html>";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Pass as stream
var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlString));

//Html to PDF API. Read more https://www.convertapi.com/html-to-pdf
var convertToPdf = convertApi.ConvertAsync("html", "pdf", 
    new ConvertApiFileParam(stream, "test.html")
);

//PDF as stream
var outputStream = convertToPdf.Result.FileStream();
```

#### 2.e. Conversions chaining

```csharp
//Import
using ConvertApiDotNet;

//Split PDF document and merge first and last pages to new PDF
const string sourceFile = @"c:\test.pdf";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Set input and output formats and pass file parameter. 
//Split PDF API. Read more https://www.convertapi.com/pdf-to-split
var splitTask = convertApi.ConvertAsync("pdf", "split",
    new ConvertApiFileParam(sourceFile));

//Get result of the first chain and move it to Merge conversion. 
//Chains are executed on server without moving files.
//Merge PDF API. Read more https://www.convertapi.com/pdf-to-merge
var mergeTask = convertApi.ConvertAsync("pdf", "merge", 
    new ConvertApiFileParam(splitTask.Result.Files.First()), 
    new ConvertApiFileParam(splitTask.Result.Files.Last()));

var saveFiles = mergeTask.Result.SaveFile("c:\merged-pdf.pdf");
```

#### 3. Read account status

```csharp
//Import
using ConvertApiDotNet;

//Convert Word document
const string sourceFile = @"c:\test.docx";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

//Read full account data
var user = convertApi.GetUserAsync().Result;

//Find out how much seconds left
var secondsLeft = user.SecondsLeft;
```

#### 4. Exception handling (asynchronous)

```csharp
//Import
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

//Convert PDF document
const string sourceFile = @"c:\test.pdf";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your-api-secret");

try
{
    //PDF to Powerpoint API. Read more https://www.convertapi.com/pdf-to-pptx
    //Set incorect value for parameter AutoRotate and get exception
    var convertToPdf = convertApi.ConvertAsync("pdf", "pptx", 
        new ConvertApiFileParam(sourceFile),
        new ConvertApiParam("AutoRotate","WrongParameter")
        );
     var outputFileName = convertToPdf.Result.Files[0];
}
//Catch exceptions from asynchronous methods
catch (AggregateException e)
{                                
    //Read exception status code
    Console.WriteLine("Status Code: " + (e.InnerException as ConvertApiException)?.StatusCode);
    //Read exception detailed description
    Console.WriteLine("Response: " + (e.InnerException as ConvertApiException)?.Response);
}
```

#### 5. Supported file formats, conversions and actions

https://www.convertapi.com/doc/supported-formats

#### 6. GitHub 

https://github.com/ConvertAPI/convertapi-dotnet

