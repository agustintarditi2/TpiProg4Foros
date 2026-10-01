namespace MyApp.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) {}
}

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) {}
}