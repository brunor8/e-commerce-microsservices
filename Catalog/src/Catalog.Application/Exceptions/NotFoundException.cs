namespace Catalog.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base("The resource could not be found.") { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public static NotFoundException For(string entityName, object key) =>
        new($"{entityName} with key '{key}' was not found.");
}
