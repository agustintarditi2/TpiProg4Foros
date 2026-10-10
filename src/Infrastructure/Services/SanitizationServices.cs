using MyApp.Application.Interfaces;
using Ganss.Xss;
namespace MyApp.Infrastructure.Services;

public class SanitizationService : ISanitizer
{
    public string Sanitize(string html)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("img");
        return sanitizer.Sanitize(html);
    }
}