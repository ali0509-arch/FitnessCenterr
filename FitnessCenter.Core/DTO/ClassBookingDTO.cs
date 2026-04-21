namespace FitnessCenterr.Core.DTOs.ClassBookings;

public class ClassBookingDto
{
    public int BookingID { get; set; }
    public int MemberID { get; set; }
    public string? MemberName { get; set; }
    public int ClassID { get; set; }
    public string? ClassName { get; set; }
}

public class CreateClassBookingDto
{
    public int MemberID { get; set; }
    public int ClassID { get; set; }
}
