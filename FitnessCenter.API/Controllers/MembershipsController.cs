using FitnessCenterr.Core.DTOs.Memberships;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembershipsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembershipsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Memberships.Include(ms => ms.Member).Include(ms => ms.Subscription)
            .Select(ms => new MembershipDto { MembershipID = ms.MembershipID, MemberID = ms.MemberID, MemberName = ms.Member.Name, SubscriptionID = ms.SubscriptionID, SubscriptionType = ms.Subscription.Type, SubscriptionPrice = ms.Subscription.Price, StartDate = ms.StartDate })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ms = await _db.Memberships.Include(ms => ms.Member).Include(ms => ms.Subscription).FirstOrDefaultAsync(ms => ms.MembershipID == id);
        if (ms == null) return NotFound(new { message = $"Medlemskab med ID {id} blev ikke fundet." });
        return Ok(new MembershipDto { MembershipID = ms.MembershipID, MemberID = ms.MemberID, MemberName = ms.Member.Name, SubscriptionID = ms.SubscriptionID, SubscriptionType = ms.Subscription.Type, SubscriptionPrice = ms.Subscription.Price, StartDate = ms.StartDate });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        var membership = new Membership { MemberID = dto.MemberID, SubscriptionID = dto.SubscriptionID, StartDate = dto.StartDate };
        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = membership.MembershipID }, membership);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipDto dto)
    {
        var ms = await _db.Memberships.FindAsync(id);
        if (ms == null) return NotFound(new { message = $"Medlemskab med ID {id} blev ikke fundet." });
        ms.SubscriptionID = dto.SubscriptionID; ms.StartDate = dto.StartDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ms = await _db.Memberships.FindAsync(id);
        if (ms == null) return NotFound(new { message = $"Medlemskab med ID {id} blev ikke fundet." });
        _db.Memberships.Remove(ms);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
