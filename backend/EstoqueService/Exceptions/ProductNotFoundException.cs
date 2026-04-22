namespace EstoqueService.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int id)
        : base($"Product {id} not found") { }
}
