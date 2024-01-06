using Cola.ColaEF.BaseUnitOfWork;
using Cola.ColaEF.EFRepository;
using Cola.ColaEF.EntityBase;
using Cola.ColaEF.Models;
using Cola.ColaEF.Tenant;
using Cola.Core.ColaConsole;
using Cola.Core.ColaException;
using Cola.Core.Models.ColaEF;
using Cola.Core.Utils.Constants;
using Cola.CoreUtils.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SqlSugar;

namespace Cola.ColaEF.EFInject;

public static class SqlSugarInject
{
    public static IServiceCollection AddSingletonColaSqlSugar(
        this IServiceCollection services,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        Action<ColaEfConfigOption> action,
        List<GlobalQueryFilter>? tableFilter = null,
        List<AopOnLogExecutingModel>? aopOnLogExecutingModels = null,
        List<AopOnErrorModel>? aopOnErrorModels = null)
    {
        var opts = new ColaEfConfigOption();
        action(opts);
        return InjectSqlSugar(services, opts, httpContextAccessor, configuration, tableFilter, aopOnLogExecutingModels, aopOnErrorModels);
    }

    public static IServiceCollection AddSingletonColaSqlSugar(
        this IServiceCollection services,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        List<GlobalQueryFilter>? tableFilter = null,
        List<AopOnLogExecutingModel>? aopOnLogExecutingModels = null,
        List<AopOnErrorModel>? aopOnErrorModels = null)
    {
        var colaEfConfig = configuration.GetSection(SystemConstant.CONSTANT_COLAORM_SECTION).Get<ColaEfConfigOption>();
        colaEfConfig = colaEfConfig ?? new ColaEfConfigOption();
        var opts = new ColaEfConfigOption
        {
            TenantResolutionStrategy = colaEfConfig.TenantResolutionStrategy,
            ColaOrmConfig = colaEfConfig.ColaOrmConfig
        };
        return InjectSqlSugar(services, opts, httpContextAccessor, configuration, tableFilter, aopOnLogExecutingModels, aopOnErrorModels);
    }

