namespace TyreCompare.Models;

public class Log
{
    public int ID { get; set; }

    public string? LogLevel { get; set; }

    public string? Message { get; set; }

    public string? Trace { get; set; }
    
    public DateTime? CreatedDate { get; set; }
}
