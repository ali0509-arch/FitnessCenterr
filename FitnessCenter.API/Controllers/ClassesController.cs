using FitnessCenterr.Core.DTOs.Classes;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClassesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var classes = await _db.Classes
            .Include(c => c.Trainer)
            .Select(c => new ClassDto
            {
                ClassID     = c.ClassID,
                Name        = c.Name,
                TrainerID   = c.TrainerID,
                TrainerName = c.Trainer.Name,
                ClassDate   = c.ClassDate
            })
            .ToListAsync();
        return Ok(classes);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var now = DateTime.UtcNow;
        var classes = await _db.Classes
            .Include(c => c.Trainer)
            .Where(c => c.ClassDate >= now)
            .OrderBy(c => c.ClassDate)
            .Select(c => new ClassDto
            {
                ClassID     = c.ClassID,
                Name        = c.Name,
                TrainerID   = c.TrainerID,
                TrainerName = c.Trainer.Name,
                ClassDate   = c.ClassDate
            })
            .ToListAsync();
        return Ok(classes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.Classes.Include(c => c.Trainer).FirstOrDefaultAsync(c => c.ClassID == id);
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        return Ok(new ClassDto { ClassID = c.ClassID, Name = c.Name, TrainerID = c.TrainerID, TrainerName = c.Trainer?.Name, ClassDate = c.ClassDate });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        var newClass = new Class { Name = dto.Name, TrainerID = dto.TrainerID, ClassDate = dto.ClassDate };
        _db.Classes.Add(newClass);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newClass.ClassID },
            new ClassDto { ClassID = newClass.ClassID, Name = newClass.Name, TrainerID = newClass.TrainerID, ClassDate = newClass.ClassDate });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
    {
        var c = await _db.Classes.FindAsync(id);
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        c.Name = dto.Name; c.TrainerID = dto.TrainerID; c.ClassDate = dto.ClassDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Classes.FindAsync(id);
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        _db.Classes.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
