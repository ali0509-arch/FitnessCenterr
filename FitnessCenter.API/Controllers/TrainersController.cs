using FitnessCenterr.Core.DTOs.Trainers;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly AppDbContext _db;
    public TrainersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Trainers.Select(t => new TrainerDto { TrainerID = t.TrainerID, Name = t.Name }).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _db.Trainers.FindAsync(id);
        if (t == null) return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        return Ok(new TrainerDto { TrainerID = t.TrainerID, Name = t.Name });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTrainerDto dto)
    {
        var trainer = new Trainer { Name = dto.Name };
        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = trainer.TrainerID }, new TrainerDto { TrainerID = trainer.TrainerID, Name = trainer.Name });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTrainerDto dto)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer == null) return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        trainer.Name = dto.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer == null) return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        _db.Trainers.Remove(trainer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
