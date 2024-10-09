using System.ComponentModel.DataAnnotations.Schema;

namespace TyreCompare.Models;

public class PatternSet
{
    public int Id { get; set; }

    public string? Brand { get; set; }

    public string? Pattern_ITyre { get; set; }

    public string? Pattern_Elite { get; set; }

    public string? Pattern_Stapleton { get; set; }

    public string? ImageName_ITyre { get; set; }

    public string? ImageName_Elite { get; set; }

    public string? ImageName_Stapleton { get; set; }

    public string? ImagePath_ITyre { get; set; }

    public string? ImagePath_Elite { get; set; }

    public string? ImagePath_Stapleton { get; set; }

    public string? NewImageSelectedFrom { get; set; }

    public string? ImagePath_New { get; set; }

    public string? Car_Type { get; set; }

    public string? Car_Type_Grouped { get; set; }

    public bool IsObsolete { get; set; }

    public bool IsImageCorrupted { get; set; }

    public bool IsLive { get; set; }

    public bool IsReviewed { get; set; }

    public bool IsUpdated { get; set; }

    public string? ReviewedBy { get; set; }
    
    public DateTime? ReviewedDate { get; set; }

    public string? PushedBy { get; set; }

    public DateTime? PushedDate { get; set; }
}
