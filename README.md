#### Cola.EF 框架

[![Version](https://flat.badgen.net/nuget/v/Cola.ColaEF?label=version)](https://github.com/odinGitGmail/Cola.ColaEF) [![download](https://flat.badgen.net/nuget/dt/Cola.ColaEF)](https://www.nuget.org/packages/Cola.ColaEF) [![commit](https://flat.badgen.net/github/last-commit/odinGitGmail/Cola.ColaEF)](https://flat.badgen.net/github/last-commit/odinGitGmail/Cola.ColaEF) [![Blog](https://flat.badgen.net/static/blog/odinsam.com)](https://odinsam.com)

### 简介
    框架使用仓储模式方便简单，实现了常用接口。并支持多租户，有 RouteValueTenant、HttpHeaderTenant、DomainTenant 以及 NoTenant 多种策略。

##### 注入

注入可以通过config配置文件形式注入（推荐）、也可以通过代码直接注入

```json 配置文件
{
  "ColaOrm": {
    "TenantResolutionStrategy": "NoTenant",
    "ColaOrmConfig": [
      {
        // 多租户模式租户id传参方式      
        // NoTenant  不使用多租户   RouteValueTenant 路由形式   HttpHeaderTenant  http头形式    DomainTenant   域名形式
        // 路由形式 和 http头形式  都必须传递  tenantId: value
        // 默认值 1
        "ConfigId": "1",
        "Domain": "www.odinsam.com",
        "DbType": "MySql",
        "ConnectionString": "server=xxx.xxx.xxx.xxx;Database=db;Uid=root;Pwd=pwd;AllowLoadLocalInfile=true;",
        "IsAutoCloseConnection": true,
        "EnableLogAop": true,
        "EnableErrorAop": true,
        "EnableGlobalFilter": true
      }
    ]
  }
}
```

```csharp
// 注入 ColaSqlSugar
builder.Services.AddColaSqlSugar(config,new HttpContextAccessor(),
        tableFilter:(new List<GlobalQueryFilter>()
        {
            new GlobalQueryFilter()
            {
                ConfigId = "1",
                // 框架查询过滤标识删除的数据
                QueryFilter = (provider => provider.AddTableFilter<IStatus>(t => t.IsDelete == false))
            }
        }),
        aopOnLogExecutingModels:new List<AopOnLogExecutingModel>()
        {
            new AopOnLogExecutingModel()
            {
                ConfigId   = "1",
                // 框架操作 打印sql语句以及参数
                AopOnLogExecuting = ((sql, parameters) =>
                {
                    ConsoleHelper.WriteInfo($"sql is\n{sql}");
                    foreach (var sqlParameter in parameters)
                    {
                        Console.WriteLine($"sqlParameter.ParameterName: { sqlParameter.ParameterName } \t\t sqlParameter.Value: {sqlParameter.Value}");
                    }
                })
            }
        },
        aopOnErrorModels:new List<AopOnErrorModel>()
        {
            // 框架操作错误  aop处理
            new AopOnErrorModel()
            {
                ConfigId = "1",
                AopOnError = (ConsoleHelper.WriteException)
            }
        }
    );
```

##### 新建 IOdinLogRepository 和 OdinLogRepository
```csharp
public interface IOdinLogRepository : IBaseRepository<OdinLog>
{
    
}

public class OdinLogRepository : BaseRepository<OdinLog>, IOdinLogRepository
{
    public OdinLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
```

##### 新建 OdinLog 实体 和 其他相关定义
框架 SqlSugarEntityBase<T> 默认定义了实体的相关标识列。可以不使用自己定义。T 类型灵活定义实体的主键类型。
IDeleted 用来标识实体是否被删除。框架默认使用IsDelete字段。

```csharp 
/// <summary>
///     SqlSugarEntityBase
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlSugarEntityBase<T> : IEntityBase<T>, IDeleted
{
    /// <summary>
    ///     Id主键
    /// </summary>
    public T? Id { get; set; }
    
    /// <summary>
    ///     Remark
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    ///     CreateUser
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    ///     CreateTime
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    ///     Remark
    /// </summary>
    public string? UpdateUser { get; set; }

    /// <summary>
    ///     Remark
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    ///     isDelete 0 false other true
    /// </summary>
    public bool IsDelete { get; set; }
    
}
```
##### 自定义 IStatus 标识实体是否被删除字段。
```csharp
public interface IStatus
{
    public bool IsDelete { get; set; }
}
```

##### 定义实体类 OdinLog MOdel
```csharp OdinLog MOdel 
[SugarTable("tb_OdinLog")]
public class OdinLog : SqlSugarEntityBase<long>, IStatus
{
    public string? LogLevel { get; set; }
    public string? LogContent { get; set; }
    public string? ExceptionInfo { get; set; }
}

// tree类型 实体 用于树查询
public class Tree
{
               
   [SqlSugar.SugarColumn(IsPrimaryKey =true)]
   public int Id { get; set; } //关联字段 默认是主键
   public string Name { get; set; }
   public int ParentId { get; set; }//父级字段
   [SqlSugar.SugarColumn(IsIgnore = true)]
   public List<Tree> Child { get; set; }
}

```

##### 新建 IOdinServices 和 OdinServices
```csharp
public interface IOdinServices : IBaseServices<OdinLog>
{
    // 模拟services业务接口
    List<OdinLog> QueryLog();
    
    // 事务查询
    Task Trans(OdinLog log1,OdinLog log2);
    
    // 树查询
    Task<List<Tree>> QueryTreeAsync();
}

public class OdinServices : BaseServices<OdinLog>, IOdinServices
{
    private readonly IOdinLogRepository _odinLogRepository;
    // 主库 多租户事务异步调用必须使用主库 而不能用字库
    private readonly ISqlSugarClient _mainDb;
    // 当前库
    private readonly ISqlSugarClient _db;
    public OdinServices(IUnitOfWork unitOfWork, IOdinLogRepository odinLogRepository)
    {
        _odinLogRepository = odinLogRepository;
        _db = _odinLogRepository.DbClient;
        _mainDb = _db.DbMaintenance.Context.Root;

    }
    // 模拟实现业务接口
    public List<OdinLog> QueryLog()
    {
        return _odinLogRepository.Query();
    }
    
    // 多租户事务调用
    public async Task Trans(OdinLog log1,OdinLog log2)
    {
        var result = await _mainDb.AsTenant().UseTranAsync(async () =>
        {
            await EditOdinLog(log1);
            var i = 0;
            var a = 1 / i;
            await EditOdinLog(log2);
            return true;//返回值会变成 res.Data
        });
        if (result.Data) //返回值为false
        {
            Console.WriteLine("trans ok");
        }
        else
        {
            Console.WriteLine("trans error");
            Console.WriteLine($"result.IsSuccess:{result.IsSuccess}");
            Console.WriteLine(result.ErrorMessage);
        }
    }
    
    // 树查询
    public async Task<List<Tree>> QueryTreeAsync()
    {
        object[] inIds = new object[] { 11, 12 };
        return await _odinLogRepository.QueryTreeAsync(it => it.Child, it => it.ParentId, 0, inIds);
    }
}
```
##### 注入仓储
```csharp
builder.Services.AddSingleton<IOdinLogRepository, OdinLogRepository>();
builder.Services.AddSingleton<IOdinServices, OdinServices>();
```

##### 模拟 controller 获取 IOdinServices
```csharp
var odinServices = builder.Services.BuildServiceProvider().GetService<IOdinServices>();
```

##### 调用业务方法获取数据
```csharp
var sqlResult = odinServices.QueryLog();
Console.WriteLine($"query count:{sqlResult.Count}");
if (sqlResult.Count > 0) Console.WriteLine(JsonConvert.SerializeObject(sqlResult[0]).ToJsonFormatString());

var tree = await odinServices.QueryTree();
```

```text console wirete query result
[2024-01-05 06:43:52] sql is
SELECT `LogLevel`,`LogContent`,`ExceptionInfo`,`Id`,`Remark`,`CreateUser`,`CreateTime`,`UpdateUser`,`UpdateTime`,`IsDelete` FROM `tb_OdinLog`  WHERE ( `IsDelete` = @IsDelete0 )
sqlParameter.ParameterName: @IsDelete0   sqlParameter.Value: False
query count:7
{
    "LogLevel": "Info",
    "LogContent": "{\"ContentType\":null,\"SerializerSettings\":null,\"StatusCode\":null,\"Value\":{\"Data\":{\"t\":\"tt\"},\"Message\":null,\"Code\":0,\"Error\":null}}",
    "ExceptionInfo": "null",
    "Id": 337152753199616000,
    "Remark": "",
    "CreateUser": "OdinLog",
    "CreateTime": "2022-07-19T16:44:40",
    "UpdateUser": "OdinLog",
    "UpdateTime": "2022-07-19T16:44:40",
    "IsDelete": false
}

```

##### 事务调用
```csharp
builder.Services.AddSingleton<IOdinLogRepository, OdinLogRepository>();
builder.Services.AddSingleton<IOdinServices, OdinServices>();
var odinServices = builder.Services.BuildServiceProvider().GetService<IOdinServices>();
var e1 = odinServices.QueryLogById(337152753199616000);
Console.WriteLine(e1.Remark);
e1.Remark = "e111";
var e2 = odinServices.QueryLogById(337152798942695424);
Console.WriteLine(e2.Remark);
e2.Remark = "e222";
try
{
    await odinServices.Trans(e1, e2);
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}
e1 = odinServices.QueryLogById(337152753199616000);
Console.WriteLine(e1.Remark);
e2 = odinServices.QueryLogById(337152798942695424);
Console.WriteLine(e2.Remark);

Console.WriteLine();Console.WriteLine();
```

##### 仓储已经默认实现的接口

```csharp
/// <summary>
/// IBaseServices
/// </summary>
/// <typeparam name="TEntity">BaseService TEntity</typeparam>
public interface IBaseServices<TEntity> where TEntity : class
{
    /// <summary>
    /// QueryPrimaryKey - query entity by primaryKey
    /// </summary>
    /// <param name="entity">e.g. new Order() { Id=1 }. the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <returns>TEntity</returns>
    TEntity QueryPrimaryKey(TEntity entity, bool useSqlSugarCache = false);
    
    /// <summary>
    /// QueryPrimaryKeyAsync - query entity by primaryKey 
    /// </summary>
    /// <param name="entity">e.g. new Order() { Id=1 }. the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <returns>Task&lt;TEntity&gt;  </returns>
    Task<TEntity> QueryPrimaryKeyAsync(TEntity entity, bool useSqlSugarCache = false);

    /// <summary>
    /// QueryPrimaryKeys - query entities by primaryKey
    /// </summary>
    /// <param name="entities">e.g. new List&lt;Order&gt;(){ new Order { Id = 1} } . the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>List&lt;TEntity&gt;</returns>
    List<TEntity> QueryPrimaryKeys(List<TEntity> entities, bool useSqlSugarCache = false);
    
    /// <summary>
    /// QueryPrimaryKeysAsync - query entities by primaryKey
    /// </summary>
    /// <param name="entities">e.g. new List&lt;Order&gt;(){ new Order { Id = 1} } . the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>Task&lt;List&lt;TEntity&gt;&gt;</returns>
    Task<List<TEntity>> QueryPrimaryKeysAsync(List<TEntity> entities, bool useSqlSugarCache = false);

    /// <summary>
    /// Query - query entities by whereExpression and sort result by strOrderByFields
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>List&lt;TEntity&gt;</returns>
    List<TEntity> Query(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        bool useSqlSugarCache = false);
    
    /// <summary>
    /// QueryAsync - query entities by whereExpression and sort result by strOrderByFields
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>Task&lt;List&lt;TEntity&gt;&gt;</returns>
    Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        bool useSqlSugarCache = false);

    /// <summary>
    /// QuerySql - query by sql
    /// </summary>
    /// <param name="strSql">query sql</param>
    /// <param name="parameters">query parameters</param>
    /// <returns>List&lt;TEntity&gt;</returns>
    List<TEntity> QuerySql(string strSql, SugarParameter[]? parameters = null);
    
    /// <summary>
    /// Query entities by Sql 
    /// </summary>
    /// <param name="strSql"></param>
    /// <param name="parameters"></param>
    /// <returns>Task&lt;List&lt;TEntity&gt;&gt;</returns>
    Task<List<TEntity>> QuerySqlAsync(string strSql, SugarParameter[]? parameters = null);

    /// <summary>
    /// query return dataTable
    /// </summary>
    /// <param name="strSql">query sql</param>
    /// <param name="parameters">query parameters</param>
    /// <returns>DataTable</returns>
    DataTable QueryTable(string strSql, SugarParameter[]? parameters = null);
    
    /// <summary>
    /// query return dataTable async
    /// </summary>
    /// <param name="strSql">query sql</param>
    /// <param name="parameters">query parameters</param>
    /// <returns>Task&lt;List&lt;DataTable&gt;&gt;</returns>
    Task<DataTable> QueryTableAsync(string strSql, SugarParameter[]? parameters = null);

    /// <summary>
    /// QueryPaging - query entities by whereExpression and sort result by strOrderByFields then return toPageListAsync
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="startPage">default 1,is not index start 1 not 0</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>List&lt;TEntity&gt;</returns>
    List<TEntity> QueryPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false);
    
    /// <summary>
    /// QueryPagingAsync - query entities by whereExpression and sort result by strOrderByFields then return toPageListAsync
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="startPage">default 1,is not index start 1 not 0</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>Task&lt;List&lt;DataTable&gt;&gt;</returns>
    Task<List<TEntity>> QueryPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false);

    /// <summary>
    /// QueryViewModelPaging - query ViewModel&lt;TEntity&gt; by whereExpression and sort result by strOrderByFields then return toPageListAsync
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="startPage">default 1,is not index start 1 not 0</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>List&lt;TEntity&gt;</returns>
    ViewModel<TEntity> QueryViewModelPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false);
    
    /// <summary>
    /// QueryViewModelPagingAsync - query ViewModel&lt;TEntity&gt; by whereExpression and sort result by strOrderByFields then return toPageListAsync
    /// </summary>
    /// <param name="whereExpression">if <paramref name="whereExpression" /> is null then query all entities</param>
    /// <param name="orderByExpression">if <paramref name="orderByExpression" /> is null then not sort</param>
    /// <param name="isAsc">sort type default Asc.must has orderByExpression</param>
    /// <param name="startPage">default 1,is not index start 1 not 0</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="useSqlSugarCache">if <paramref name="useSqlSugarCache" /> was true; otherwise.default false</param>
    /// <returns>Task&lt;List&lt;TEntity&gt;&gt;</returns>
    Task<ViewModel<TEntity>> QueryViewModelPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1, 
        int pageSize = 20, 
        bool useSqlSugarCache = false);

    /// <summary>
    /// Add - add entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>Identity</returns>
    int Add(TEntity entity);
    
    /// <summary>
    /// AddAsync - add entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>Task&lt;Identity&gt;</returns>
    Task<int> AddAsync(TEntity entity);

    /// <summary>
    /// AddListEntities - add list entities
    /// </summary>
    /// <param name="entities">entities</param>
    /// <returns>Identity</returns>
    int AddListEntities(List<TEntity> entities);
    /// <summary>
    /// AddListEntitiesAsync - add list entities
    /// </summary>
    /// <param name="entities">entities</param>
    /// <returns>Task&lt;Identity&gt;</returns>
    Task<int> AddListEntitiesAsync(List<TEntity> entities);

    /// <summary>
    /// AddBulkEntities - add bulk list entities
    /// </summary>
    /// <param name="entities">entities</param>
    /// <param name="pageSize">pageSize</param>
    /// <returns>Identity</returns>
    int AddBulkEntities(List<TEntity> entities, int? pageSize = null);
    
    /// <summary>
    /// AddBulkEntitiesAsync - add bulk list entities
    /// </summary>
    /// <param name="entities">entities</param>
    /// <param name="pageSize">pageSize</param>
    /// <returns>Task&lt;Identity&gt;</returns>
    Task<int> AddBulkEntitiesAsync(List<TEntity> entities,int? pageSize=null);

    /// <summary>
    /// DeleteEntityById - delete entity
    /// </summary>
    /// <param name="id">the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <returns>bool HasChange was true.otherwise</returns>
    bool DeleteEntityById(object id);
    /// <summary>
    /// DeleteEntityByIdAsync - delete entity
    /// </summary>
    /// <param name="id">the id column must has attr [SugarColumn(IsPrimaryKey=true)]）</param>
    /// <returns>Task&lt;bool&gt; HasChange was true.otherwise</returns>
    Task<bool> DeleteEntityByIdAsync(object id);

    /// <summary>
    /// Delete - delete entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>bool HasChange was true.otherwise</returns>
    bool Delete(TEntity entity);
    /// <summary>
    /// DeleteAsync - delete entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>Task&lt;bool&gt; HasChange was true.otherwise</returns>
    Task<bool> DeleteAsync(TEntity entity);

    /// <summary>
    /// DeleteEntitiesByIds - delete entities by ids
    /// </summary>
    /// <param name="ids">ids</param>
    /// <returns>bool HasChange was true.otherwise</returns>
    bool DeleteEntitiesByIds(object[] ids);
    /// <summary>
    /// DeleteEntitiesByIdsAsync - delete entities by ids
    /// </summary>
    /// <param name="ids">ids</param>
    /// <returns>Task&lt;bool&gt; HasChange was true.otherwise</returns>
    Task<bool> DeleteEntitiesByIdsAsync(object[] ids);
    
    /// <summary>
    /// Update - update entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>bool HasChange was true.otherwise</returns>
    bool Update(TEntity entity);
    /// <summary>
    /// UpdateAsync - update entity
    /// </summary>
    /// <param name="entity">entity</param>
    /// <returns>Task&lt;bool&gt; HasChange was true.otherwise</returns>
    Task<bool> UpdateAsync(TEntity entity);

    /// <summary>
    /// UpdateBulkEntities - update bulk entities
    /// </summary>
    /// <param name="entities">entities</param>
    /// <param name="pageSize">pageSize</param>
    /// <returns>Identity</returns>
    int UpdateBulkEntities(List<TEntity> entities, int? pageSize = null);

    /// <summary>
    /// UpdateBulkEntitiesAsync - update bulk entities
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="pageSize"></param>
    /// <returns>Task&lt;Identity&gt;</returns>
    Task<int> UpdateBulkEntitiesAsync(List<TEntity> entities, int? pageSize = null);
    
    /// <summary>
    /// QueryTree - query entity to tree struct
    /// </summary>
    /// <param name="childListExpression">childListExpression e.g. it=>it.Child    child in model is  [SqlSugar.SugarColumn(IsIgnore = true)]public List&lt;Tree&gt; Child { get; set; }</param>
    /// <param name="parentIdExpression">parentIdExpression  e.g. it=>it.ParentId    ParentId in model is  public int ParentId { get; set; }//父级字段</param>
    /// <param name="rootValue"> parentId is 0 or null ,default  top node pid is 0 or null</param>
    /// <param name="childIds">primary key filter condition  e.g. object [] inIds=new object[]{11,12} </param>
    /// <returns>List&lt;TEntity&gt;</returns>
    List<TEntity> QueryTree(
        Expression<Func<TEntity, IEnumerable<object>>> childListExpression,
        Expression<Func<TEntity, object>> parentIdExpression,
        object rootValue,
        object[]? childIds = null);

    /// <summary>
    /// QueryTreeAsync - query entity to tree struct
    /// </summary>
    /// <param name="childListExpression">childListExpression e.g. it=>it.Child    child in model is  [SqlSugar.SugarColumn(IsIgnore = true)]public List&lt;Tree&gt; Child { get; set; }</param>
    /// <param name="parentIdExpression">parentIdExpression  e.g. it=>it.ParentId    ParentId in model is  public int ParentId { get; set; }//父级字段</param>
    /// <param name="rootValue"> parentId is 0 or null ,default  top node pid is 0 or null</param>
    /// <param name="childIds">primary key filter condition  e.g. object [] inIds=new object[]{11,12} </param>
    /// <returns>Task&lt;List&lt;DataTable&gt;&gt;</returns>
    Task<List<TEntity>> QueryTreeAsync(
        Expression<Func<TEntity, IEnumerable<object>>> childListExpression,
        Expression<Func<TEntity, object>> parentIdExpression,
        object rootValue,
        object[]? childIds = null);
}
```
















