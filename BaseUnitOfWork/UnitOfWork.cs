using System.Diagnostics;
using Cola.ColaEF.Tenant;
using SqlSugar;

namespace Cola.ColaEF.BaseUnitOfWork;

/// <summary>
/// UnitOfWork
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _sqlSugarClient;

    public UnitOfWork(ITenantContext tenantContext)
    {
        _sqlSugarClient = tenantContext.GetDbClientByTenant();
    }

    /// <summary>
    /// 获取DB，保证唯一性
    /// </summary>
    /// <returns></returns>
    public SqlSugarClient GetDbClient()
    {
        // 必须要as，后边会用到切换数据库操作
        return (_sqlSugarClient as SqlSugarClient)!;
    }

    public void BeginTran()
    {
        GetDbClient().BeginTran();
    }

    public void CommitTran()
    {
        try
        {
            GetDbClient().CommitTran(); 
        }
        catch (Exception ex)
        {
            GetDbClient().RollbackTran();
            throw ex;
        }
    }

    public void RollbackTran()
    {
        GetDbClient().RollbackTran();
    }
}
