namespace MyApp.Domain.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) {}
}

public class ApplicationServiceException : ApplicationException
{
    public ApplicationServiceException(string message) : base(message) {}
}