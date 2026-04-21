namespace FitnessCenterr.Core.DTOs.Memberships;

public class MembershipDto
{
    public int MembershipID { get; set; }
    public int MemberID { get; set; }
    public string? MemberName { get; set; }
    public int SubscriptionID { get; set; }
    public string? SubscriptionType { get; set; }
    public decimal? SubscriptionPrice { get; set; }
    public DateOnly StartDate { get; set; }
}

public class CreateMembershipDto
{
    public int MemberID { get; set; }
    public int SubscriptionID { get; set; }
    public DateOnly StartDate { get; set; }
}

public class UpdateMembershipDto
{
    public int SubscriptionID { get; set; }
    public DateOnly StartDate { get; set; }
}
