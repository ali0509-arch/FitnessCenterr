using FitnessCenterr.Core.DTOs.Subscriptions;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SubscriptionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Subscriptions.Select(s => new SubscriptionDto { SubscriptionID = s.SubscriptionID, Type = s.Type, Price = s.Price }).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.Subscriptions.FindAsync(id);
        if (s == null) return NotFound(new { message = $"Abonnement med ID {id} blev ikke fundet." });
        return Ok(new SubscriptionDto { SubscriptionID = s.SubscriptionID, Type = s.Type, Price = s.Price });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto dto)
    {
        if (dto.Price <= 0) return BadRequest(new { message = "Pris skal være større end 0." });
        var sub = new Subscription { Type = dto.Type, Price = dto.Price };
        _db.Subscriptions.Add(sub);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = sub.SubscriptionID }, new SubscriptionDto { SubscriptionID = sub.SubscriptionID, Type = sub.Type, Price = sub.Price });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriptionDto dto)
    {
        var sub = await _db.Subscriptions.FindAsync(id);
        if (sub == null) return NotFound(new { message = $"Abonnement med ID {id} blev ikke fundet." });
        sub.Type = dto.Type; sub.Price = dto.Price;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var sub = await _db.Subscriptions.FindAsync(id);
        if (sub == null) return NotFound(new { message = $"Abonnement med ID {id} blev ikke fundet." });
        _db.Subscriptions.Remove(sub);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
