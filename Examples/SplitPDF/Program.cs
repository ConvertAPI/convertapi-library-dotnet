using ConvertApiDotNet;

class Program
{
    static async Task Main()
    {
        // IMPORTANT: Replace "api-token" with your actual ConvertAPI API token from the dashboard https://www.convertapi.com/a/api-tokens.
        var api = new ConvertApi("api-token");

        // Ensure output directories exist before saving files.
        // You might want to implement more robust directory handling in production code.
        Directory.CreateDirectory("output/bookmark-splits-example");
        Directory.CreateDirectory("output/pattern-splits-example");
        Directory.CreateDirectory("output/text-pattern-splits-example");
        Directory.CreateDirectory("output/extract-specific-pages-example");
        Directory.CreateDirectory("output/split-by-ranges-example");
        Directory.CreateDirectory("output/merged-output-example");

        // 1) Bookmark-Based Splitting
        // Split the PDF into separate files at each bookmark level.
        var res1 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("SplitByBookmark", "true")
        );
        await res1.SaveFilesAsync("output/bookmark-splits-example");

        // 2) Pattern-Based Splitting
        // Split the document into chunks of pages by a repeating pattern (e.g., 2 then 3 pages).
         var res2 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("SplitByPattern", "2,3")
        );
        await res2.SaveFilesAsync("output/pattern-splits-example");

        // 3) Text-Based Splitting
        // Split the document each time a specific text pattern (regex) is matched.
        var res3 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("SplitByTextPattern", @"Chapter \d+:") // Example: splits at "Chapter 1:", "Chapter 2:", etc.
        );
        await res3.SaveFilesAsync("output/text-pattern-splits-example");

        // 4) Extracting Specific Pages
        // Extract individual pages within a specified range, saving each as a separate file.
         var res4 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("ExtractPages", "5-10") // Example: extracts pages 5, 6, 7, 8, 9, 10 into separate files
        );
        await res4.SaveFilesAsync("output/extract-specific-pages-example");

        // 5) Splitting by Ranges
        // Create separate files for specific pages or ranges.
         var res5 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("SplitByRange", "1,3,5-7") // Example: creates files for page 1, page 3, and pages 5-7
        );
        await res5.SaveFilesAsync("output/split-by-ranges-example");

        // 6) Merging Output
        // Use SplitByRange or ExtractPages, but merge all results into a single output file.
         var res6 = await api.ConvertAsync("pdf", "split",
            new ConvertApiFileParam("File", "files/sample.pdf"),
            new ConvertApiParam("SplitByRange", "2,4-6,8"), // Specify pages/ranges
            new ConvertApiParam("MergeOutput", "true") // Merge them into one file
        );
        // This will save a single file containing pages 2, 4, 5, 6, and 8
        await res6.SaveFilesAsync("output/merged-output-example");

        Console.WriteLine("All splits complete!");
    }
}