    private static IServiceCollection InjectSqlSugar(
        IServiceCollection services,
        ColaEfConfigOption colaEfConfigOption,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        List<GlobalQueryFilter>? tableFilter = null,
        List<AopOnLogExecutingModel>? aopOnLogExecutingModels = null,
        List<AopOnErrorModel>? aopOnErrorModels = null)
    {
        // 配置参数验证
        ValidateColaEfConfigOption(services, colaEfConfigOption);
        
        var sqlSugarConfigLst = new List<ConnectionConfig>();
        for (var i = 0; i < colaEfConfigOption.ColaOrmConfig!.Count; i++)
        {
            var opt = colaEfConfigOption.ColaOrmConfig[i];
            sqlSugarConfigLst.Add(new ConnectionConfig
            {
                ConfigId = opt.ConfigId,
                DbType = opt.GetSqlSugarDbType(),
                ConnectionString = opt.ConnectionString,
                IsAutoCloseConnection = opt.IsAutoCloseConnection
            });
        }

        #region 参数验证

        ValidateSqlSugarConfigLst(services, sqlSugarConfigLst);

        ValidateAopOnLogExecuting(services, sqlSugarConfigLst, colaEfConfigOption, aopOnLogExecutingModels);

        ValidateAopOnError(services, sqlSugarConfigLst, colaEfConfigOption, aopOnErrorModels);

        #endregion
        
        // ISqlSugarClient 注入
        services.AddSingleton<ISqlSugarClient>(s =>
        {
            var connectionConfigs = new List<ConnectionConfig>();
            foreach (var config in colaEfConfigOption.ColaOrmConfig!)
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
                connectionConfigs.Add(connectionConfig);
            }

            var sqlSugarScope = new SqlSugarScope(
                connectionConfigs,
                sqlSugarClient =>
                {
                    foreach (var connectionConfig in connectionConfigs)
                    {
                        var client = sqlSugarClient.GetConnection(connectionConfig.ConfigId);
                        var config = colaEfConfigOption.ColaOrmConfig.Single(c => c.ConfigId == connectionConfig.ConfigId);

                        #region OnLogExecuting

                        if (config.EnableLogAop)
                        {
                            if (aopOnLogExecutingModels != null)
                            {
                                var aopOnLogExecutingModel =
                                    aopOnLogExecutingModels.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                                if (aopOnLogExecutingModel != null)
                                {
                                    client.Aop.OnLogExecuting = aopOnLogExecutingModel.AopOnLogExecuting!;
                                }

                            }
                            else
                            {
                                (client.Aop as AopProvider)!.OnLogExecuting = (sql, parameters) =>
                                {
                                    ConsoleHelper.WriteInfo($"sql:\n\t{sql}");
                                    ConsoleHelper.WriteInfo("parameters is :");
                                    foreach (var parameter in parameters)
                                    {
                                        if (parameter.Value != null)
                                            Console.WriteLine(
                                                $"\tparameter name:{parameter.ParameterName}\tparameter value:{parameter.Value.ToString()}");
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
                                var aopOnErrorModel =
                                    aopOnErrorModels.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                                if (aopOnErrorModel != null && aopOnErrorModel.AopOnError != null)
                                {
                                    client.Aop.OnError = aopOnErrorModel.AopOnError;
                                }
                            }
                            else
                            {
                                (client.Aop as AopProvider)!.OnError = exception =>
                                {
                                    ConsoleHelper.WriteException($"Sql Error:");
                                    ConsoleHelper.WriteException(JsonConvert.SerializeObject(exception)
                                        .ToJsonFormatString());
                                };
                            }
                        }

                        #endregion

                        #region GlobalFilter

                        if (config.EnableGlobalFilter)
                        {
                            if (tableFilter != null)
                            {
                                var globalQueryFilter = tableFilter.SingleOrDefault(m => m.ConfigId == config.ConfigId);
                                if (globalQueryFilter != null)
                                {
                                    globalQueryFilter.QueryFilter!(client.QueryFilter);
                                }
                            }
                            else
                            {
                                (client.QueryFilter as QueryFilterProvider)!.AddTableFilter<IDeleted>(t =>
                                    t.IsDelete == false);
                            }
                        }

                        #endregion
                    }
                });
            return sqlSugarScope;
        });
        InjectTenantResolutionStrategy(services, httpContextAccessor, configuration, colaEfConfigOption);
        services.AddSingleton<ITenantContext, TenantContext>();
        services.AddSingleton<IUnitOfWork,UnitOfWork>();
        return services;
    }

    private static void InjectTenantResolutionStrategy(IServiceCollection services, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ColaEfConfigOption colaEfConfigOption)
    {
        Dictionary<string, ITenantResolutionStrategy> dicTenantResolutionStrategys =
            new Dictionary<string, ITenantResolutionStrategy>()
            {
                { "DomainTenant",new DomainTenantResolutionStrategy(httpContextAccessor, configuration, services.BuildServiceProvider()) },
                { "HttpHeaderTenant",new HttpHeaderTenantResolutionStrategy(httpContextAccessor) },
                { "RouteValueTenant",new RouteValueTenantResolutionStrategy(httpContextAccessor) },
                { "NoTenant",new NoTenantResolutionStrategy() }
            };
        var tenantResolutionStrategys = dicTenantResolutionStrategys[colaEfConfigOption.TenantResolutionStrategy];
        services.AddSingleton(tenantResolutionStrategys);
    }
    
    #region 检查参数方法

    private static void ValidateAopOnLogExecuting(IServiceCollection services, List<ConnectionConfig> sqlSugarConfigLst,
        ColaEfConfigOption opts, List<AopOnLogExecutingModel>? aopOnLogExecutingModels = null)
    {
        var exceptionHelper = services.BuildServiceProvider().GetService<IColaException>();
        if (aopOnLogExecutingModels != null)
        {
            if (aopOnLogExecutingModels.Count != sqlSugarConfigLst.Count)
                exceptionHelper!.ThrowException("AopOnLogExecuting 个数配置不正确");
            for (var i = 0; i < aopOnLogExecutingModels.Count; i++)
            {
                if (aopOnLogExecutingModels[i].AopOnLogExecuting == null)
                    exceptionHelper!.ThrowException("AopOnLogExecuting 配置不正确");
                aopOnLogExecutingModels[i].ConfigId = opts!.ColaOrmConfig![i].ConfigId ?? string.Empty;
            }
        }
    }

    private static void ValidateColaEfConfigOption(IServiceCollection services, ColaEfConfigOption option)
    {
        var exceptionHelper = services.BuildServiceProvider().GetService<IColaException>();
        if (option.ColaOrmConfig == null || option.ColaOrmConfig.Count == 0)
            exceptionHelper!.ThrowException("ColaEfConfig 配置不正确");
    }

    private static void ValidateAopOnError(IServiceCollection services, List<ConnectionConfig> sqlSugarConfigLst,
        ColaEfConfigOption opts, List<AopOnErrorModel>? aopOnErrorModels = null)
    {
        var exceptionHelper = services.BuildServiceProvider().GetService<IColaException>();
        if (aopOnErrorModels != null)
        {
            if (aopOnErrorModels.Count != sqlSugarConfigLst.Count)
                exceptionHelper!.ThrowException("AopOnLogExecuting 个数配置不正确");
            for (var i = 0; i < aopOnErrorModels.Count; i++)
            {
                if (aopOnErrorModels[i].AopOnError == null)
                    exceptionHelper!.ThrowException("AopOnError 配置不正确");
                aopOnErrorModels[i].ConfigId = opts!.ColaOrmConfig![i].ConfigId ?? string.Empty;
            }
        }
    }

    private static void ValidateSqlSugarConfigLst(IServiceCollection services, List<ConnectionConfig> sqlSugarConfigLst)
    {
        var exceptionHelper = services.BuildServiceProvider().GetService<IColaException>();
        if (sqlSugarConfigLst.Count > 1)
            if (sqlSugarConfigLst.Count(c => string.IsNullOrEmpty(c.ConfigId)) > 0)
                exceptionHelper!.ThrowException("SqlSugar 多库配置 ConfigId 必须配置");
    }

    #endregion
}