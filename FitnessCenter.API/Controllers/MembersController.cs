using FitnessCenterr.Core.DTOs.Members;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // ← tilføj denne
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _db.Members.Include(m => m.Trainer)
            .Select(m => new MemberDto { MemberID = m.MemberID, Name = m.Name, Email = m.Email, TrainerID = m.TrainerID, TrainerName = m.Trainer != null ? m.Trainer.Name : null })
            .ToListAsync();
        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _db.Members.Include(m => m.Trainer).FirstOrDefaultAsync(m => m.MemberID == id);
        if (m == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        return Ok(new MemberDto { MemberID = m.MemberID, Name = m.Name, Email = m.Email, TrainerID = m.TrainerID, TrainerName = m.Trainer?.Name });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            return BadRequest(new { message = "Email er allerede i brug." });
        var member = new Member { Name = dto.Name, Email = dto.Email, TrainerID = dto.TrainerID };
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = member.MemberID }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        member.Name = dto.Name; member.Email = dto.Email; member.TrainerID = dto.TrainerID;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}