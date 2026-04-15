using System;
using System.Globalization;
using System.Net;
using Microsoft.Maui.Controls;

namespace BiblioLinx.Converters
{
    public class TextCleanerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            
            string text = value.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return parameter?.ToString() == "DocumentHtml" ? WrapHtmlDocument("<p></p>") : string.Empty;

            text = CleanStylesFast(text);
            text = NormalizeNewLines(text);

            var mode = parameter?.ToString();
            if (mode == "Html" || mode == "DocumentHtml")
            {
                if (!text.Contains("<p>") && !text.Contains("<br>") && !text.Contains("<b>") && !text.Contains("<li>"))
                    text = text.Replace("\n", "<br>");
                
                return mode == "DocumentHtml" ? WrapHtmlDocument(text) : text;
            }

            return StripHtmlFast(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;

        private static string NormalizeNewLines(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

        private static string CleanStylesFast(string text)
        {
            int safeGuard = 0; // BLOQUEIO DE TRAVAMENTO: Nunca roda mais de 100 vezes!
            while (safeGuard++ < 100)
            {
                int startIndex = text.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1) break;
                
                int endIndex = text.IndexOf("</style>", startIndex, StringComparison.OrdinalIgnoreCase);
                if (endIndex == -1) { text = text.Remove(startIndex); break; }
                
                text = text.Remove(startIndex, endIndex - startIndex + 8);
            }
            return text.Trim();
        }

        public static string StripHtmlFast(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var charArray = new char[input.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < input.Length; i++)
            {
                char let = input[i];
                if (let == '<') { inside = true; continue; }
                if (let == '>') { inside = false; continue; }
                if (!inside) { charArray[arrayIndex++] = let; }
            }
            return WebUtility.HtmlDecode(new string(charArray, 0, arrayIndex)).Trim();
        }

        private static string WrapHtmlDocument(string bodyContent)
        {
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; font-size: 16px; color: #1F2430; background: transparent; margin: 0; padding: 0; line-height: 1.6; word-wrap: break-word; }}
    h1, h2, h3 {{ margin: 0 0 12px 0; font-weight: 600; color: #111827; }}
    p {{ margin: 0 0 14px 0; }}
    ul, ol {{ margin: 0 0 14px 22px; padding: 0; }}
    li {{ margin: 0 0 6px 0; }}
    a {{ color: #4C1D95; text-decoration: none; }}
    @media (prefers-color-scheme: dark) {{ body {{ color: #E0E0E0 !important; }} h1, h2, h3 {{ color: #FFFFFF !important; }} a {{ color: #FF8500 !important; }} }}
</style></head><body>{bodyContent}</body></html>";
        }
    }
}