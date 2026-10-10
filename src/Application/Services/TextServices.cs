using System;
using System.Text.RegularExpressions;

namespace MyApp.Application.Services;

public class TextServices
{
    

public static string Parse(string text)
{
    if (string.IsNullOrEmpty(text))
        return "<p></p>";
    
    // Imágenes: ![alt](url)
    text = Regex.Replace(text, @"!\[([^\]]+)\]\(([^\)]+)\)", "<img src=\"$2\" alt=\"$1\">");
    
    // Links: [texto](url)
    text = Regex.Replace(text, @"\[([^\]]+)\]\(([^\)]+)\)", "<a href=\"$2\">$1</a>");
    
    // Negrita: **texto** or __texto__
    text = Regex.Replace(text, @"(\*\*|__)([^\*_]+)\1", "<b>$2</b>");
    
    // Cursiva: *texto* or _texto_
    text = Regex.Replace(text, @"(\*|_)([^\*_]+)\1", "<i>$2</i>");
    
    return $"<p>{text}</p>";
}

}