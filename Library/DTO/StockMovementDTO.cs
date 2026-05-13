namespace Library.DTO;

public class StockMovementDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public int MovementTypeId { get; set; }
    public string MovementType { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }

}