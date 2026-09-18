using MyApp.Domain.Exceptions;

namespace MyApp.Domain.ValueObjects;

public sealed record BodyText
{
    public const int MaxLength = 299999;
    public string Value {get;}
    private BodyText(string value) {Value = value;}


    public static BodyText Create(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Content is required.");
        if (trimmed.Length > MaxLength)
            throw new DomainException($"Text cannot exceed {MaxLength} characters.");
        if (trimmed.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
            throw new DomainException("Text contains invalid control characters.");
        var normalized = trimmed.Normalize(System.Text.NormalizationForm.FormC);

        return new BodyText(normalized);
    }
    public override string ToString() => Value;
}