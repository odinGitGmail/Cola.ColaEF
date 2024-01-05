using Cola.Core.Models.ColaEF;
using Cola.Core.Utils.Constants;
using Cola.CoreUtils.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Cola.ColaEF.Tenant;
/// <summary>
/// 通过 efConfig 获取 租户id策略
/// 在 ColaOrm 配置中 配置 Domain 节点，获取 TenantId
/// </summary>
public class DomainTenantResolutionStrategy : ITenantResolutionStrategy
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _services;

    public DomainTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IServiceProvider services)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _services = services;
    }

    public string GetTenantResolutionKey()
    {
        return _httpContextAccessor.HttpContext.Request.Host.Value;
    }

    public string ResolveTenantKey()
    {
        var domain = GetTenantResolutionKey();
        var colaOrmConfig = _configuration.GetSection(SystemConstant.CONSTANT_COLAORM_SECTION).Get<List<ColaEfConfig>>();
        var config = colaOrmConfig.SingleOrDefault(d => d.Domain != null && d.Domain.StringCompareIgnoreCase(domain));
        if (config == null) throw new ArgumentException($"{domain} 无法找到对应的 tenant租户id");
        if (!int.TryParse(config.ConfigId, out var tenantId))
            throw new ArgumentException($"{domain} 中的 tenant租户id {tenantId} 无法转为int类型");
        return tenantId.ToString();
    }
}