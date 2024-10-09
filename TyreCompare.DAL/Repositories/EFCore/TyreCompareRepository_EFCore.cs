using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.DAL.EFCore;

public class TyreCompareRepository_EFCore : BaseTestingRepository, ITyreCompareRepository
{
    private readonly TyreCompareContext_EFCore DbContext;

    public TyreCompareRepository_EFCore(TyreCompareContext_EFCore dbContext)
    {
        DbContext = dbContext;
    }

    public IEnumerable<Summary> GetAllSummary(bool includeObsolete = false)
    {
        var summary = DbContext.Summary.Where(x => x.IsObsoleteIncluded == includeObsolete).ToList();
        return summary;
    }

    public IEnumerable<PatternSet> GetAllPatternSets(bool onlyActive = false)
    {
        IQueryable<PatternSet> patternSets = DbContext.PatternSets;
        if (onlyActive)
        { patternSets = patternSets.Where(x => x.IsObsolete); }
        return patternSets.ToList();
    }

    public IEnumerable<ITyre> GetAllITyres(bool onlyActive = false)
    {
        IQueryable<ITyre> iTyres = DbContext.ITyres;
        if (onlyActive)
        { iTyres = iTyres.Where(x => x.IsObsolete); }
        return iTyres.ToList();
    }

    public IEnumerable<ITyre> GetPushToLiveITyres(Expression<Func<ITyre, bool>> filters)
    {
        IQueryable<ITyre> iTyres = DbContext.ITyres.Where(filters);
        return iTyres.ToList();
    }

    public IEnumerable<User> GetAllUsers()
    {
        var users = DbContext.Users.ToList();
        return users;
    }

    public async Task<int> UpdateITyre(ITyre iTyre)
    {
        DbContext.Update(iTyre);
        var updatedITyres = await DbContext.SaveChangesAsync();
        return updatedITyres;
    }

    public IEnumerable<string> GetAllBrandNames()
    {
        var brandNames = DbContext.ITyres.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();
        return brandNames;
    }

    public Summary GetSummaryByBrand(string brand, bool includeObsolete = false)
    {
        return DbContext.Summary.FirstOrDefault(x => x.Brand == brand && x.IsObsoleteIncluded == includeObsolete);
    }

    public IEnumerable<PatternSet> GetPatternSetByBrand(string brand)
    {
        return DbContext.PatternSets.Where(x => x.Brand == brand).ToList();
    }

    public PatternSet GetPatternSetByBrandPattern(string brand, string pattern)
    {
        return DbContext.PatternSets.FirstOrDefault(x => x.Brand == brand && x.Pattern_ITyre == pattern);
    }

    public ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery)
    {
        throw new NotImplementedException();
    }

    public User GetUserByCredentials(string username, string password)
    {
        return DbContext.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
    }

    public ITyre GetITyreByBrandPattern(string brand, string pattern)
    {
        return DbContext.ITyres.FirstOrDefault(x => x.Brand == brand && x.Pattern == pattern);
    }

    public PatternSet GetPatternSetById(int patternSetId)
    {
        return DbContext.PatternSets.FirstOrDefault(x => x.Id == patternSetId);
    }

    public string GetImageNameById(int patternSetId)
    {
        return DbContext.ITyres.FirstOrDefault(x => x.Id == patternSetId).Image_Name;
    }

    protected override bool CanConnectToDB()
    {
        return DbContext.Database.CanConnect();
    }

    protected override string GetConnectionString()
    {
        return DbContext.Database.GetConnectionString();
    }

    protected override void TryOpenConnection()
    {
        DbContext.Database.OpenConnection();
    }

    public ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery)
    {
        var query = DbContext.Summary.AsQueryable();

        // Additional Filtering based on PaginationQuery
        if (paginationQuery.FilteredColumns != null)
        {
            foreach (var filteredColumn in paginationQuery.FilteredColumns)
            {
                var propertyName = filteredColumn.ColumnName;
                var propertyValue = filteredColumn.ColumnValue;

                // This assumes all filters are string equals. Adjust as needed.
                query = query.Where(item => EF.Property<string>(item, propertyName) == propertyValue);
            }
        }

        // Ordering
        if (!string.IsNullOrEmpty(paginationQuery.SortedColumn))
        {
            if (paginationQuery.IsSortAscending)
            {
                query = query.OrderBy(item => EF.Property<object>(item, paginationQuery.SortedColumn));
            }
            else
            {
                query = query.OrderByDescending(item => EF.Property<object>(item, paginationQuery.SortedColumn));
            }
        }

        // Pagination
        if (paginationQuery.PageInfo != null)
        {
            var skipAmount = (paginationQuery.PageInfo.PageNo - 1) * paginationQuery.PageInfo.PageSize;
            query = query.Skip(skipAmount).Take(paginationQuery.PageInfo.PageSize);
        }

        var result = query.ToList();
        return null;
    }

    public async Task<bool> ResetITyreById(int patternSetId)
    {
        var iTyre = await DbContext.ITyres.FirstOrDefaultAsync(x => x.Id == patternSetId);
        if (iTyre == null)
        { return false; }

        iTyre.IsUpdated = false;
        iTyre.IsReviewed = false;
        iTyre.ReviewedBy = null;
        iTyre.ReviewedDate = null;
        iTyre.SelectedFrom = null;
        iTyre.Image_Url_New = null;
        iTyre.IsLive = false;

        await DbContext.SaveChangesAsync();
        return true;
    }

    public IEnumerable<string> GetAllCarTypes()
    {
        var carTypes = DbContext.ITyres.Select(x => x.Car_Type).Distinct().OrderBy(x => x).ToList();
        return carTypes;
    }

    public IEnumerable<string> GetCarTypesByBrand(string brand)
    {
        var carTypes = DbContext.ITyres.Where(x => x.Brand == brand).Select(x => x.Car_Type).Distinct().OrderBy(x => x).ToList();
        return carTypes;
    }

    public ITyre GetITyreById(int patternSetId)
    {
        var iTyre = DbContext.ITyres.FirstOrDefault(x => x.Id == patternSetId);
        return iTyre;
    }

    public async Task<int> UpdateITyre(PatternSet patternSet)
    {
        var iTyre = await DbContext.ITyres.FirstOrDefaultAsync(x => x.Id == patternSet.Id);
        if (iTyre == null)
        { return 0; }

        iTyre.IsUpdated = patternSet.IsUpdated;
        iTyre.IsLive = patternSet.IsLive;
        iTyre.IsReviewed = patternSet.IsReviewed;
        iTyre.ReviewedBy = patternSet.ReviewedBy;
        iTyre.ReviewedDate = patternSet.ReviewedDate;
        iTyre.SelectedFrom = patternSet.NewImageSelectedFrom;
        iTyre.Image_Url_New = patternSet.ImagePath_New;
        iTyre.PushedBy = patternSet.PushedBy;
        iTyre.PushedDate = patternSet.PushedDate;

        DbContext.Update(iTyre);
        await DbContext.SaveChangesAsync();
        return 1;
    }

    public IEnumerable<ITyre> GetPushToLiveITyres(ITyre iTyreFilters)
    {
        var iTyresList = DbContext.ITyres.Where(x => x.IsReviewed == iTyreFilters.IsReviewed && x.IsLive == iTyreFilters.IsLive && x.IsImageCorrupted == iTyreFilters.IsImageCorrupted && x.IsObsolete == iTyreFilters.IsObsolete && x.IsUpdated == iTyreFilters.IsUpdated);

        return iTyresList;
    }
}
