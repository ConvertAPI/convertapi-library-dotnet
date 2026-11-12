using System.Net;
using System.Runtime.InteropServices;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet.Tests;

// SaveFileAsync and SaveFilesAsync download through the static ConvertApi.GetClient(). Each test installs a
// fake client that serves the result files from memory, so the tests make no network calls, and they must not
// run in parallel with other tests that install a client. Installing it also sets the static ConvertApi.ApiToken,
// so both are put back after each test.
[NonParallelizable]
public class SaveFileTests
{
    private string _dir = "";
    private string? _savedToken;
    private IConvertApiHttpClient? _savedClient;
    private static int _requests;

    [SetUp]
    public void SetUp()
    {
        _savedToken = ConvertApi.ApiToken;
        _savedClient = ConvertApi.GetClient();
        _requests = 0;
        _dir = Path.Combine(Path.GetTempPath(), "ConvertApi.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown()
    {
        _ = new ConvertApi(_savedToken!, _savedClient!);
        Directory.Delete(_dir, recursive: true);
    }

    [Test]
    public async Task SaveFileAsync_ReplacesALongerExistingFile()
    {
        // The sizes from the report: a 146,734-byte PDF saved over a 400,000-byte file left 400,000 bytes.
        var path = Path.Combine(_dir, "document.pdf");
        File.WriteAllBytes(path, Bytes(400_000, seed: 1));
        var content = Bytes(146_734, seed: 2);
        var file = Serve(("document.pdf", content)).Single();

        var saved = await file.SaveFileAsync(path);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(saved.FullName, Is.EqualTo(Path.GetFullPath(path)));
            Assert.That(saved.Length, Is.EqualTo(content.Length));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(content));
        }
    }

