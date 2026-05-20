using Embedded.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Embedded.Controllers;

/// <summary>
/// A simple business API controller that relies on Bedrock for authentication.
/// Any valid Bedrock JWT is accepted — no extra wiring needed beyond [Authorize].
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult GetOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(db.Orders.Where(o => o.OwnerId == userId).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = new Order
        {
            Description = request.Description,
            Amount      = request.Amount,
            OwnerId     = userId
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}

public record CreateOrderRequest(string Description, decimal Amount);
