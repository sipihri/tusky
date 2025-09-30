using System.Text.RegularExpressions;

namespace Tusky.Utils;

public static partial class MarkupExtensions
{
    private const string MarkupPattern = @"\[[^\]]+\]";
    private const string AnsiEscapePattern = @"\x1b\[[0-9;]*[mK]";
    
    public static string WrapInStyle(this string text, string style)
    {
        return $"[{style}]{text}[/]";
    }

    public static string WrapInStyleIf(this string text, string style, bool condition)
    {
        return condition ? WrapInStyle(text, style) : text;
    }
    
    public static string StripMarkup(this string str)
    {
        string noMarkup = MarkupRegex().Replace(str, string.Empty);
        return AnsiEscapeRegex().Replace(noMarkup, string.Empty);
    }
    
    [GeneratedRegex(MarkupPattern)] private static partial Regex MarkupRegex();
    [GeneratedRegex(AnsiEscapePattern)] private static partial Regex AnsiEscapeRegex();
}