using ConvertApiDotNet;

// Demo app showing how to use ConvertApi.GetConverterInfo to build dynamic UIs

class Program
{
    static async Task<int> Main(string[] args)
    {
        //Get your api token at https://www.convertapi.com/a
        var token = Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN");

        var api = new ConvertApi(token);

        var samples = new List<(string src, string dst)>
        {
            ("docx", "pdf"),
            ("pdf", "pptx"),
            ("pdf", "merge"),
            ("any", "pdf")
        };

        foreach (var (src, dst) in samples)
        {
            try
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"Converter {src} -> {dst}");
                var info = await api.GetConverterInfo(src, dst);
                Console.WriteLine($"Title           : {info.Title}");
                Console.WriteLine($"Summary         : {info.Summary}");
                Console.WriteLine($"AcceptsFormats  : {info.AcceptsFormats}");
                Console.WriteLine($"AcceptsMultiple : {info.AcceptsMultiple}");

                if (info.Parameters != null && info.Parameters.Count > 0)
                {
                    Console.WriteLine("Parameters:");
                    foreach (var kv in info.Parameters)
                    {
                        Console.WriteLine($"  - {kv.Key} : {kv.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("Parameters      : (none)");
                }

                // Example of how you may use AcceptsFormats value in HTML input accept attribute
                Console.WriteLine($"HTML <input type=\"file\" accept=\"{info.AcceptsFormats}\" {(info.AcceptsMultiple ? "multiple" : string.Empty)}>\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get converter info for {src}->{dst}: {ex.Message}");
            }
        }

        return 0;
    }
}
