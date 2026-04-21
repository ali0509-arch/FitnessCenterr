namespace FitnessCenterr.Core.Models;

public class Subscription
{
    public int SubscriptionID { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Navigation properties
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}