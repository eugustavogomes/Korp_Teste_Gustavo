namespace EstoqueService.Exceptions;

public class InsufficientReservationException : Exception
{
    public InsufficientReservationException(string description, int reserved, int requested)
        : base($"Insufficient reservation for '{description}': reserved {reserved}, requested {requested}") { }
}
