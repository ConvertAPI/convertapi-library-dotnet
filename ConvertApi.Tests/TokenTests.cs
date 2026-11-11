using System.Net;
using System.Text;
using ConvertApiDotNet.Exceptions;
using ConvertApiDotNet.Interface;

namespace ConvertApiDotNet.Tests;

// Which token a request carries: the one set in code, else the CONVERTAPI_TOKEN environment variable, else none,
// in which case nothing is sent. The tests change the static ConvertApi.ApiToken, the static client and the
// process environment, so they restore all three and must not run in parallel with other tests.
[NonParallelizable]
public class TokenTests
{
    private const string Variable = "CONVERTAPI_TOKEN";

    private string? _savedToken;
    private IConvertApiHttpClient? _savedClient;
    private string? _savedVariable;
    private string _dir = "";
    private RecordingServer _server = null!;

    [SetUp]
    public void SetUp()
    {
        _savedToken = ConvertApi.ApiToken;
        _savedClient = ConvertApi.GetClient();
        _savedVariable = Environment.GetEnvironmentVariable(Variable);
        Environment.SetEnvironmentVariable(Variable, null);

        _dir = Path.Combine(Path.GetTempPath(), "ConvertApi.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "document.docx"), "not really a docx");

        _server = new RecordingServer();
        // Installs the fake client with no token set in code.
        _ = new ConvertApi(null!, new FakeClient(_server));
    }

    [TearDown]
    public void TearDown()
    {
        _ = new ConvertApi(_savedToken!, _savedClient!);
        Environment.SetEnvironmentVariable(Variable, _savedVariable);
        Directory.Delete(_dir, recursive: true);
        _server.Dispose();
    }

    [Test]
    public async Task TheTokenSetInCodeWinsOverTheVariable()
    {
        Environment.SetEnvironmentVariable(Variable, "token-from-variable");
        var convertApi = new ConvertApi("token-from-code");

        await convertApi.ConvertAsync("docx", "pdf", new ConvertApiFileParam(Path.Combine(_dir, "document.docx")));

        Assert.That(_server.Tokens, Is.EqualTo(new[] { "token-from-code", "token-from-code" }));
    }

    [Test]
    public async Task WithNoTokenInCodeTheVariableIsUsed()
    {
        Environment.SetEnvironmentVariable(Variable, "  token-from-variable\n");
        var convertApi = new ConvertApi();

        await convertApi.ConvertAsync("docx", "pdf", new ConvertApiFileParam(Path.Combine(_dir, "document.docx")));
        await convertApi.GetUserAsync();

        Assert.That(_server.Tokens, Is.EqualTo(new[] { "token-from-variable", "token-from-variable", "token-from-variable" }));
    }

    [Test]
    public async Task WithNoTokenAtAllNothingIsSent()
    {
        var convertApi = new ConvertApi();

        var error = await Assert.ThrowsAsync<ConvertApiException>(() =>
            convertApi.ConvertAsync("docx", "pdf", new ConvertApiFileParam(Path.Combine(_dir, "document.docx"))));

        using (Assert.EnterMultipleScope())
        {
            // Code that already handled the API's 401 for a missing token keeps handling this one.
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(error.Message, Does.Contain("CONVERTAPI_TOKEN"));
            Assert.That(error.Response, Is.EqualTo(error.Message));
            Assert.That(_server.Tokens, Is.Empty);
        }
    }

    [Test]
    public async Task WithNoTokenAtAllGetUserAsyncSendsNothing()
    {
        var error = await Assert.ThrowsAsync<ConvertApiException>(() => new ConvertApi().GetUserAsync());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(_server.Tokens, Is.Empty);
        }
    }

    [Test]
    public void AnEmptyTokenPassedInCodeNamesTheVariable()
    {
        var error = Assert.Throws<ArgumentNullException>(() => _ = new ConvertApi(""));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error!.ParamName, Is.EqualTo("apiToken"));
            Assert.That(error.Message, Does.Contain("CONVERTAPI_TOKEN"));
        }
    }

    private sealed class FakeClient(HttpMessageHandler handler) : IConvertApiHttpClient
    {
        public HttpClient Client { get; } = new(handler);
    }

    // Answers the upload, convert and user requests the way the API does, and records each request's token.
    private sealed class RecordingServer : HttpMessageHandler
    {
        public List<string?> Tokens { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tokens.Add(request.Headers.Authorization?.Parameter);
            var path = request.RequestUri!.AbsolutePath;
            var json = path switch
            {
                "/upload" => """{"FileId":"0123456789abcdef0123456789abcdef","FileName":"document.docx","FileExt":"docx"}""",
                "/convert/docx/to/pdf" => """{"ConversionCost":1,"Files":[{"FileName":"document.pdf","FileExt":"pdf","FileSize":3,"Url":"https://v2.convertapi.com/d/test/document.pdf"}]}""",
                "/user" => """{"Active":true}""",
                _ => throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}"),
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}
