namespace FitnessCenterr.Core.Models;

public class Member
{
    public int MemberID { get; set; }
    public int? UserID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int? TrainerID { get; set; }

    public Trainer? Trainer { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}