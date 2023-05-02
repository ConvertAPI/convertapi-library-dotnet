# ConvertAPI C# Client
## Convert your files with our online file conversion API

ConvertAPI is a convenient REST API that simplifies the process of converting different file formats. You can easily create PDFs and images from sources like Word, Excel, PowerPoint, images, web pages, or raw HTML codes. Furthermore, it enables you to merge, encrypt, split, repair, and decrypt PDF files, along with other file manipulation options. With ConvertAPI, integrating it into your application takes only a few minutes, making it developer-friendly.

The ConvertAPI.NET NuGet package simplifies the usage of the Convert API in your .NET Framework, .NET Core, and .NET projects. You don't need to create your own API calls.

## Installation

Run this line from Package Manager Console:

```csharp
Install-Package ConvertApi
```

## Usage

### Configuration

#### Set credentials

You can get your secret at https://www.convertapi.com/a

```csharp
ConvertApi convertApi = new ConvertApi("your-api-secret");
```

#### Set conversion location (optional)

There are several conversion locations available, and if you want to comply with GDPR, you can choose the EU API location. However, this option is not mandatory as ConvertAPI uses GEO DNS to detect the nearest server automatically. More information at https://www.convertapi.com/doc/servers-location

convertApi.ApiBaseUri = "https://v2.convertapi.com";

### File conversion

Example to convert file to PDF. All supported formats and options can be found 
[here](https://www.convertapi.com).

```csharp
ConvertApiResponse result = await convertApi.ConvertAsync("docx", "pdf", new[]
{
   new ConvertApiFileParam(@"c:\source\test.docx")   
});

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
ConvertApiResponse result = await convertApi.ConvertAsync("pptx", "pdf", new[]
{
   new ConvertApiFileParam(new Uri("https://cdn.convertapi.com/cara/testfiles/presentation.pptx"))
});
```

#### Additional conversion parameters

ConvertAPI accepts extra conversion parameters depending on converted formats. All conversion 
parameters and explanations can be found [here](https://www.convertapi.com/conversions).

```csharp
ConvertApiResponse result = await convertApi.ConvertAsync("pdf", "jpg", new[]
{
   new ConvertApiFileParam(@"c:\source\test.docx"),
   new ConvertApiParam("ScaleImage","true"),
   new ConvertApiParam("ScaleProportions","true"),
   new ConvertApiParam("ImageHeight","300"),
   new ConvertApiParam("ImageWidth","300")
});
```

### User information

You can always check remaining seconds amount by fetching [user information](https://www.convertapi.com/doc/user).

```csharp
ConvertApiUser user = await convert.GetUserAsync();
int secondsLeft = user.SecondsLeft;
```

### More examples

You can find more advanced examples in the [examples](https://github.com/ConvertAPI/convertapi-dotnet/tree/master/Examples) folder.

#### Converting your first file, full example:

ConvertAPI is designed to make converting file super easy, the following snippet shows how easy it is to get started. Let's convert WORD DOCX file to PDF:

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
          Console.WriteLine("Secret is not provided or no additional seconds left in account to proceed conversion. More information https://www.convertapi.com/a");
  }
```

This is the bare-minimum to convert a file using the ConvertAPI client, but you can do a great deal more with the ConvertAPI .NET library. Take special note that you should replace `your-api-secret` with the secret you obtained in item two of the pre-requisites.

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI .NET Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refere to the [LICENSE](https://github.com/ConvertAPI/convertapi-dotnet/blob/master/LICENSE) file for more information.
