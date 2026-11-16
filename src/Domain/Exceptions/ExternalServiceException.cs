namespace MyApp.Domain.Exceptions;
//atrapa las excepciones que vengan del lado de la api externa
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message) { }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}