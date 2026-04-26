using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendingMachinesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VendingMachinesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.VendingMachines.Include(v => v.Stocks).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var v = await _db.VendingMachines.Include(v => v.Stocks).FirstOrDefaultAsync(v => v.VendingMachineID == id);
        if (v == null) return NotFound(new { message = $"VendingMachine med ID {id} blev ikke fundet." });
        return Ok(v);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] VendingMachine dto)
    {
        _db.VendingMachines.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.VendingMachineID }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] VendingMachine dto)
    {
        var v = await _db.VendingMachines.FindAsync(id);
        if (v == null) return NotFound();
        v.Name = dto.Name;
        v.Location = dto.Location;
        v.CenterID = dto.CenterID;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.VendingMachines.FindAsync(id);
        if (v == null) return NotFound();
        _db.VendingMachines.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}