using TyreCompare.Models;

namespace TyreCompare.DAL.Interfaces;

public interface ITyreCompareRepository : ITestingRepository
{
    IEnumerable<string> GetAllBrandNames();
    IEnumerable<ITyre> GetAllITyres(bool onlyActive = false);
    ITyre GetITyreByBrandPattern(string brand, string pattern);
    ITyre GetITyreById(int patternSetId);
    Task<int> UpdateITyre(ITyre iTyre);
    Task<int> UpdateITyre(PatternSet patternSet);

    IEnumerable<Summary> GetAllSummary(bool includeObsolete = false);
    Summary GetSummaryByBrand(string brand, bool includeObsolete = false);
    ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery);

    IEnumerable<PatternSet> GetAllPatternSets(bool onlyActive = false);
    IEnumerable<PatternSet> GetPatternSetByBrand(string brand);
    ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery);
    PatternSet GetPatternSetByBrandPattern(string brand, string pattern);
    PatternSet GetPatternSetById(int patternSetId);
    string GetImageNameById(int patternSetId);
    Task<bool> ResetITyreById(int patternSetId);

    IEnumerable<User> GetAllUsers();
    User GetUserByCredentials(string username, string password);

    IEnumerable<string> GetAllCarTypes();
    IEnumerable<string> GetCarTypesByBrand(string brand);
    IEnumerable<ITyre> GetPushToLiveITyres(ITyre iTyreFilters);
}
