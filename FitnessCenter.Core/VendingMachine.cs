namespace FitnessCenterr.Core.Models;

public class VendingMachine
{
    public int VendingMachineID { get; set; }
    public int? CenterID { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }

    public ICollection<VendingMachineStock> Stocks { get; set; } = new List<VendingMachineStock>();
}