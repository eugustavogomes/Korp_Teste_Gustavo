namespace FaturamentoService.Exceptions;

public class InvoiceNotFoundException : Exception
{
    public InvoiceNotFoundException(int id)
        : base($"Invoice {id} not found") { }
}
