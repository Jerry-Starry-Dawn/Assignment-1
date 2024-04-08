namespace PRN231_Group11_Assignment1_API.Models.Request.Products;

public class UpdateProductRequest
{
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int CategoryId { get; set; }
    public int UnitsInStock { get; set; }
    public string Weight { get; set; } = null!;
}