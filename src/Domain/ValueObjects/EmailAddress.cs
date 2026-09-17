namespace MyApp.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public string Value {get;}
    private EmailAddress(string value)
    {
        Value = value;
    }
    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception("The username must have content.");
        }
        var trimmed = value.Trim();

        if (trimmed.Length > 254)
            throw new Exception("Email is too long.");

        if (!IsValidFormat(trimmed))
            throw new Exception("Email format is invalid.");

        var at = trimmed.LastIndexOf('@');
        var local = trimmed[..at];
        var domain = trimmed[(at + 1)..].ToLowerInvariant();

                return new EmailAddress($"{local}@{domain}");
    }

    private static bool IsValidFormat(string value)
    {
        var at = value.LastIndexOf('@');
        if (at <= 0 || at == value.Length - 1) return false;

        var local = value[..at];
        var domain = value[(at + 1)..];

        return local.Length <= 64
            && domain.Contains('.')
            && !domain.StartsWith('.')
            && !domain.EndsWith('.');
    }
// Esto hace que cuando se llama a un objeto de esta clase con el método .ToString(), reemplace el resultado con la propiedad .Value... Ej: para EmailAddress email, email.ToString() va a devolver lo mismo que email.Value.
    public override string ToString() => Value;
}