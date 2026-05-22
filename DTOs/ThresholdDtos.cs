namespace PharmacyWmsBackend.DTOs;

public class ThresholdResponse
{
    public int LowStockThreshold { get; set; }
    public int ExpiringSoonDays { get; set; }
}

public class UpdateThresholdRequest
{
    public int? LowStockThreshold { get; set; }
    public int? ExpiringSoonDays { get; set; }
}
