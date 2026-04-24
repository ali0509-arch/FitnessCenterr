using FitnessCenterr.Core.DTOs.ClassBookings;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassBookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClassBookingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _db.ClassBookings.Include(cb => cb.Member).Include(cb => cb.Class)
            .Select(cb => new ClassBookingDto { BookingID = cb.BookingID, MemberID = cb.MemberID, MemberName = cb.Member.Name, ClassID = cb.ClassID, ClassName = cb.Class.Name })
            .ToListAsync();
        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cb = await _db.ClassBookings.Include(cb => cb.Member).Include(cb => cb.Class).FirstOrDefaultAsync(cb => cb.BookingID == id);
        if (cb == null) return NotFound(new { message = $"Booking med ID {id} blev ikke fundet." });
        return Ok(new ClassBookingDto { BookingID = cb.BookingID, MemberID = cb.MemberID, MemberName = cb.Member.Name, ClassID = cb.ClassID, ClassName = cb.Class.Name });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassBookingDto dto)
    {
        if (!await _db.Members.AnyAsync(m => m.MemberID == dto.MemberID))
            return BadRequest(new { message = $"Medlem med ID {dto.MemberID} findes ikke." });
        if (!await _db.Classes.AnyAsync(c => c.ClassID == dto.ClassID))
            return BadRequest(new { message = $"Klasse med ID {dto.ClassID} findes ikke." });
        var booking = new ClassBooking { MemberID = dto.MemberID, ClassID = dto.ClassID };
        _db.ClassBookings.Add(booking);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = booking.BookingID }, booking);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _db.ClassBookings.FindAsync(id);
        if (booking == null) return NotFound(new { message = $"Booking med ID {id} blev ikke fundet." });
        _db.ClassBookings.Remove(booking);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
