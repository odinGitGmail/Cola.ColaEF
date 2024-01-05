using System.Data;
using System.Linq.Expressions;
using Cola.ColaEF.BaseUnitOfWork;
using Cola.Core.Models.ColaEF;
using SqlSugar;
using Cola.CoreUtils.Extensions;
namespace Cola.ColaEF.EFRepository;
/// <summary>
/// BaseRepository
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
{
    #region variable and ctor

    private readonly IUnitOfWork _unitOfWork;
    private SqlSugarClient _dbBase;

    /// <summary>
    /// BaseRepository ctor
    /// </summary>
    /// <param name="unitOfWork"></param>
    public BaseRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dbBase = unitOfWork.GetDbClient();
    }

    /// <summary>
    /// _db ISqlSugarClient
    /// </summary>
    private ISqlSugarClient Db => _dbBase;
    
    /// <summary>
    /// public  ISqlSugarClient DbClient
    /// </summary>
    public ISqlSugarClient DbClient => _dbBase;
    

    #endregion

    #region queryPrimaryKey

    private ISugarQueryable<TEntity> QueryablePrimaryKey(TEntity entity, bool useSqlSugarCache = false)
    {
        return Db.Queryable<TEntity>().WithCacheIF(useSqlSugarCache).WhereClassByPrimaryKey(entity);
    }
    public TEntity QueryPrimaryKey(TEntity entity, bool useSqlSugarCache = false)
    {
        return QueryablePrimaryKey(entity, useSqlSugarCache).Single();
    }

    public Task<TEntity> QueryPrimaryKeyAsync(TEntity entity, bool useSqlSugarCache = false)
    {
        return QueryablePrimaryKey(entity, useSqlSugarCache).SingleAsync();
    }

    #endregion

    #region queryPrimaryKeys

    private ISugarQueryable<TEntity> QueryablePrimaryKeys(List<TEntity> entities, bool useSqlSugarCache = false)
    {
        return Db.Queryable<TEntity>().WithCacheIF(useSqlSugarCache).WhereClassByPrimaryKey(entities);
    }
    
    public List<TEntity> QueryPrimaryKeys(List<TEntity> entities, bool useSqlSugarCache = false)
    {
        return QueryablePrimaryKeys(entities, useSqlSugarCache).ToList();
    }
    
    public Task<List<TEntity>> QueryPrimaryKeysAsync(List<TEntity> entities, bool useSqlSugarCache = false)
    {
        return QueryablePrimaryKeys(entities, useSqlSugarCache).ToListAsync();
    }

    #endregion

    #region query not pageing

    private ISugarQueryable<TEntity> Queryable(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        bool useSqlSugarCache = false)
    {
        return Db
            .Queryable<TEntity>()
            .WithCacheIF(useSqlSugarCache)
            .WhereIF(whereExpression != null, whereExpression)
            .OrderByIF(orderByExpression != null, orderByExpression, isAsc ? OrderByType.Asc : OrderByType.Desc);
    }
    
    public List<TEntity> Query(
        Expression<Func<TEntity, bool>>? whereExpression=null,
        Expression<Func<TEntity, object>>? orderByExpression=null,
        bool isAsc = true,
        bool useSqlSugarCache = false)
    {
        return Queryable(
            whereExpression,
            orderByExpression,
            isAsc,
            useSqlSugarCache).ToList();
    }

    public Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? whereExpression=null,
        Expression<Func<TEntity, object>>? orderByExpression=null,
        bool isAsc = true,
        bool useSqlSugarCache = false)
    {
        return Queryable(
            whereExpression,
            orderByExpression,
            isAsc,
            useSqlSugarCache).ToListAsync();
    }

    #endregion

    #region query by sql

    public List<TEntity> QuerySql(string strSql, SugarParameter[]? parameters = null)
    {
        return Db.Ado.SqlQuery<TEntity>(strSql, parameters);
    }
    public Task<List<TEntity>> QuerySqlAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return Db.Ado.SqlQueryAsync<TEntity>(strSql, parameters);
    }

    #endregion

    #region query by sql return datatable

    public DataTable QueryTable(string strSql, SugarParameter[]? parameters = null)
    {
        return Db.Ado.GetDataTable(strSql, parameters);
    }
    public Task<DataTable> QueryTableAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return Db.Ado.GetDataTableAsync(strSql, parameters);
    }

    #endregion

    #region query has pageing
    
    public List<TEntity> QueryPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return Queryable(
                whereExpression,
                orderByExpression,
                isAsc,
                useSqlSugarCache).ToPageList(startPage, pageSize);
    }

    public Task<List<TEntity>> QueryPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return Db
            .Queryable<TEntity>()
            .WithCacheIF(useSqlSugarCache)
            .WhereIF(whereExpression != null, whereExpression)
            .OrderByIF(orderByExpression != null, orderByExpression, isAsc ? OrderByType.Asc : OrderByType.Desc)
            .ToPageListAsync(startPage, pageSize);
    }

    #endregion

    #region query viewModelpaging

    public ViewModel<TEntity> QueryViewModelPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1, 
        int pageSize = 20, 
        bool useSqlSugarCache = false)
    {
        RefAsync<int> totalCount = 0;
        var list = QueryPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
        int pageCount = Math.Ceiling((totalCount.Value.IntToDecimal() / pageSize.IntToDecimal())).DecimalToInt();
        return new ViewModel<TEntity>() { TotalCount = totalCount, PageCount = pageCount, CurrnetPage = startPage, PageSize = pageSize, Data = list };
    }
    
    public async Task<ViewModel<TEntity>> QueryViewModelPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1, 
        int pageSize = 20, 
        bool useSqlSugarCache = false)
    {
        RefAsync<int> totalCount = 0;
        var list = await QueryPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
        int pageCount = Math.Ceiling((totalCount.Value.IntToDecimal() / pageSize.IntToDecimal())).DecimalToInt();
        return new ViewModel<TEntity>() { TotalCount = totalCount, PageCount = pageCount, CurrnetPage = startPage, PageSize = pageSize, Data = list };
    }

    #endregion

    #region add entity

    private IInsertable<TEntity> AddEntity(TEntity entity)
    {
        return Db.Insertable(entity);
    }
    public int Add(TEntity entity)
    {
        return AddEntity(entity).ExecuteReturnIdentity();
    }
    
    public Task<int> AddAsync(TEntity entity)
    {
        return AddEntity(entity).ExecuteReturnIdentityAsync();
    }

    #endregion

    #region add list entities

    public IInsertable<TEntity> AddListEntity(List<TEntity> entities)
    {
        return Db.Insertable(entities.ToArray());
    }
    public int AddListEntities(List<TEntity> entities)
    {
        return AddListEntity(entities).ExecuteCommand();
    }
    public Task<int> AddListEntitiesAsync(List<TEntity> entities)
    {
        return AddListEntity(entities).ExecuteCommandAsync();
    }

    #endregion

    #region add bulk entites

    public int AddBulkEntities(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? Db.Fastest<TEntity>().PageSize(pageSize.Value).BulkCopy(entities)
            : Db.Fastest<TEntity>().BulkCopy(entities);
    }
    
    public Task<int> AddBulkEntitiesAsync(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? Db.Fastest<TEntity>().PageSize(pageSize.Value).BulkCopyAsync(entities)
            : Db.Fastest<TEntity>().BulkCopyAsync(entities);

    }

    #endregion

    #region delete entity by id

    private IDeleteable<TEntity> DeleteById(object id)
    {
        return Db.Deleteable<TEntity>(id);
    }
    public bool DeleteEntityById(object id)
    {
        return DeleteById(id).ExecuteCommandHasChange();
    }
    public Task<bool> DeleteEntityByIdAsync(object id)
    {
        return DeleteById(id).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region delete entity

    private IDeleteable<TEntity> DeleteEntity(TEntity entity)
    {
        return Db.Deleteable(entity);
    }
    public bool Delete(TEntity entity)
    {
        return DeleteEntity(entity).ExecuteCommandHasChange();
    }
    public Task<bool> DeleteAsync(TEntity entity)
    {
        return DeleteEntity(entity).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region delete entities by ids

    private IDeleteable<TEntity> DeleteListEntityByIds(object[] ids)
    {
        return Db.Deleteable<TEntity>().In(ids);
    }
    public bool DeleteEntitiesByIds(object[] ids)
    {
        return DeleteListEntityByIds(ids).ExecuteCommandHasChange();
    }
    public Task<bool> DeleteEntitiesByIdsAsync(object[] ids)
    {
        return DeleteListEntityByIds(ids).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region update entity

    private IUpdateable<TEntity> UpdateEntity(TEntity entity)
    {
        return Db.Updateable(entity);
    }
    public bool Update(TEntity entity)
    {
        return UpdateEntity(entity).ExecuteCommandHasChange();
    }
    public Task<bool> UpdateAsync(TEntity entity)
    {
        return Db.Updateable(entity).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region update bulk entities

    public int UpdateBulkEntities(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? Db.Fastest<TEntity>().PageSize(pageSize.Value).BulkUpdate(entities)
            : Db.Fastest<TEntity>().BulkUpdate(entities);
    }
    
    public Task<int> UpdateBulkEntitiesAsync(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? Db.Fastest<TEntity>().PageSize(pageSize.Value).BulkUpdateAsync(entities)
            : Db.Fastest<TEntity>().BulkUpdateAsync(entities);

    }

    #endregion
    
}