using TyreCompare.BCL;

namespace TyreCompare.Models;

public class PaginationQuery
{
    private const int MaxPageSize = 200;

    public PaginationMeta PageInfo { get; set; }

    public string SortedColumn { get; set; }

    public bool IsSortAscending { get; set; } = true;

    public bool UseCursor { get; set; } = false;

    public string? CursorValue { get; set; }

    public bool GetNextData { get; set; } = true;

    public List<FilterColumn>? FilteredColumns { get; set; }

    public bool IsValidObject()
    {
        if (string.IsNullOrWhiteSpace(SortedColumn))
        {  return false; }

        if (PageInfo.PageSize > MaxPageSize)
        { return false; }

        if (UseCursor)
        {
            if (string.IsNullOrWhiteSpace(CursorValue))
            { return false; }
        }
        else
        {
            if (PageInfo == null)
            { return false; }

            if (PageInfo.PageNo < 1)
            { return false; }

            if (PageInfo.PageSize < 1)
            { return false; }
        }

        return true;
    }
}

public class FilterColumn
{
    public string ColumnName { get; set; }
    public string ColumnValue { get; set; }
    public string ComparisonType { get; set; } = ComparisonTypes.Equal.ToString();
}