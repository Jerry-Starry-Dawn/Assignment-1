namespace PRN231_Group11_Assignment1_API.Models.DTO;

public class OrderDetailDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }
}