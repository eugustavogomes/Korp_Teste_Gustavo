namespace EstoqueService.Exceptions;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(string description, int available, int requested)
        : base($"Insufficient balance for '{description}': available {available}, requested {requested}") { }
}
