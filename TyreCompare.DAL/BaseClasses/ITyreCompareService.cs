using TyreCompare.Log;
using TyreCompare.Models;

namespace TyreCompare.DAL.Interfaces;

public interface ITyreCompareService: ITestingService
{
    IEnumerable<Summary> GetCompleteSummary(bool includeObsolete = false);
    Summary GetSummaryByBrand(string brand, bool includeObsolete = false);
    ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery);
    IEnumerable<PatternSet> GetPatternSetByBrand(string brand);
    PatternSet GetPatternSetByBrandPattern(string brand, string pattern);
    ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery);
    Task<string> SavePatternImage(UserSelectedImage userSelectedImage);
    User ValidateUser(string username, string password);
    IEnumerable<string> GetAllBrandNames();
    Task<string> ResetPatternSetById(int patternSetId);
    IEnumerable<string> GetAllCarTypes();
    IEnumerable<string> GetCarTypesByBrand(string brand);
    Task<string> PushToLivePatternSetById(int patternSetId, string username);
    Task<string> PushToLiveBulk(AzureContainerInfo azureContainerInfo, string username);
}