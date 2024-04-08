namespace PRN231_Group11_Assignment1_API.Models.DTO;

public class OrderDto
{
    public int Id { get; set; }
    public int? MemberId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public decimal? Freight { get; set; }
    
    public List<OrderDetailDto>? OrderDetails { get; set; }
}