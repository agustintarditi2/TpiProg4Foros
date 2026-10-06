using MyApp.Domain.Exceptions;
using System.Text.RegularExpressions;

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
            throw new DomainValidationException("Password Hash cannot be empty.");
        if (value.Length > 64)
            throw new DomainValidationException("The password is too long. It must be less than 64 characters.");
        if (value.Length < 12)
            throw new DomainValidationException("The password is too short. It must be at least 12 characters.");
// Validamos que la contraseña no consista solo de números.
        if (Regex.IsMatch(value, @"^\d+$"))
            throw new DomainValidationException("The password cannot consist only of numbers. It must contain letters and/or special characters.");
        return new PasswordHash(value);
    }
    public override string ToString() => Value;
}

