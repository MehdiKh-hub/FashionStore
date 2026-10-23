using FashionStore.Web.Models.DTOs;
using FashionStore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly StoreService _storeService;

    public CatalogController(StoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] Guid? categoryId)
    {
        var items = await _storeService.GetProductsAsync(search, categoryId);
        return Ok(items);
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        var product = await _storeService.GetProductByIdAsync(productId);
        if (product is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(product);
    }

    [Authorize]
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var product = await _storeService.CreateProductAsync(request);
            return Ok(new { message = "Product created successfully.", productId = product.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _storeService.GetCategoriesAsync();
        return Ok(categories.Select(c => new { c.Id, c.Name, c.Description }));
    }
}
