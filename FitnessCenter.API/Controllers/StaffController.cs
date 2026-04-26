using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _db;
    public StaffController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Staffs.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.Staffs.FindAsync(id);
        if (s == null) return NotFound(new { message = $"Staff med ID {id} blev ikke fundet." });
        return Ok(s);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Staff dto)
    {
        _db.Staffs.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.StaffID }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Staff dto)
    {
        var s = await _db.Staffs.FindAsync(id);
        if (s == null) return NotFound();
        s.Name = dto.Name;
        s.Role = dto.Role;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Staffs.FindAsync(id);
        if (s == null) return NotFound();
        _db.Staffs.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}