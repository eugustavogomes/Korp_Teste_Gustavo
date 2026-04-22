namespace EstoqueService.Exceptions;

public class ProductWithActiveReservationException : Exception
{
    public ProductWithActiveReservationException(string description, int reservedBalance)
        : base($"Product '{description}' has {reservedBalance} unit(s) reserved in open invoices and cannot be deleted.")
    {
    }
}
