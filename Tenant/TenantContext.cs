using System.Collections.Concurrent;
using Cola.Core.ColaException;
using Cola.Core.Models.ColaEF;
using Cola.CoreUtils.Constants;
using Cola.CoreUtils.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Cola.ColaEF.Tenant;

public class TenantContext : ITenantContext
{
    private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
    private static readonly ConcurrentDictionary<int, ISqlSugarClient> DbClients = new ConcurrentDictionary<int, ISqlSugarClient>();
    private readonly IColaException _colaException;
    private readonly ColaEfConfigOption _efConfig;
    private readonly IServiceProvider _serviceProvider;

    public TenantContext(
        IServiceProvider serviceProvider,
        ITenantResolutionStrategy tenantResolutionStrategy,
        IConfiguration configuration,
        IColaException colaException)
    {
        _serviceProvider = serviceProvider;
        _tenantResolutionStrategy = tenantResolutionStrategy;
        _efConfig = configuration.GetSection(SystemConstant.CONSTANT_COLAORM_SECTION).Get<ColaEfConfigOption>();
        _colaException = colaException;
    }

    public ISqlSugarClient GetMainDb()
    {
        return _serviceProvider.GetService<ISqlSugarClient>()!;
    }
    
    public ISqlSugarClient GetDbClientByTenant()
    {
        var tenantId = _tenantResolutionStrategy.ResolveTenantKey().StringToInt();
        var sqlSugarClient = _serviceProvider.GetService<ISqlSugarClient>();
        return (sqlSugarClient as SqlSugarScope)!.GetConnection(tenantId);
    }
}