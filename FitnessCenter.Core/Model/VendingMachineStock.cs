namespace FitnessCenterr.Core.Models;

public class VendingMachineStock
{
    public int StockID { get; set; }
    public int VendingMachineID { get; set; }
    public string? ProductName { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }

    public VendingMachine VendingMachine { get; set; } = null!;
}