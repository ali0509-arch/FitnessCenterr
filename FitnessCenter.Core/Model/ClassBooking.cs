namespace FitnessCenterr.Core.Models;

public class ClassBooking
{
    public int BookingID { get; set; }
    public int MemberID { get; set; }
    public int ClassID { get; set; }

    // Navigation properties
    public Member Member { get; set; } = null!;
    public Class Class { get; set; } = null!;
}