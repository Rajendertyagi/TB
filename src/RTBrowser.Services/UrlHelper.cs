using System;

namespace RTBrowser.Services
{
    public static class UrlHelper
    {
        public static string Normalize(
            string input)
        {
            input =
                input.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                return
                    "https://www.google.com";
            }

            bool looksLikeUrl =
                input.Contains('.')
                && !input.Contains(' ');

            if (looksLikeUrl)
            {
                if (!input.StartsWith(
                        "http://",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    !input.StartsWith(
                        "https://",
                        StringComparison.OrdinalIgnoreCase))
                {
                    input =
                        "https://" + input;
                }

                return input;
            }

            return
                "https://www.google.com/search?q="
                + Uri.EscapeDataString(input);
        }

        public static bool IsHttpUrl(
            string url)
        {
            return
                Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out Uri? uri)
                &&
                (
                    uri.Scheme == Uri.UriSchemeHttp
                    ||
                    uri.Scheme == Uri.UriSchemeHttps
                );
        }
    }
}
