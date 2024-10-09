using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Drawing.Drawing2D;
using System.Reflection;
using TyreCompare.BCL;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.DAL.Dapper;

public class TyreCompareRepository_Dapper : BaseTestingRepository, ITyreCompareRepository
{
    private string ConnectionString;

    public TyreCompareRepository_Dapper(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public IEnumerable<string> GetAllBrandNames()
    {
        var brandNames = GetAllData<string>("[Data].[ITyres]", "[Brand]", string.Empty, "GROUP BY [Brand]", null, "ORDER BY Brand");
        return brandNames;
    }

    public IEnumerable<ITyre> GetAllITyres(bool onlyActive = false)
    {
        var iTyresList = GetAllData<ITyre>("[Data].[ITyres]", "*", (onlyActive ? "[IsActive = 1]" : string.Empty), string.Empty, null, "ORDER BY Brand");
        return iTyresList;
    }

    public IEnumerable<PatternSet> GetAllPatternSets(bool onlyActive = false)
    {
        var patternSetList = GetAllData<PatternSet>("[View].[PatternSets]", "*", (onlyActive ? "[IsActive = 1]" : string.Empty));
        return patternSetList;
    }

    public IEnumerable<Summary> GetAllSummary(bool includeObsolete = false)
    {
        var summaryList = GetAllData<Summary>("[View].[Summary]", "*", $"WHERE [IsObsoleteIncluded] = @includeObsolete", string.Empty, new { includeObsolete }, "ORDER BY [Brand]");
        return summaryList;
    }

    public IEnumerable<User> GetAllUsers()
    {
        var usersList = GetAllData<User>("[App].[Users]");
        return usersList;
    }

    public IEnumerable<PatternSet> GetPatternSetByBrand(string brand)
    {
        var patternSetList = GetAllData<PatternSet>("[View].[PatternSets]", "*", "WHERE Brand = @brand", string.Empty, new { brand });
        return patternSetList;
    }

    public PatternSet GetPatternSetByBrandPattern(string brand, string pattern)
    {
        var patternSetList = GetAllData<PatternSet>("[View].[PatternSets]", "*", "WHERE Brand = @brand AND Pattern_ITyre = @pattern", string.Empty, new { brand, pattern });
        return patternSetList.FirstOrDefault();
    }

    public PatternSet GetPatternSetById(int patternSetId)
    {
        var patternSetList = GetAllData<PatternSet>("[View].[PatternSets]", "*", "WHERE Id = @patternSetId", string.Empty, new { patternSetId });
        return patternSetList.FirstOrDefault();
    }

    public string GetImageNameById(int patternSetId)
    {
        var imageNames = GetAllData<string>("[Data].[ITyres]", "[Image_Name]", "WHERE Id = @patternSetId", string.Empty, new { patternSetId });
        return imageNames.FirstOrDefault();
    }

    public ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery)
    {
        var (patternSetList, paginationInfo) = GetPaginatedData<PatternSet>("[View].[PatternSets]", paginationQuery);
        var patternSetPage = new ModelPage<PatternSet>(patternSetList, paginationInfo);
        return patternSetPage;
    }

    public Summary GetSummaryByBrand(string brand, bool includeObsolete = false)
    {
        var summaryList = GetAllData<Summary>("[View].[Summary]", "*", "WHERE Brand = @Brand AND [IsObsoleteIncluded] = @includeObsolete", string.Empty, new { brand, includeObsolete });
        return summaryList.FirstOrDefault();
    }

