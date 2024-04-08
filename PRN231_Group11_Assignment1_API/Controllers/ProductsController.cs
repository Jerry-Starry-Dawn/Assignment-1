using Microsoft.AspNetCore.Mvc;
using PRN231_Group11_Assignment1_API.Models.DTO;
using PRN231_Group11_Assignment1_API.Models.Entities;
using PRN231_Group11_Assignment1_API.Models.Request.Products;
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
            var products = _unitOfWork.GetRepository<Product>().Get(1,10,x => x.Category!).ToList();
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
            
            var product = _unitOfWork.GetRepository<Product>().GetById(id, x => x.Category! );
            if (product == null)
            {
                return NotFound();
            }
            var productDto = new ProductDetailDto()
            {
                ProductName = product.ProductName,
                CategoryName = product.Category!.CategoryName,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                Weight = product.Weight
            };
            return Ok(productDto);
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Error occurred while fetching product by ID.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost(Name = "CreateProduct")]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var category = _unitOfWork.GetRepository<Category>().GetById(request.CategoryId);
            if (category is null)
            {
                return NotFound("Category does not exist.");
            }
            var product = new Product
            {
                ProductName = request.ProductName,
                CategoryId = request.CategoryId,
                UnitPrice = request.UnitPrice,
                UnitsInStock = request.UnitsInStock,
                Weight = request.Weight,
                Category = category
            };
            _unitOfWork.GetRepository<Product>().Insert(product);
            _unitOfWork.Save();
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id}", Name = "DeleteProduct")]
    public IActionResult DeleteProduct([FromRoute] int id)
    {
        try
        {
            var existingProduct = _unitOfWork.GetRepository<Product>().GetById(id , x => x.Category!);
            if (existingProduct == null)
            {
                return NotFound();
            }

            _unitOfWork.GetRepository<Product>().Delete(existingProduct);
            _unitOfWork.Save();
            return Ok(existingProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut("{id}", Name = "UpdateProduct")]
    
    public IActionResult UpdateProduct([FromRoute] int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var existingProduct = _unitOfWork.GetRepository<Product>().GetById(id, x => x.Category!);
            if (existingProduct == null)
            {
                return NotFound("Product does not exist.");
            }

            var category = _unitOfWork.GetRepository<Category>().GetById(request.CategoryId);
            if (category is null)
            {
                return NotFound("Category does not exist.");
            }
            existingProduct.ProductName = request.ProductName;
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.UnitPrice = request.UnitPrice;
            existingProduct.UnitsInStock = request.UnitsInStock;
            existingProduct.Weight = request.Weight;
            existingProduct.Category = category;
            _unitOfWork.GetRepository<Product>().Update(existingProduct);
            _unitOfWork.Save();
            return Ok(existingProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product.");
            return StatusCode(500, "Internal server error");
        }
    }
    
}