namespace TyreCompare.DAL.Interfaces;

public abstract class BaseTestingRepository : ITestingRepository
{
    protected abstract bool CanConnectToDB();
    protected abstract string GetConnectionString();
    protected abstract void TryOpenConnection();
    public virtual string TestDbConnection()
    {
        var finalMessage = string.Empty;
        var message = string.Empty;
        bool shouldContinue = false;

        try
        {
            var canConnect = CanConnectToDB();
            if (!canConnect)
            { throw new Exception("Cannot connect to Db."); }

            message = "Can connect to Db";
            shouldContinue = true;
        }
        catch (Exception ex)
        { message = "Cannot connect to Db"; }

        finalMessage += $"{message}\n";

        if (!shouldContinue)
        { return finalMessage; }

        var cs = GetConnectionString();
        try
        {
            TryOpenConnection();
            message = $"Can open Db connection with connection string:\n{cs}";
        }
        catch (Exception ex)
        { message = $"Cannot open Db connection with connection string:\n{cs}."; }
        finalMessage += $"{message}\n";

        return finalMessage;
    }
}