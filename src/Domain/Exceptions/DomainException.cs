namespace MyApp.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) {}
}

public class DomainValidationException : ApplicationException
{
    public DomainValidationException(string message) : base(message) {}
}