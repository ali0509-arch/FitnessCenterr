using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CentersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CentersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Centers.Include(c => c.Location).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.Centers.Include(c => c.Location).FirstOrDefaultAsync(c => c.CenterID == id);
        if (c == null) return NotFound(new { message = $"Center med ID {id} blev ikke fundet." });
        return Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Center dto)
    {
        _db.Centers.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.CenterID }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Center dto)
    {
        var c = await _db.Centers.FindAsync(id);
        if (c == null) return NotFound();
        c.LocationID = dto.LocationID;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Centers.FindAsync(id);
        if (c == null) return NotFound();
        _db.Centers.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}