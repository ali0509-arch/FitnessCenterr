namespace FitnessCenterr.Core.Models;

/// <summary>
/// Bruges til login/authentication – separat fra Member.
/// Role: "Admin" eller "User"
/// </summary>
public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "Admin" or "User"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}