using Cola.ColaEF.Models;
using SqlSugar;

namespace Cola.ColaEF.Tenant;

/// <summary>
///     租户上下文
/// </summary>
public interface ITenantContext
{
    int? TenantId { get; }

    void SetDbClientByTenant(List<AopOnLogExecutingModel>? aopOnLogExecutingModels,
        List<AopOnErrorModel>? aopOnErrorModels, List<GlobalQueryFilter>? globalQueryFilters);
    
    SqlSugarClient GetDbClientByTenant(int? tenantId);
}