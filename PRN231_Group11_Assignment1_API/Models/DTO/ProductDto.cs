namespace PRN231_Group11_Assignment1_API.Models.DTO;

public class ProductDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? Weight { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
}

public class ProductDetailDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? Weight { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
    public string? CategoryName { get; set; }
}