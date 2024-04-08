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
            var productDtos = products.Select(x => new ProductDto()
            {
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                UnitsInStock = x.UnitsInStock,
                Weight = x.Weight
            }).ToList();
            return Ok(productDtos);
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
    
    [HttpGet("search", Name = "SearchProducts")]
    public IActionResult SearchProducts([FromQuery] string keyword, [FromQuery] decimal minPrice, [FromQuery] decimal maxPrice, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var products = new List<Product>();
            if(minPrice != 0 || maxPrice != 0 || !string.IsNullOrEmpty(keyword))
            {
                if (minPrice != 0 && maxPrice != 0 && minPrice < maxPrice 
                    && !string.IsNullOrEmpty(keyword))
                {
                    products = _unitOfWork.GetRepository<Product>()
                        .FindByCondition(product => product.UnitPrice >= minPrice 
                                                    && product.UnitPrice <= maxPrice,
                            pageNumber, pageSize).ToList();
                }
                else if (minPrice == 0 && maxPrice == 0)
                {
                    products = _unitOfWork.GetRepository<Product>()
                        .FindByCondition(product => product.ProductName.Contains(keyword),
                            pageNumber, pageSize).ToList();
                }
                else
                {
                    return BadRequest("Invalid price range or keyword.");
                }
            }
            
            var productDtos = products.Select(product => new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Weight = product.Weight,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
            }).ToList();
        
            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products by price range and keyword.");
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
            
            if(request.UnitPrice <= 0 || request.UnitsInStock <= 0)
                return BadRequest("Unit price and units in stock must be greater than 0.");
            
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

            if(request.UnitPrice <= 0 || request.UnitsInStock <= 0)
                return BadRequest("Unit price and units in stock must be greater than 0.");
            
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