namespace FaturamentoService.Exceptions;

public class InvalidStatusException : Exception
{
    public InvalidStatusException(string message)
        : base(message) { }
}
