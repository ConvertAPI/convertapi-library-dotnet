using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;
using ConvertApiDotNet.Services;

// Demo app to showcase IConverterCatalog methods backed by ConvertAPI OpenAPI spec

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== ConvertAPI Converter Catalog Demo ===");
        Console.WriteLine($"Base URI: {ConvertApiDotNet.ConvertApi.ApiBaseUri}");
        Console.WriteLine();
        
        ConvertApiDotNet.ConvertApi.ApiBaseUri = "https://stag-v2.convertapi.com";

        var catalog = new ConverterCatalog();

        try
        {
            Console.WriteLine("Loading catalog (OpenAPI fetch + parse)...");
            await catalog.Reload();
            Console.WriteLine("Loaded.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load catalog: " + ex.Message);
            Console.WriteLine("The demo will try to continue with lazy loading on each call.\n");
        }

        // 1) GetAllConverters
        Console.WriteLine("--- GetAllConverters() ---");
        var all = SafeCall(() => catalog.GetAllConverters());
        Console.WriteLine($"Total converters: {all.Count}");
        PrintConverters(all.Take(10), title: "First 10 converters");

        // 2) GetConverter(source, destination)
        Console.WriteLine("\n--- GetConverter(src, dst) ---");
        var samples = new (string src, string dst)[]
        {
            ("md", "html"),
            ("pdf", "docx"),
            ("docx", "pdf"),
            ("pdf", "merge"),
        };
        foreach (var (src, dst) in samples)
        {
            var c = SafeCall(() => catalog.GetConverter(src, dst));
            if (c != null)
            {
                Console.WriteLine($"Found: {c.SourceFormat} -> {c.DestinationFormat} | {Trim(c.Summary, 60)}");
                Console.WriteLine($"Source extensions: {string.Join(",", c.SourceExtensions)} -> Destination extensions: {string.Join(",", c.DestinationExtensions)}");
            }
            else
            {
                Console.WriteLine($"Not found: {src} -> {dst}");
            }
        }

        // 3) GetConvertersByTags(tags)
        Console.WriteLine("\n--- GetConvertersByTags([\"pdf\"]) ---");
        var pdfTagged = SafeCall(() => catalog.GetConvertersByTags(new List<string> { "pdf" }));
        Console.WriteLine($"Converters with tag 'pdf': {pdfTagged.Count}");
        PrintConverters(pdfTagged.Take(10));

        // 4) SearchConverters(query) — now hits /info/openapi?q=... on every call
        foreach (var q in new[] { "pdf to docx", "pdf to word", "heic to pdf" })
        {
            Console.WriteLine($"\n--- SearchConverters(\"{q}\") ---");
            var search = SafeCall(() => catalog.SearchConverters(q));
            Console.WriteLine($"Search results: {search.Count}");
            PrintConverters(search.Take(10));
        }

        // 5) GetTags()
        Console.WriteLine("\n--- GetTags() ---");
        var tags = SafeCall(() => catalog.GetTags());
        Console.WriteLine($"Total tags: {tags.Count}");
        foreach (var t in tags.Take(20))
        {
            var label = string.IsNullOrWhiteSpace(t.FriendlyName) ? t.Name : $"{t.FriendlyName} ({t.Name})";
            Console.WriteLine(" - " + label);
        }
        if (tags.Count > 20) Console.WriteLine($" ... and {tags.Count - 20} more");

        // 6) Reload()
        Console.WriteLine("\n--- Reload() ---");
        try
        {
            await catalog.Reload();
            var after = catalog.GetAllConverters();
            Console.WriteLine($"Reloaded. Converters count now: {after.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Reload failed: " + ex.Message);
        }

        Console.WriteLine("\nDemo finished.");
        return 0;
    }

    private static List<ConverterDto> SafeCall(Func<List<ConverterDto>> f)
    {
        try { return f() ?? new List<ConverterDto>(); }
        catch (Exception ex)
        {
            Console.WriteLine("Call failed: " + ex.Message);
            return new List<ConverterDto>();
        }
    }

    private static ConverterDto SafeCall(Func<ConverterDto> f)
    {
        try { return f(); }
        catch (Exception ex)
        {
            Console.WriteLine("Call failed: " + ex.Message);
            return null;
        }
    }

    private static List<TagDto> SafeCall(Func<List<TagDto>> f)
    {
        try { return f() ?? new List<TagDto>(); }
        catch (Exception ex)
        {
            Console.WriteLine("Call failed: " + ex.Message);
            return new List<TagDto>();
        }
    }

    private static void PrintConverters(IEnumerable<ConverterDto> items, string title = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Console.WriteLine(title + ":");

        foreach (var c in items)
        {
            Console.WriteLine($" - {c.SourceFormat} -> {c.DestinationFormat} | {Trim(c.Summary, 60)}");
        }
    }

    private static string Trim(string value, int max)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value.Substring(0, max - 1) + "…";
    }
}
