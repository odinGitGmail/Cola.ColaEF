using System.Collections.Concurrent;
using Cola.ColaEF.EntityBase;
using Cola.ColaEF.Models;
using Cola.Core.ColaConsole;
using Cola.Core.ColaException;
using Cola.Core.Models.ColaEF;
using Cola.Core.Utils.Constants;
using Cola.CoreUtils.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SqlSugar;

namespace Cola.ColaEF.Tenant;

public class TenantContext : ITenantContext
{
    private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
    private static readonly ConcurrentDictionary<int, SqlSugarClient> DbClients = new ConcurrentDictionary<int, SqlSugarClient>();
    private readonly IColaException _colaException;
    private readonly ColaEfConfigOption _efConfig;
    public TenantContext(ITenantResolutionStrategy tenantResolutionStrategy,
        IConfiguration configuration,
        IColaException colaException,
        List<AopOnLogExecutingModel>? aopOnLogExecutingModels,
        List<AopOnErrorModel>? aopOnErrorModels,
        List<GlobalQueryFilter>? globalQueryFilters)
    {
        _tenantResolutionStrategy = tenantResolutionStrategy;
        _efConfig = configuration.GetSection(SystemConstant.CONSTANT_COLAORM_SECTION).Get<ColaEfConfigOption>();
        _colaException = colaException;
        SetDbClientByTenant(aopOnLogExecutingModels, aopOnErrorModels, globalQueryFilters);
    }

    public static TenantContext Create(IServiceProvider serviceProvider, IConfiguration configuration,List<AopOnLogExecutingModel>? aopOnLogExecutingModels, List<AopOnErrorModel>? aopOnErrorModels, List<GlobalQueryFilter>? globalQueryFilters)
    {
        var exceptionHelper = serviceProvider.GetService<IColaException>();
        if (exceptionHelper == null) throw new Exception("未注入 IColaException 类型");
        var tenantResolutionStrategy = serviceProvider.GetService<ITenantResolutionStrategy>();
        if (tenantResolutionStrategy == null) throw new Exception("未注入 ITenantResolutionStrategy 类型");
        return new TenantContext(tenantResolutionStrategy, configuration, exceptionHelper, aopOnLogExecutingModels, aopOnErrorModels, globalQueryFilters);
    }

    public void SetDbClientByTenant(List<AopOnLogExecutingModel>? aopOnLogExecutingModels,List<AopOnErrorModel>? aopOnErrorModels, List<GlobalQueryFilter>? globalQueryFilters)
    {
        foreach (var config in _efConfig.ColaOrmConfig!)
        {
            var connectionConfig = new ConnectionConfig
            {
                ConfigId = config.ConfigId,
                // 配置连接字符串
                ConnectionString = config.ConnectionString,
                DbType = config.DbType!.ConvertStringToEnum<DbType>(),
                IsAutoCloseConnection = config.IsAutoCloseConnection,
                InitKeyType = InitKeyType.Attribute
            };
            
            var client = new SqlSugarClient(connectionConfig);

            #region OnLogExecuting

            if (config.EnableLogAop)
            {
                if (aopOnLogExecutingModels != null)
                {
                    var aopOnLogExecutingModel = aopOnLogExecutingModels.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                    if (aopOnLogExecutingModel != null)
                        client.Aop.OnLogExecuting = aopOnLogExecutingModel.AopOnLogExecuting;
                }
                else
                {
                    client.Aop.OnLogExecuting = (sql, parameters) =>
                    {
                        ConsoleHelper.WriteInfo($"sql:\n\t{sql}");
                        ConsoleHelper.WriteInfo("parameters is :");
                        foreach (var parameter in parameters)
                        {
                            Console.WriteLine($"\tparameter name:{parameter.ParameterName}\tparameter value:{parameter.Value.ToString()}");
                        }
                    };
                }
            }

            #endregion
            
            #region OnError

            if (config.EnableErrorAop)
            {
                if (aopOnErrorModels != null)
                {
                    var aopOnErrorModel = aopOnErrorModels.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                    if (aopOnErrorModel != null)
                        client.Aop.OnError = aopOnErrorModel.AopOnError;
                }
                else
                {
                    client.Aop.OnError = exception =>
                    {
                        ConsoleHelper.WriteException($"Sql Error:");
                        ConsoleHelper.WriteException(JsonConvert.SerializeObject(exception).ToJsonFormatString());
                    };
                }
            }

            #endregion

            #region GlobalFilter

            if (config.EnableGlobalFilter)
            {
                if (globalQueryFilters != null)
                {
                    var globalQueryFilter = globalQueryFilters.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                    if (globalQueryFilter != null)
                        globalQueryFilter.QueryFilter!(client.QueryFilter);
                }
                else
                {
                    client.QueryFilter.AddTableFilter<IDeleted>(t => t.IsDelete == false);
                }
            }
            
            #endregion
            
            DbClients.TryAdd(config.ConfigId!.ToInt(), client);
        }
    }
    
    public SqlSugarClient GetDbClientByTenant()
    {
        var tenantId = _tenantResolutionStrategy.ResolveTenantKey().ToInt();
        if (DbClients.TryGetValue(tenantId, out SqlSugarClient? sqlSugarClient))
        {
            return sqlSugarClient;
        }
        throw _colaException.ThrowException($"{tenantId} 无法找到对应的链接对象");
    }
}