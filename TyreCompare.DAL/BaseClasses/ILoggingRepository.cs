namespace TyreCompare.DAL.Interfaces;

public interface ILoggingRepository
{
    Task<bool> AddLog(Models.Log logObject);
}