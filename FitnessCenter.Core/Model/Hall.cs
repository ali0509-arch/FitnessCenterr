namespace FitnessCenterr.Core.Models;

public class Hall
{
    public int HallID { get; set; }
    public int CenterID { get; set; }
    public string? Name { get; set; }

    public Center Center { get; set; } = null!;
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}