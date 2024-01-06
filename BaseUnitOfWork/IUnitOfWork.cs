using SqlSugar;

namespace Cola.ColaEF.BaseUnitOfWork;
/// <summary>
/// IUnitOfWork
/// </summary>
public interface IUnitOfWork
{
    ISqlSugarClient GetDbClient();
}