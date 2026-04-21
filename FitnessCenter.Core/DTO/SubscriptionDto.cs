namespace FitnessCenterr.Core.DTOs.Subscriptions;

public class SubscriptionDto
{
    public int SubscriptionID { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreateSubscriptionDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateSubscriptionDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
