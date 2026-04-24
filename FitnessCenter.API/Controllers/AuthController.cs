using FitnessCenterr.Core.DTOs.Auth;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) { _db = db; }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Brugernavnet er allerede taget." });

        var user = new User
        {
            Username     = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Enabled      = true,
            Role         = dto.Role ?? "User"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Bruger '{user.Username}' oprettet med rollen '{user.Role}'." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Forkert brugernavn eller adgangskode." });

        return Ok(new AuthResponseDto
        {
            Token    = GenerateJwt(user),
            Username = user.Username,
            Role     = user.Role
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => Ok(new { message = "Logget ud. Slet dit JWT token på klienten." });

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new {
        username = User.FindFirst(ClaimTypes.Name)?.Value,
        role     = User.FindFirst(ClaimTypes.Role)?.Value
    });

    private string GenerateJwt(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role)
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("fitness-center-super-secret-key-2024!!"));
        var token = new JwtSecurityToken(
            issuer:            "FitnessAPI",
            audience:          "FitnessAPIUsers",
            claims:            claims,
            expires:           DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}