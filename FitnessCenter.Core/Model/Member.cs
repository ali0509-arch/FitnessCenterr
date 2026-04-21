namespace FitnessCenterr.Core.Models;

public class Member
{
    public int MemberID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? TrainerID { get; set; }

    // Navigation properties
    public Trainer? Trainer { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}