using TyreCompare.DAL.Interfaces;

namespace TyreCompare.DAL.EFCore;

public class LoggingRepository_EFCore : ILoggingRepository
{
    private readonly TyreCompareContext_EFCore DbContext;

    public LoggingRepository_EFCore(TyreCompareContext_EFCore dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<bool> AddLog(Models.Log logObject)
    {
        DbContext.Logs.Add(logObject);
        await DbContext.SaveChangesAsync();
        return true;
    }
}
