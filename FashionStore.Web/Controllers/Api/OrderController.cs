using FashionStore.Web.Models.DTOs;
using FashionStore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FashionStore.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly StoreService _storeService;

    public OrderController(StoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _storeService.GetOrdersAsync(GetUserId());
        return Ok(orders);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var order = await _storeService.CheckoutAsync(GetUserId(), request);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("address")]
    public async Task<IActionResult> AddAddress([FromBody] AddressRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var address = await _storeService.AddAddressAsync(GetUserId(), request);
            return Ok(address);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(claim))
        {
            throw new UnauthorizedAccessException("User id not found in token.");
        }

        return Guid.Parse(claim);
    }
}
