#### 1. Install the ConvertAPI library from NuGet

```
PM> Install-Package ConvertApi
```

#### 2.a. Convert local file

```csharp
//Import
using ConvertApiDotNet;

//Convert Word document
const string sourceFile = @"c:\test.docx";

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your api secret");

//Set input and output formats and pass file parameter. 
//Word to PDF API. Read more https://www.convertapi.com/docx-to-pdf
var convertToPdf = convertApi.ConvertAsync("docx", "pdf", new ConvertApiFileParam(sourceFile));
//Save PDF file 
convertToPdf.Result.SaveFiles(@"c:\output");
```

#### 2.b. Convert remote file and set additional parameters

```csharp
//Import
using ConvertApiDotNet;

//Convert PowerPoint document
var sourceFile = new Uri("https://github.com/Baltsoft/CDN/raw/master/cara/testfiles/presentation2.pptx");

//Get your secret at https://www.convertapi.com/a
var convertApi = new ConvertApi("your api secret");

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