    [Test]
    public async Task SaveFilesAsync_ReplacesLongerExistingFiles()
    {
        var first = Bytes(1_000, seed: 3);
        var second = Bytes(2_000, seed: 4);
        File.WriteAllBytes(Path.Combine(_dir, "document.jpg"), Bytes(50_000, seed: 5));
        File.WriteAllBytes(Path.Combine(_dir, "document-2.jpg"), Bytes(50_000, seed: 6));
        var response = new ConvertApiResponse { Files = Serve(("document.jpg", first), ("document-2.jpg", second)) };

        var saved = await response.SaveFilesAsync(_dir);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(saved.Select(f => f.Name), Is.EqualTo(new[] { "document.jpg", "document-2.jpg" }));
            Assert.That(File.ReadAllBytes(Path.Combine(_dir, "document.jpg")), Is.EqualTo(first));
            Assert.That(File.ReadAllBytes(Path.Combine(_dir, "document-2.jpg")), Is.EqualTo(second));
        }
    }

    [Test]
    public async Task SaveFilesAsync_CreatesAMissingFolder()
    {
        var output = Path.Combine(_dir, "output", "nested");
        var content = Bytes(1_000, seed: 7);
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", content)) };

        await response.SaveFilesAsync(output);

        Assert.That(File.ReadAllBytes(Path.Combine(output, "document.pdf")), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFilesAsync_EmptyFolderMeansTheCurrentDirectory()
    {
        var content = Bytes(1_000, seed: 8);
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", content)) };

        var previous = Environment.CurrentDirectory;
        Environment.CurrentDirectory = _dir;
        try
        {
            await response.SaveFilesAsync("");
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }

        Assert.That(File.ReadAllBytes(Path.Combine(_dir, "document.pdf")), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFileAsync_ReplacesAHiddenFile()
    {
        // On Windows, FileMode.Create (CREATE_ALWAYS) refuses an existing hidden or system file, and 3.2.2 wrote it.
        var path = Path.Combine(_dir, "document.pdf");
        File.WriteAllBytes(path, Bytes(5_000, seed: 9));
        File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
        var content = Bytes(1_000, seed: 10);
        var file = Serve(("document.pdf", content)).Single();

        await file.SaveFileAsync(path);

        Assert.That(File.ReadAllBytes(path), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFileAsync_ReplacesAShorterFile()
    {
        var path = Path.Combine(_dir, "document.pdf");
        File.WriteAllBytes(path, Bytes(10, seed: 11));
        var content = Bytes(1_000, seed: 12);
        var file = Serve(("document.pdf", content)).Single();

        await file.SaveFileAsync(path);

        Assert.That(File.ReadAllBytes(path), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFileAsync_WritesToADeviceThatCannotSeek()
    {
        // The length of /dev/null cannot be set, so the save must not try to.
        Assume.That(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), Is.False, "Unix only");
        var file = Serve(("document.pdf", Bytes(1_000, seed: 13))).Single();

        var saved = await file.SaveFileAsync("/dev/null");

        Assert.That(saved.FullName, Is.EqualTo("/dev/null"));
    }

    [TestCase("result.pdf")]
    [TestCase("RESULT.PDF")]
    [TestCase("nested/result.pdf")]
    public async Task SaveFilesAsync_AResultFileNameIsRejected(string name)
    {
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", Bytes(1_000, seed: 14))) };
        var path = Path.Combine(_dir, name);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => response.SaveFilesAsync(path));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(error!.ParamName, Is.EqualTo("outputDirectory"));
            Assert.That(error.Message, Does.Contain("SaveFileAsync"));
            Assert.That(Directory.Exists(path), Is.False, "no folder is created");
            Assert.That(_requests, Is.Zero, "nothing is downloaded");
        }
    }

    [Test]
    public async Task SaveFilesAsync_AnExistingFileIsRejected()
    {
        var path = Path.Combine(_dir, "notes.txt");
        File.WriteAllText(path, "keep me");
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", Bytes(1_000, seed: 15))) };

        await Assert.ThrowsAsync<ArgumentException>(() => response.SaveFilesAsync(path));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.ReadAllText(path), Is.EqualTo("keep me"));
            Assert.That(_requests, Is.Zero);
        }
    }

    [Test]
    public async Task SaveFilesAsync_ABlankFolderIsRejected()
    {
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", Bytes(1_000, seed: 16))) };

        await Assert.ThrowsAsync<ArgumentException>(() => response.SaveFilesAsync("   "));
        Assert.That(_requests, Is.Zero);
    }

    [TestCase("archive.pdf/")]
    [TestCase("v1.2")]
    [TestCase("output.d")]
    public async Task SaveFilesAsync_FolderNamesWithADotAreAccepted(string name)
    {
        // A trailing slash says "folder"; an extension that no result has is a folder name too.
        var content = Bytes(1_000, seed: 17);
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", content)) };
        var path = Path.Combine(_dir, name);

        await response.SaveFilesAsync(path);

        Assert.That(File.ReadAllBytes(Path.Combine(path, "document.pdf")), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFilesAsync_AnExistingFolderNamedLikeAFileIsUsed()
    {
        var path = Path.Combine(_dir, "archive.pdf");
        Directory.CreateDirectory(path);
        var content = Bytes(1_000, seed: 18);
        var response = new ConvertApiResponse { Files = Serve(("document.pdf", content)) };

        await response.SaveFilesAsync(path);

        Assert.That(File.ReadAllBytes(Path.Combine(path, "document.pdf")), Is.EqualTo(content));
    }

    [Test]
    public async Task SaveFilesAsync_NoResultsCreateNoFolder()
    {
        var path = Path.Combine(_dir, "output");
        var response = new ConvertApiResponse { Files = Serve() };

        var saved = await response.SaveFilesAsync(path);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(saved, Is.Empty);
            Assert.That(Directory.Exists(path), Is.False);
        }
    }

    private static byte[] Bytes(int length, int seed)
    {
        var bytes = new byte[length];
        new Random(seed).NextBytes(bytes);
        return bytes;
    }

    // Installs a client that serves these files, and returns them as a conversion response lists them.
    private static ConvertApiFile[] Serve(params (string Name, byte[] Content)[] files)
    {
        var served = new Dictionary<Uri, byte[]>();
        var results = new List<ConvertApiFile>();
        foreach (var (name, content) in files)
        {
            var url = new Uri("https://v2.convertapi.com/d/test/" + name);
            served[url] = content;
            results.Add(new ConvertApiFile { FileName = name, FileSize = content.Length, Url = url });
        }

        // This constructor is the SDK's only way to replace the client that ConvertApi.GetClient() returns.
        _ = new ConvertApi("test-token", new FakeClient(served));
        return results.ToArray();
    }

    private sealed class FakeClient(Dictionary<Uri, byte[]> files) : IConvertApiHttpClient
    {
        public HttpClient Client { get; } = new(new FileServer(files));
    }

    private sealed class FileServer(Dictionary<Uri, byte[]> files) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requests);
            if (request.Method != HttpMethod.Get || !files.TryGetValue(request.RequestUri!, out var content))
                throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });
        }
    }
}
