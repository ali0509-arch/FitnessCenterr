namespace FitnessCenterr.Core.Models;

public class Center
{
    public int CenterID { get; set; }
    public int LocationID { get; set; }

    public Location Location { get; set; } = null!;
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    public ICollection<VendingMachine> VendingMachines { get; set; } = new List<VendingMachine>();
}