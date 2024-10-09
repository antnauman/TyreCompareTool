using System.ComponentModel.DataAnnotations.Schema;

namespace TyreCompare.Models;

public class UserSelectedImage
{
    public int Id { get; set; }

    public string? Brand { get; set; }

    public string? Pattern_ITyre { get; set; }

    public string? ImageName_ITyre { get; set; }

    public string? NewImageSelectedFrom { get; set; }

    public string? NewImageName { get; set; }

    [NotMapped]
    public byte[]? NewImageData { get; set; }

    public string? NewImageString { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }
}