    public ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery)
    {
        var (summaryList, paginationInfo) = GetPaginatedData<Summary>("[View].[Summary]", paginationQuery);
        var summaryPage = new ModelPage<Summary>(summaryList, paginationInfo);
        return summaryPage;
    }

    public ITyre GetITyreByBrandPattern(string brand, string pattern)
    {
        var iTyreList = GetAllData<ITyre>("[Data].[ITyres]", "*", "WHERE Brand = @Brand AND Pattern = @pattern", string.Empty, new { brand, pattern });
        return iTyreList.FirstOrDefault();
    }

    public ITyre GetITyreById(int patternSetId)
    {
        var iTyresList = GetAllData<ITyre>("[Data].[ITyres]", "*", "WHERE Id = @patternSetId", string.Empty, new { patternSetId });
        return iTyresList.FirstOrDefault();
    }

    public IEnumerable<ITyre> GetPushToLiveITyres(ITyre iTyreFilters)
    {
        var iTyresList = GetAllData<ITyre>("[Data].[ITyres]", "*", "WHERE IsReviewed = @IsReviewed AND IsLive = @IsLive AND ReviewedBy != 'system'", string.Empty, iTyreFilters); 
        
        return iTyresList;
    }

    public User GetUserByCredentials(string username, string password)
    {
        var query = "SELECT u.ID, u.Name, u.Username, u.Password, u.UserRoleId, r.RoleName AS UserRoleName FROM [App].[Users] U INNER JOIN [Lookup].[Roles] R ON U.UserRoleId = R.ID WHERE u.Username = @username AND u.Password = @password";
        using var connection = new SqlConnection(ConnectionString);
        var user = connection.Query<User>(query, new { Username = username, Password = password }).FirstOrDefault();
        return user;
    }

    public async Task<int> UpdateITyre(ITyre iTyre)
    {
        var query = @"  UPDATE [Data].[ITyres]
                        SET
                            IsReviewed = @IsReviewed, 
                            SelectedFrom = @SelectedFrom,
                            Image_Url_New = @Image_Url_New,
                            IsUpdated = @IsUpdated,
                            ReviewedBy = @ReviewedBy,
                            ReviewedDate = @ReviewedDate,
                            IsLive = @IsLive,
                            PushedBy = @PushedBy,
                            PushedDate = @PushedDate
                        WHERE ID = @ID";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return await connection.ExecuteAsync(query, iTyre);
    }

    public async Task<int> UpdateITyre(PatternSet patternSet)
    {
        var query = @"  UPDATE [Data].[ITyres]
                        SET
                            IsReviewed = @IsReviewed, 
                            SelectedFrom = @NewImageSelectedFrom,
                            Image_Url_New = @ImagePath_New,
                            IsUpdated = @IsUpdated,
                            ReviewedBy = @ReviewedBy,
                            ReviewedDate = @ReviewedDate,
                            IsLive = @IsLive,
                            PushedBy = @PushedBy,
                            PushedDate = @PushedDate
                        WHERE ID = @ID";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return await connection.ExecuteAsync(query, patternSet);
    }

    public async Task<bool> ResetITyreById(int patternSetId)
    {
        var query = @"  UPDATE [Data].[ITyres]
                        SET
                            IsUpdated = 0,
                            IsReviewed = 0,
                            ReviewedBy = NULL,
                            ReviewedDate = NULL,
                            SelectedFrom = NULL,
                            Image_Url_New = NULL,
                            IsLive = 0
                        WHERE ID = @ID";
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var result = await connection.ExecuteAsync(query, new { ID = patternSetId });
        return result > 0;
    }

    public IEnumerable<string> GetAllCarTypes()
    {
        var carTypes = GetAllData<string>("[Data].[ITyres]", "[Car_Type]", string.Empty, "GROUP BY [Car_Type]", null, "ORDER BY [Car_Type]");
        return carTypes;
    }

    public IEnumerable<string> GetCarTypesByBrand(string brand)
    {
        var carTypes = GetAllData<string>("[Data].[ITyres]", "[Car_Type]", $"WHERE [Brand] = @brand", "GROUP BY [Car_Type]", new { brand }, "ORDER BY [Car_Type]");
        return carTypes;
    }

    protected override bool CanConnectToDB()
    {
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            connection.Open();
            var result = connection.QuerySingle<int>("SELECT 1");
            return true;
        }
        catch (Exception ex)
        { return false; }
    }

    protected override string GetConnectionString()
    {
        return ConnectionString;
    }

    protected override void TryOpenConnection()
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
    }

    private List<Model> GetAllData<Model>(string tableName, string columns = "*", string whereClause = null, string groupClause = null, object parameters = null, string orderClause = null)
    {
        var query = $"SELECT {columns} FROM {tableName} {whereClause} {groupClause} {orderClause}";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection.Query<Model>(query, parameters).ToList();
    }
    
    private (List<Model>, PaginationMeta) GetPaginatedData<Model>(string tableName, PaginationQuery paginationQuery)
    {
        var parameters = new Dictionary<string, object>();
        var orderClause = PaginatedData_BuildOrderClause(paginationQuery);
        (var whereClause, var whereClauseCursor) = PaginatedData_BuildWhereClause(paginationQuery, parameters);
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        var paginationInfo = PaginatedData_GetPaginationInfo(connection, tableName, paginationQuery, whereClause, parameters);
        var pagedRecordsQuery = PaginatedData_BuildPagedRecordsQuery(tableName, paginationQuery, paginationInfo, whereClause, whereClauseCursor, parameters, orderClause);
        var modelList = connection.Query<Model>(pagedRecordsQuery, parameters).ToList();
        PaginatedData_SetFirstAndLastValues(modelList, paginationQuery, paginationInfo);

        return (modelList, paginationInfo);
    }

    #region PAGINATED DATA

    private string PaginatedData_BuildOrderClause(PaginationQuery paginationQuery)
    {
        var orderclause = string.IsNullOrWhiteSpace(paginationQuery.SortedColumn)
                            ? string.Empty
                            : $"ORDER BY {paginationQuery.SortedColumn} {(paginationQuery.IsSortAscending ? "ASC" : "DESC")}";
        return orderclause;
    }

    private (string, string) PaginatedData_BuildWhereClause(PaginationQuery paginationQuery, Dictionary<string, object> parameters)
    {
        string filters = string.Empty;
        string cursorFilter = string.Empty;
        string whereClause = string.Empty;
        string whereClauseCursor = string.Empty;
        var filterList = new List<string>();

        if (paginationQuery.FilteredColumns?.Any() == true)
        {
            foreach (var filteredColumn in paginationQuery.FilteredColumns)
            {
                if (filteredColumn.ComparisonType == ComparisonTypes.Contains.ToString())
                {
                    filterList.Add($"{filteredColumn.ColumnName} LIKE @{filteredColumn.ColumnName}");
                    parameters.Add($"{filteredColumn.ColumnName}", $"%{filteredColumn.ColumnValue}%");
                }
                else if (filteredColumn.ComparisonType == ComparisonTypes.Equal.ToString())
                {
                    filterList.Add($"{filteredColumn.ColumnName} = @{filteredColumn.ColumnName}");
                    parameters.Add($"{filteredColumn.ColumnName}", filteredColumn.ColumnValue);
                }
            }

            filters += $"{string.Join(" AND ", filterList)}";
        }

        if (paginationQuery.UseCursor)
        {
            var symbol = paginationQuery.GetNextData ? ">" : "<";
            cursorFilter = $"{paginationQuery.SortedColumn} {symbol} @cursorValue";
            parameters.Add($"cursorValue", paginationQuery.CursorValue);
        }

        if (!string.IsNullOrWhiteSpace(filters))
        { whereClause = $" WHERE {filters} "; }

        if (!string.IsNullOrWhiteSpace(cursorFilter))
        { whereClauseCursor = $" WHERE {filters} {(filterList.Any() ? "AND" : string.Empty)} {cursorFilter} "; }
        
        return (whereClause, whereClauseCursor);
    }

    private PaginationMeta PaginatedData_GetPaginationInfo(SqlConnection connection, string tableName, PaginationQuery paginationQuery, string whereClause, Dictionary<string, object> parameters)
    {
        int totalRecords = paginationQuery.PageInfo.TotalRecords;
        if (totalRecords <= 0)
        {
            var countQuery = $"SELECT COUNT(*) FROM {tableName}";
            var totalCountQuery = $"{countQuery} {whereClause}";
            totalRecords = connection.ExecuteScalar<int>(totalCountQuery, parameters);
        }
        var paginationInfo = new PaginationMeta()
        {
            TotalRecords = totalRecords,
            PageSize = paginationQuery.PageInfo.PageSize
        };
        paginationInfo.PageNo = (paginationQuery.PageInfo.PageNo <= paginationInfo.TotalPages)
                                ? paginationQuery.PageInfo.PageNo
                                : paginationInfo.TotalPages;
        parameters.Add("pageSize", paginationInfo.PageSize);

        return paginationInfo;
    }

    private string PaginatedData_BuildPagedRecordsQuery(string tableName, PaginationQuery paginationQuery, PaginationMeta paginationInfo, string whereClause, string whereClauseCursor, Dictionary<string, object> parameters, string orderClause)
    {
        string pagedRecordsQuery = string.Empty;
        if (paginationQuery.UseCursor)
        {
            var recordsQuery = $"SELECT TOP {paginationInfo.PageSize} * FROM {tableName}";
            pagedRecordsQuery = $"{recordsQuery} {whereClauseCursor} {orderClause}";
        }
        else
        {
            parameters.Add("offset", (paginationInfo.PageNo - 1) * paginationInfo.PageSize);
            var pagedClause = "OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            var recordsQuery = $"SELECT * FROM {tableName}";
            pagedRecordsQuery = $"{recordsQuery} {whereClause} {orderClause} {pagedClause}";
        }
        return pagedRecordsQuery;
    }

    private void PaginatedData_SetFirstAndLastValues<Model>(List<Model> modelList, PaginationQuery paginationQuery, PaginationMeta paginationInfo)
    {
        var firstItem = modelList.FirstOrDefault();
        var lastItem = modelList.LastOrDefault();
        if (firstItem != null && lastItem != null)
        {
            try
            {
                var property = typeof(Model).GetProperty(paginationQuery.SortedColumn, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    paginationInfo.FirstValue = property.GetValue(firstItem)?.ToString();
                    paginationInfo.LastValue = property.GetValue(lastItem)?.ToString();
                }
            }
            catch (Exception ex)
            {
                paginationInfo.FirstValue = string.Empty;
                paginationInfo.LastValue = string.Empty;
            }
        }
    }

    #endregion
}