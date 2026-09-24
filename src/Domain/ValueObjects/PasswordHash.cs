using MyApp.Domain.Exceptions;

namespace MyApp.Domain.ValueObjects;

public sealed record PasswordHash
{
    public string Value {get;}
    private PasswordHash( string value )
    {
        Value = value;
    }
    public static PasswordHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Password Hash cannot be empty.");
        if (value.Length > 512)
            throw new DomainException("Password Hash is not a valid format");
        return new PasswordHash(value);
    }
    public override string ToString() => Value;
}

