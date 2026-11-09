using System.Net;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet.Tests;

// SaveFileAsync and SaveFilesAsync download through the static ConvertApi.GetClient(). Each test installs a
// fake client that serves the result files from memory, so the tests make no network calls, and they must not
// run in parallel with other tests that install a client.
[NonParallelizable]
public class SaveFileTests
{
    private string _dir = "";

    [SetUp]
    public void CreateTempDirectory()
    {
        _dir = Path.Combine(Path.GetTempPath(), "ConvertApi.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void DeleteTempDirectory() => Directory.Delete(_dir, recursive: true);

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
            if (request.Method != HttpMethod.Get || !files.TryGetValue(request.RequestUri!, out var content))
                throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });
        }
    }
}
