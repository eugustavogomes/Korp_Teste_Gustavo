namespace EstoqueService.Models;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Balance { get; set; }
    public int ReservedBalance { get; set; }
    public int AvailableBalance => Balance - ReservedBalance;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
