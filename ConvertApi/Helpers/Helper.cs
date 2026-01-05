using System;

namespace ConvertApiDotNet.Helpers
{
    public static class Helper
    {
        public static bool ContainsIgnoreCase(string source, string token) =>
            source?.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}