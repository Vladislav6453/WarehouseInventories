namespace Library.DTO;

public class TopCustomerDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal TotalPurchases { get; set; }
    public int OrderCount { get; set; }
}