namespace TyreCompare.Models;

public class ITyre
{
    public ITyre(int id, int brandId)
    {
        Id = id;
        Brand = "brand" + brandId;
        Pattern = "pattern" + id;
        Image_Name = "image_Name" + id;
        Image_Url = "image_Url" + id;
        Image_Url_New = "image_Url_New" + id;
        IsReviewed = true;
        IsUpdated = true;
        SelectedFrom = "selectedFrom";
        ReviewedBy = "reviewedBy";
        ReviewedDate = DateTime.UtcNow;
        IsObsolete = false;
    }

    public ITyre() { }

    public int Id { get; set; }

    public string? Brand { get; set; }

    public string? Pattern { get; set; }

    public string? Image_Name { get; set; }

    public string? Image_Url { get; set; }

    public string? Image_Url_New { get; set; }

    public string? Car_Type { get; set; }

    public string? Car_Type_Grouped { get; set; }

    public string? SelectedFrom { get; set; }

    public bool IsReviewed { get; set; }

    public bool IsUpdated { get; set; }

    public string? ReviewedBy { get; set; }    

    public DateTime? ReviewedDate { get; set; }

    public bool IsObsolete { get; set; }

    public bool IsImageCorrupted { get; set; }

    public bool IsLive { get; set; }

    public string? PushedBy { get; set; }

    public DateTime? PushedDate { get; set; }
}
