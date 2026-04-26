namespace FitnessCenterr.Core.Models;

public class Location
{
    public int LocationID { get; set; }
    public string City { get; set; } = string.Empty;

    public ICollection<Center> Centers { get; set; } = new List<Center>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}