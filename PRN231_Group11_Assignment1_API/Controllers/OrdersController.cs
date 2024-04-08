using Microsoft.AspNetCore.Mvc;
using PRN231_Group11_Assignment1_API.Models.Entities;
using PRN231_Group11_Assignment1_Repo.UnitOfWork;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PRN231_Group11_Assignment1_API.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase {
    private readonly ILogger<OrdersController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(ILogger<OrdersController> logger, IUnitOfWork unitOfWork) {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet(Name = "GetOrders")]
    // With pagination
    public IActionResult Get([FromQuery] int? pageIndex, [FromQuery] int? pageSize) {
        try {
            var orders = _unitOfWork.GetRepository<Order>().Get(pageIndex, pageSize, o => o.Member).ToList().Select(o => new {
                id = o.Id,
                orderDate = o.OrderDate,
                requiredDate = o.RequiredDate,
                shippedDate = o.ShippedDate,
                freight = o.Freight,
                member = new {
                    id = o.MemberId,
                    email = o.Member?.Email,
                },
            });
            return Ok(orders);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while fetching orders.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public IActionResult GetOrderById([FromRoute] int id) {
        try {
            var order = _unitOfWork.GetRepository<Order>().FindByCondition((o) => o.Id == id, o => o.Member).FirstOrDefault();

            if (order == null) {
                return NotFound();
            }

            var details = _unitOfWork.GetRepository<OrderDetail>().FindByCondition((od) => od.OrderId == id, od => od.Product).ToList().Select(od => new {
                productId = od.ProductId,
                productName = od.Product?.ProductName,
                unitPrice = od.UnitPrice,
                quantity = od.Quantity,
                discount = od.Discount,
            }).ToList();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
                details = details,
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while fetching order by ID.");
            return StatusCode(500, "Internal server error");
        }
    }

    public class OrderDetailDTO {
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(0, 100)]
        public double? Discount { get; set; }
    }

    [HttpPost(Name = "CreateOrder")]
    public IActionResult CreateOrder([FromBody] List<OrderDetailDTO> details) {
        var memberId = 1; // Hardcoded member ID for now

        try {
            // Check if details is empty
            if (details.Count == 0) {
                return BadRequest("Order details cannot be empty.");
            }

            var validDetails = new List<OrderDetail>();

            // Validate details
            foreach (var detail in details) {
                // Check if product exists
                var product = _unitOfWork.GetRepository<Product>().GetById(detail.ProductId);
                if (product == null) {
                    return BadRequest($"Product with ID {detail.ProductId} does not exist.");
                }

                // Check discount is between 0 and 100
                if (detail.Discount < 0 || detail.Discount > 100) {
                    return BadRequest("Discount must be between 0 and 100.");
                }

                // Check quantity is greater than 0
                if (detail.Quantity <= 0) {
                    return BadRequest("Quantity must be greater than 0.");
                }

                // Check quantity is less or equal to product's units in stock
                if (detail.Quantity > product.UnitsInStock) {
                    return BadRequest($"Quantity must be less or equal to product's units in stock ({product.UnitsInStock}).");
                }

                validDetails.Add(new OrderDetail {
                    ProductId = detail.ProductId,
                    UnitPrice = product.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount ?? 0
                });

                // Update product's units in stock
                product.UnitsInStock -= detail.Quantity;
                _unitOfWork.GetRepository<Product>().Update(product);
            }

            // Create order
            var order = new Order {
                OrderDate = DateTime.Now,
                OrderDetails = validDetails,
                MemberId = memberId
            };

            _unitOfWork.GetRepository<Order>().Insert(order);
            _unitOfWork.Save();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while creating order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}", Name = "UpdateOrder")]
    public IActionResult UpdateOrder([FromRoute] int id, [FromBody] List<OrderDetailDTO> details) {
        try {
            var order = _unitOfWork.GetRepository<Order>().FindByCondition((o) => o.Id == id, (o) => o.OrderDetails).FirstOrDefault();

            if (order == null) {
                return NotFound();
            }

            // Check if details is empty
            if (details.Count == 0) {
                return BadRequest("Order details cannot be empty.");
            }

            var validDetails = new List<OrderDetail>();

            // Validate details
            foreach (var detail in details) {
                // Check if product exists
                var product = _unitOfWork.GetRepository<Product>().GetById(detail.ProductId);
                if (product == null) {
                    return BadRequest($"Product with ID {detail.ProductId} does not exist.");
                }

                // Check discount is between 0 and 100
                if (detail.Discount < 0 || detail.Discount > 100) {
                    return BadRequest("Discount must be between 0 and 100.");
                }

                // Check quantity is greater than 0
                if (detail.Quantity <= 0) {
                    return BadRequest("Quantity must be greater than 0.");
                }

                // Check if order detail exists
                var orderDetail = order.OrderDetails.FirstOrDefault((od) => od.ProductId == detail.ProductId && od.OrderId == id);

                // If null, create new order detail
                if (orderDetail == null) {
                    // Check quantity is less or equal to product's units in stock
                    if (detail.Quantity > product.UnitsInStock) {
                        return BadRequest($"Quantity must be less or equal to product's units in stock ({product.UnitsInStock}).");
                    }

                    validDetails.Add(new OrderDetail {
                        ProductId = detail.ProductId,
                        UnitPrice = product.UnitPrice,
                        Quantity = detail.Quantity,
                        Discount = detail.Discount ?? 0
                    });

                    // Update product's units in stock
                    product.UnitsInStock -= detail.Quantity;
                    _unitOfWork.GetRepository<Product>().Update(product);
                } else {
                    // Check quantity is less or equal to product's units in stock
                    if (detail.Quantity > product.UnitsInStock + orderDetail.Quantity) {
                        return BadRequest($"Quantity must be less or equal to product's units in stock ({product.UnitsInStock + orderDetail.Quantity}).");
                    }

                    orderDetail.Quantity = detail.Quantity;
                    orderDetail.Discount = detail.Discount ?? 0;

                    validDetails.Add(orderDetail);

                    // Update product's units in stock
                    product.UnitsInStock += orderDetail.Quantity - detail.Quantity;
                    _unitOfWork.GetRepository<Product>().Update(product);
                }
            }

            // Update order
            order.OrderDetails = validDetails;
            _unitOfWork.GetRepository<Order>().Update(order);
            _unitOfWork.Save();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while updating order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}", Name = "DeleteOrder")]
    public IActionResult DeleteOrder([FromRoute] int id) {
        try {
            var order = _unitOfWork.GetRepository<Order>().FindByCondition((o) => o.Id == id, (o) => o.OrderDetails).FirstOrDefault();

            if (order == null) {
                return NotFound();
            }

            foreach (var detail in order.OrderDetails) {
                _unitOfWork.GetRepository<OrderDetail>().Delete(detail);
            }

            _unitOfWork.GetRepository<Order>().Delete(order);
            _unitOfWork.Save();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while deleting order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/cancel", Name = "CancelOrder")]
    public IActionResult CancelOrder([FromRoute] int id) {
        try {
            var order = _unitOfWork.GetRepository<Order>().FindByCondition((o) => o.Id == id, (o) => o.OrderDetails).FirstOrDefault();

            if (order == null) {
                return NotFound();
            }

            foreach (var detail in order.OrderDetails) {
                var product = _unitOfWork.GetRepository<Product>().GetById(detail.ProductId);
                // Ignore if product is null
                if (product == null) continue;
                product.UnitsInStock += detail.Quantity;
                _unitOfWork.GetRepository<Product>().Update(product);
            }

            _unitOfWork.GetRepository<Order>().Delete(order);
            _unitOfWork.Save();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while cancelling order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/ship", Name = "ShipOrder")]
    public IActionResult ShipOrder([FromRoute] int id) {
        try {
            var order = _unitOfWork.GetRepository<Order>().GetById(id);

            if (order == null) {
                return NotFound();
            }

            if (order.ShippedDate != null) {
                return BadRequest("Order has already been shipped.");
            }

            order.ShippedDate = DateTime.Now;
            _unitOfWork.GetRepository<Order>().Update(order);
            _unitOfWork.Save();

            return Ok(new {
                id = order.Id,
                orderDate = order.OrderDate,
                requiredDate = order.RequiredDate,
                shippedDate = order.ShippedDate,
                freight = order.Freight,
                member = new {
                    id = order.MemberId,
                    email = order.Member?.Email,
                },
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while shipping order.");
            return StatusCode(500, "Internal server error");
        }
    }
}