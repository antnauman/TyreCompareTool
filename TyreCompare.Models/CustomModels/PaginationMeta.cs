namespace TyreCompare.Models;

public class PaginationMeta
{
    public int TotalRecords { get; set; }

    public int TotalPages => TotalRecords == 0 ? 1 : (int)Math.Ceiling((decimal)TotalRecords / PageSize);

    public int PageSize { get; set; } = 10;

    public int PageNo { get; set; } = 1;

    public int RecordsFrom => TotalRecords == 0 ? 0 : ((PageNo - 1) * PageSize) + 1;

    public int RecordsTo => TotalRecords == 0 ? 0 : (PageNo * PageSize < TotalRecords ? PageNo * PageSize : TotalRecords);

    public string? FirstValue { get; set; }

    public string? LastValue { get; set; }
}
