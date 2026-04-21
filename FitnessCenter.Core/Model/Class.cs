namespace FitnessCenterr.Core.Models;

public class Class
{
    public int ClassID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TrainerID { get; set; }
    public DateTime ClassDate { get; set; }

    // Navigation properties
    public Trainer Trainer { get; set; } = null!;
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}