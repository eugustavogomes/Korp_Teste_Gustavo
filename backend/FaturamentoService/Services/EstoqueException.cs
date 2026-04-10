namespace FaturamentoService.Services;

public class EstoqueException : Exception
{
    public int StatusCode { get; }

    public EstoqueException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
