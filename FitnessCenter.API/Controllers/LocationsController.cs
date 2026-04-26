using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Locations.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var l = await _db.Locations.FindAsync(id);
        if (l == null) return NotFound(new { message = $"Location med ID {id} blev ikke fundet." });
        return Ok(l);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Location dto)
    {
        _db.Locations.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.LocationID }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Location dto)
    {
        var l = await _db.Locations.FindAsync(id);
        if (l == null) return NotFound();
        l.City = dto.City;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var l = await _db.Locations.FindAsync(id);
        if (l == null) return NotFound();
        _db.Locations.Remove(l);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}