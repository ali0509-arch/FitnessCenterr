namespace FitnessCenterr.Core.Models;

public class Membership
{
    public int MembershipID { get; set; }
    public int MemberID { get; set; }
    public int SubscriptionID { get; set; }
    public DateOnly StartDate { get; set; }

    // Navigation properties
    public Member Member { get; set; } = null!;
    public Subscription Subscription { get; set; } = null!;
}