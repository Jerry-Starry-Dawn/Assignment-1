using Microsoft.AspNetCore.Mvc;
using PRN231_Group11_Assignment1_API.Models.Entities;
using PRN231_Group11_Assignment1_Repo.UnitOfWork;

namespace PRN231_Group11_Assignment1_API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController :  ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(ILogger<ProductsController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet(Name = "GetProducts")]
    public IActionResult Get()
    {
        try
        {
            var products = _unitOfWork.GetRepository<Product>().Get().ToList();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("{id}", Name = "GetProductById")]
    public IActionResult GetProductById([FromRoute] int id)
    {
        try
        {
            var product = _unitOfWork.GetRepository<Product>().GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Error occurred while fetching product by ID.");
            return StatusCode(500, "Internal server error");
        }
    }
}