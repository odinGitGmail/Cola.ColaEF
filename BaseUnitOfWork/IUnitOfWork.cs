using SqlSugar;

namespace Cola.ColaEF.BaseUnitOfWork;
/// <summary>
/// IUnitOfWork
/// </summary>
public interface IUnitOfWork
{
    SqlSugarClient GetDbClient();

    void BeginTran();

    void CommitTran();
    void RollbackTran();
}