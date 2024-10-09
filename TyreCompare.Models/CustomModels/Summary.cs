using System.ComponentModel.DataAnnotations.Schema;

namespace TyreCompare.Models;

public class Summary
{
    public Summary(int brandId)
    {
        Brand = "brand" + brandId;
        TotalPatterns = 10;
        ReviewedPatterns = 5;
        UpdatedPatterns = 2;
    }

    public Summary() { }

    public string? Brand { get; set; }

    public int TotalPatterns { get; set; }

    public int ReviewedPatterns { get; set; }

    public int UpdatedPatterns { get; set; }

    public bool IsObsoleteIncluded { get; set; }

    [NotMapped]
    public int RemainingPatterns => TotalPatterns - ReviewedPatterns;
}
