namespace FitnessCenterr.Core.Models;

public class Staff
{
    public int StaffID { get; set; }
    public int? UserID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
}