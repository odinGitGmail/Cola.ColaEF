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

    public ISqlSugarClient GetDbClient()
    {
        return _sqlSugarClient;
    }
}
