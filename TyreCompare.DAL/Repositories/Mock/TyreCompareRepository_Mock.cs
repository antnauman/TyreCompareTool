using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.DAL.Mock;

public class TyreCompareRepository_Mock : BaseTestingRepository, ITyreCompareRepository
{
    public IEnumerable<string> GetAllBrandNames()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITyre> GetAllITyres(bool onlyActive = false)
    {
        var ityre1 = new ITyre(1, 1);
        var ityre2 = new ITyre(2, 1);
        var ityre3 = new ITyre(3, 2);
        var ityre4 = new ITyre(4, 2);
        var ityre5 = new ITyre(5, 2);

        var ityres = new List<ITyre> { ityre1, ityre2, ityre3, ityre4, ityre5 };
        return ityres;
    }

    public IEnumerable<PatternSet> GetAllPatternSets(bool onlyActive = false)
    {
        return null;
    }

    public IEnumerable<Summary> GetAllSummary(bool includeObsolete = false)
    {
        var summary1 = new Summary(1);
        var summary2 = new Summary(2);

        var summaries = new List<Summary> { summary1, summary2 };
        return summaries;
    }

    public IEnumerable<User> GetAllUsers()
    {
        var user1 = new User(1);
        var user2 = new User(2);

        var users = new List<User> { user1, user2 };
        return users;
    }

    public IEnumerable<PatternSet> GetPatternSetByBrand(string brand)
    {
        return null;
    }

    public PatternSet GetPatternSetByBrandPattern(string brand, string pattern)
    {
        return null;
    }

    public ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery)
    {
        return null;
    }

    public Summary GetSummaryByBrand(string brand, bool includeObsolete = false)
    {
        return null;
    }

    public ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery)
    {
        throw new NotImplementedException();
    }

    public ITyre GetITyreByBrandPattern(string brand, string pattern)
    {
        return null;
    }

    public User GetUserByCredentials(string username, string password)
    {
        return new User
        {
            Id = 1,
            Name = username,
            Password = password,
            UserRoleId = 1,
            UserRoleName = "Admin"
        };
    }

    public async Task<int> UpdateITyre(ITyre iTyre)
    {
        return 1;
    }

    public async Task<bool> ResetITyreById(int patternSetId)
    {
        return true;
    }

    protected override bool CanConnectToDB()
    {
        return true;
    }

    protected override string GetConnectionString()
    {
        return "this is mock connection string";
    }

    protected override void TryOpenConnection()
    {
        return;
    }

    public IEnumerable<string> GetAllCarTypes()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetCarTypesByBrand(string brand)
    {
        throw new NotImplementedException();
    }

    public PatternSet GetPatternSetById(int patternSetId)
    {
        throw new NotImplementedException();
    }

    public string GetImageNameById(int patternSetId)
    {
        throw new NotImplementedException();
    }

    public ITyre GetITyreById(int patternSetId)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateITyre(PatternSet patternSet)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITyre> GetPushToLiveITyres(ITyre iTyreFilters)
    {
        throw new NotImplementedException();
    }
}
