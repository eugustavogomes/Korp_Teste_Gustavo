namespace EstoqueService.Exceptions;

public class ConcurrencyException : Exception
{
    public ConcurrencyException()
        : base("Another process modified the data simultaneously. Please try again.") { }
}
