using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PaymentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Payments.Include(p => p.Member).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Payments.Include(p => p.Member).FirstOrDefaultAsync(p => p.PaymentID == id);
        if (p == null) return NotFound(new { message = $"Payment med ID {id} blev ikke fundet." });
        return Ok(p);
    }

    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetByMember(int memberId) =>
        Ok(await _db.Payments.Where(p => p.MemberID == memberId).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Payment dto)
    {
        dto.PaymentDate = DateTime.UtcNow;
        _db.Payments.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.PaymentID }, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p == null) return NotFound();
        _db.Payments.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}