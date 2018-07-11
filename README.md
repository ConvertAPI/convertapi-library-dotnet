# ConvertAPI C# Client
## Convert your files with our online file conversion API

The ConvertAPI helps converting various file formats. Creating PDF and Images from various sources like Word, Excel, Powerpoint, images, web pages or raw HTML codes. Merge, Encrypt, Split, Repair and Decrypt PDF files. And many others files manipulations. In just few minutes you can integrate it into your application and use it easily.

The ConvertAPI.NET NuGet package makes it easier to use the Convert API from your .NET 2, 3.x, and 4.x projects without having to build your own API calls. You can get your free API secret at https://www.convertapi.com/a

## Installation

Run this line from Package Manager Console:

```csharp
Install-Package ConvertApi
```

## Usage

### Configuration

You can get your secret at https://www.convertapi.com/a

```csharp
ConvertApi convertApi = new ConvertApi("your api secret");
```

### File conversion

Example to convert file to PDF. All supported formats and options can be found 
[here](https://www.convertapi.com).

```csharp
ConvertApiResponse result = convertApi.ConvertAsync("docx", "pdf", new[]
{
   new ConvertApiParam("File", File.OpenRead(@"\source\test.docx"))
}).Result;

// save to file
result.SaveFile(@"\result\test.pdf");
```

Other result operations:

```csharp
// save all result files to folder
result.SaveFiles(@"\result\");

// get result files
ProcessedFile[] files = result.Files;

// get conversion cost
int cost = result.ConversionCost; 
```

#### Convert file url

```csharp
ConvertApiResponse result = convertApi.ConvertAsync("pptx", "pdf", new[]
{
   new ConvertApiParam("File", "https://cdn.convertapi.com/cara/testfiles/presentation.pptx")
}).Result;
```

#### Additional conversion parameters

ConvertAPI accepts extra conversion parameters depending on converted formats. All conversion 
parameters and explanations can be found [here](https://www.convertapi.com).

```csharp
ConvertApiResponse result = convertApi.ConvertAsync("pdf", "jpg", new[]
{
   new ConvertApiParam("File", File.OpenRead(@"\source\test.pdf")),
   new ConvertApiParam("ScaleImage","true"),
   new ConvertApiParam("ScaleProportions","true"),
   new ConvertApiParam("ImageHeight","300"),
   new ConvertApiParam("ImageWidth","300")
}).Result;
```

### User information

You can always check remaining seconds amount by fetching [user information](https://www.convertapi.com/doc/user).

```csharp
ConvertApiUser user = convert.GetUser().Result;
int secondsLeft = user.SecondsLeft;
```

### More examples

You can find more advanced examples in the [examples](https://github.com/ConvertAPI/convertapi-dotnet/tree/master/Examples) folder.

#### Converting your first file, full example:

ConvertAPI is designed to make converting file super easy, the following snippet shows how easy it is to get started. Let's convert WORD DOCX file to PDF:

```csharp
try
{
  
  var convertApi = new ConvertApi("your api secret");
  
  var fileToConvert = @"c:\test.docx";
  var conversionTask = convertApi.ConvertAsync("docx", "pdf", new[]
  {
     new ConvertApiParam("File", File.OpenRead(fileToConvert))
  });
  
  conversionTask.Result.SaveFiles(@"c:\");
  
  }
  //Catch exceptions from asynchronous methods
  catch (Exception e)
  {
    Console.WriteLine($"{e.GetType()}: {e.Message}");
    Console.WriteLine(e.InnerException?.Message);
    var httpStatusCode = (e.InnerException as ConvertApiException)?.StatusCode;
    Console.WriteLine("Status Code: " + httpStatusCode);
    Console.WriteLine("Response: " + (e.InnerException as ConvertApiException)?.Response);

    if (httpStatusCode == HttpStatusCode.Unauthorized)
       Console.WriteLine("Secret is not provided or no additional seconds left in account to proceed conversion. More information https://www.convertapi.com/a");
   }
```

This is the bare-minimum to convert a file using the ConvertAPI client, but you can do a great deal more with the ConvertAPI .NET library. Take special note that you should replace `your api secret` with the secret you obtained in item two of the pre-requisites.

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI .NET Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refere to the [LICENSE](https://github.com/ConvertAPI/convertapi-dotnet/blob/master/LICENSE) file for more information.
