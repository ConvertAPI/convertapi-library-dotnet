using System.Net;
using System.Text;
using ConvertApiDotNet.Exceptions;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet.Tests;

// What an error answer turns into: a ConvertApiException with the status and the body, and for 401 and 403 a
// message that says what to do. Also: a download that is not 200 OK throws instead of returning the error page.
// The tests change the static token and client, so they restore both and must not run in parallel with others.
[NonParallelizable]
public class ErrorTests
{
    private const string Body = """{"Code":4000,"Message":"from the server"}""";
    private static readonly Uri ResultUrl = new("https://v2.convertapi.com/d/test/document.pdf");

    private string? _savedToken;
    private IConvertApiHttpClient? _savedClient;
    private string _dir = "";
    private StatusServer _server = null!;

    [SetUp]
    public void SetUp()
    {
        _savedToken = ConvertApi.ApiToken;
        _savedClient = ConvertApi.GetClient();
        _dir = Path.Combine(Path.GetTempPath(), "ConvertApi.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "document.docx"), "not really a docx");
        _server = new StatusServer();
        _ = new ConvertApi("test-token", new FakeClient(_server));
    }

    [TearDown]
    public void TearDown()
    {
        _ = new ConvertApi(_savedToken!, _savedClient!);
        Directory.Delete(_dir, recursive: true);
        _server.Dispose();
    }

    [Test]
    public async Task A401FromConvertSaysTheTokenIsMissingOrInvalid()
    {
        _server.Status = HttpStatusCode.Unauthorized;

        var error = await ThrowsFrom(() => new ConvertApi().ConvertAsync("docx", "pdf", new ConvertApiParam("File", "https://example.com/a.docx")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(error.Response, Is.EqualTo(Body));
            Assert.That(error.Message, Does.StartWith("Conversion from docx to pdf error. Unauthorized. The API token"));
            Assert.That(error.Message, Does.Contain("The API token is missing or invalid. Find or get your token at https://www.convertapi.com/a/signup?ReturnUrl=%2Fa%2Fauthentication&utm_source=dotnet-sdk&utm_medium=referral&utm_campaign=dotnet-sdk&utm_content=error-401"));
        }
    }

    [Test]
    public async Task A403FromConvertSaysNoConversionsAreLeft()
    {
        _server.Status = HttpStatusCode.Forbidden;

        var error = await ThrowsFrom(() => new ConvertApi().ConvertAsync("docx", "pdf", new ConvertApiParam("File", "https://example.com/a.docx")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(error.Response, Is.EqualTo(Body));
            Assert.That(error.Message, Does.Contain("No conversions left or the account is suspended. Plans: https://www.convertapi.com/pricing?utm_source=dotnet-sdk&utm_medium=referral&utm_campaign=dotnet-sdk&utm_content=error-403"));
        }
    }

    [Test]
    public async Task A401FromTheUploadGetsTheSameHelp()
    {
        _server.Status = HttpStatusCode.Unauthorized;

        var error = await ThrowsFrom(() => new ConvertApi().ConvertAsync("docx", "pdf", new ConvertApiFileParam(Path.Combine(_dir, "document.docx"))));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(error.Message, Does.StartWith("Unable to upload file."));
            Assert.That(error.Message, Does.Contain("utm_content=error-401"));
        }
    }

    [Test]
    public async Task A403FromGetUserGetsTheSameHelp()
    {
        _server.Status = HttpStatusCode.Forbidden;

        var error = await ThrowsFrom(() => new ConvertApi().GetUserAsync());

        Assert.That(error.Message, Does.Contain("utm_content=error-403"));
    }

    [Test]
    public async Task OtherErrorsKeepTheirMessage()
    {
        _server.Status = HttpStatusCode.InternalServerError;

        var error = await ThrowsFrom(() => new ConvertApi().ConvertAsync("docx", "pdf", new ConvertApiParam("File", "https://example.com/a.docx")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(error.Message, Is.EqualTo("Conversion from docx to pdf error. Internal Server Error"));
            Assert.That(error.Response, Is.EqualTo(Body));
        }
    }

    [Test]
    public async Task AFailedDownloadThrowsAndWritesNoFile()
    {
        _server.Status = HttpStatusCode.NotFound;
        var target = Path.Combine(_dir, "result.pdf");

        var error = await ThrowsFrom(() => Result().SaveFileAsync(target));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(error.Message, Does.StartWith("Unable to download file."));
            Assert.That(error.Response, Is.EqualTo(Body));
            Assert.That(File.Exists(target), Is.False, "the error page must not be saved as the result");
        }
    }

    [Test]
    public async Task AFailedDownloadLeavesAnExistingFileAlone()
    {
        _server.Status = HttpStatusCode.NotFound;
        var target = Path.Combine(_dir, "result.pdf");
        File.WriteAllText(target, "the old result");

        await ThrowsFrom(() => Result().SaveFileAsync(target));

        Assert.That(File.ReadAllText(target), Is.EqualTo("the old result"));
    }

    [Test]
    public async Task AFailedDownloadAsAStreamThrows()
    {
        _server.Status = HttpStatusCode.Forbidden;

        var error = await ThrowsFrom(() => Result().FileStreamAsync());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(error.Message, Does.Contain("utm_content=error-403"));
        }
    }

    [Test]
    public async Task ASuccessfulDownloadStillWorks()
    {
        _server.Status = HttpStatusCode.OK;
        var target = Path.Combine(_dir, "result.pdf");

        await Result().SaveFileAsync(target);

        Assert.That(File.ReadAllText(target), Is.EqualTo(Body));
    }

    private static ConvertApiFile Result() => new() { FileName = "document.pdf", FileSize = 3, Url = ResultUrl };

    private static async Task<ConvertApiException> ThrowsFrom(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (ConvertApiException error)
        {
            return error;
        }
        Assert.Fail("Expected a ConvertApiException.");
        return null!;
    }

    private sealed class FakeClient(HttpMessageHandler handler) : IConvertApiHttpClient
    {
        public HttpClient Client { get; } = new(handler);
    }

    // Answers every request with Status and the same JSON body.
    private sealed class StatusServer : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(Status)
            {
                Content = new StringContent(Body, Encoding.UTF8, "application/json"),
            });
    }
}
