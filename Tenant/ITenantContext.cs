using Cola.ColaEF.Models;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Cola.ColaEF.Tenant;

/// <summary>
///     租户上下文
/// </summary>
public interface ITenantContext
{
    ISqlSugarClient GetMainDb();
    ISqlSugarClient GetDbClientByTenant();
}