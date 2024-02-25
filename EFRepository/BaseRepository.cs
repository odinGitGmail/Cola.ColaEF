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

    private readonly ISqlSugarClient _dbBase;
    /// <summary>
    /// BaseRepository ctor
    /// </summary>
    /// <param name="unitOfWork"></param>
    public BaseRepository(IUnitOfWork unitOfWork)
    {
        _dbBase = unitOfWork.GetDbClient();
    }
    
    /// <summary>
    /// public  ISqlSugarClient DbClient
    /// </summary>
    public ISqlSugarClient DbClient => _dbBase;
    

    #endregion

    #region queryPrimaryKey

    private ISugarQueryable<TEntity> QueryablePrimaryKey(TEntity entity)
    {
        return _dbBase.Queryable<TEntity>().WhereClassByPrimaryKey(entity);
    }
    public TEntity QueryPrimaryKey(TEntity entity)
    {
        return QueryablePrimaryKey(entity).Single();
    }

    public async Task<TEntity> QueryPrimaryKeyAsync(TEntity entity)
    {
        return await QueryablePrimaryKey(entity).SingleAsync();
    }

    #endregion

    #region queryPrimaryKeys

    private ISugarQueryable<TEntity> QueryablePrimaryKeys(List<TEntity> entitiestrue)
    {
        return _dbBase.Queryable<TEntity>().WhereClassByPrimaryKey(entitiestrue);
    }
    
    public List<TEntity> QueryPrimaryKeys(List<TEntity> entitiestrue)
    {
        return QueryablePrimaryKeys(entitiestrue).ToList();
    }
    
    public async Task<List<TEntity>> QueryPrimaryKeysAsync(List<TEntity> entitiestrue)
    {
        return await QueryablePrimaryKeys(entitiestrue).ToListAsync();
    }

    #endregion

    #region query not pageing

    private ISugarQueryable<TEntity> Queryable(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true)
    {
        return _dbBase
            .Queryable<TEntity>()
            .WhereIF(whereExpression != null, whereExpression)
            .OrderByIF(orderByExpression != null, orderByExpression, isAsc ? OrderByType.Asc : OrderByType.Desc);
    }
    
    public List<TEntity> Query(
        Expression<Func<TEntity, bool>>? whereExpression=null,
        Expression<Func<TEntity, object>>? orderByExpression=null,
        bool isAsc = true)
    {
        return Queryable(
            whereExpression,
            orderByExpression,
            isAsc).ToList();
    }

    public async Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? whereExpression=null,
        Expression<Func<TEntity, object>>? orderByExpression=null,
        bool isAsc = true)
    {
        return await Queryable(
            whereExpression,
            orderByExpression,
            isAsc).ToListAsync();
    }

    #endregion

    #region query by sql

    public List<TEntity> QuerySql(string strSql, SugarParameter[]? parameters = null)
    {
        return _dbBase.Ado.SqlQuery<TEntity>(strSql, parameters);
    }
    public async Task<List<TEntity>> QuerySqlAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return await _dbBase.Ado.SqlQueryAsync<TEntity>(strSql, parameters);
    }

    #endregion

    #region query by sql return datatable

    public DataTable QueryTable(string strSql, SugarParameter[]? parameters = null)
    {
        return _dbBase.Ado.GetDataTable(strSql, parameters);
    }
    public async Task<DataTable> QueryTableAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return await _dbBase.Ado.GetDataTableAsync(strSql, parameters);
    }

    #endregion

    #region query has pageing
    
    public List<TEntity> QueryPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20)
    {
        return Queryable(
                whereExpression,
                orderByExpression,
                isAsc).ToPageList(startPage, pageSize);
    }

    public async Task<List<TEntity>> QueryPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20)
    {
        return await _dbBase
            .Queryable<TEntity>()
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
        int pageSize = 20)
    {
        RefAsync<int> totalCount = 0;
        var list = QueryPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
        int pageCount = Math.Ceiling((totalCount.Value.IntToDecimal() / pageSize.IntToDecimal())).DecimalToInt();
        return new ViewModel<TEntity>() { TotalCount = totalCount, PageCount = pageCount, CurrentPage = startPage, PageSize = pageSize, Data = list };
    }
    
    public async Task<ViewModel<TEntity>> QueryViewModelPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1, 
        int pageSize = 20)
    {
        RefAsync<int> totalCount = 0;
        var list = await QueryPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
        int pageCount = Math.Ceiling((totalCount.Value.IntToDecimal() / pageSize.IntToDecimal())).DecimalToInt();
        return new ViewModel<TEntity>() { TotalCount = totalCount, PageCount = pageCount, CurrentPage = startPage, PageSize = pageSize, Data = list };
    }

    #endregion

    #region add entity

    private IInsertable<TEntity> AddEntity(TEntity entity)
    {
        return _dbBase.Insertable(entity);
    }
    public int Add(TEntity entity)
    {
        return AddEntity(entity).ExecuteReturnIdentity();
    }
    
    public async Task<int> AddAsync(TEntity entity)
    {
        return await AddEntity(entity).ExecuteReturnIdentityAsync();
    }

    #endregion

    #region add list entities

    public IInsertable<TEntity> AddListEntity(List<TEntity> entities)
    {
        return _dbBase.Insertable(entities.ToArray());
    }
    public int AddListEntities(List<TEntity> entities)
    {
        return AddListEntity(entities).ExecuteCommand();
    }
    public async Task<int> AddListEntitiesAsync(List<TEntity> entities)
    {
        return await AddListEntity(entities).ExecuteCommandAsync();
    }

    #endregion

    #region add bulk entites

    public int AddBulkEntities(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? _dbBase.Fastest<TEntity>().PageSize(pageSize.Value).BulkCopy(entities)
            : _dbBase.Fastest<TEntity>().BulkCopy(entities);
    }
    
    public async Task<int> AddBulkEntitiesAsync(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? await _dbBase.Fastest<TEntity>().PageSize(pageSize.Value).BulkCopyAsync(entities)
            : await _dbBase.Fastest<TEntity>().BulkCopyAsync(entities);

    }

    #endregion

    #region delete entity by id

    private IDeleteable<TEntity> DeleteById(object id)
    {
        return _dbBase.Deleteable<TEntity>(id);
    }
    public bool DeleteEntityById(object id)
    {
        return DeleteById(id).ExecuteCommandHasChange();
    }
    public async Task<bool> DeleteEntityByIdAsync(object id)
    {
        return await DeleteById(id).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region delete entity

    private IDeleteable<TEntity> DeleteEntity(TEntity entity)
    {
        return _dbBase.Deleteable(entity);
    }
    public bool Delete(TEntity entity)
    {
        return DeleteEntity(entity).ExecuteCommandHasChange();
    }
    public async Task<bool> DeleteAsync(TEntity entity)
    {
        return await DeleteEntity(entity).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region delete entities by ids

    private IDeleteable<TEntity> DeleteListEntityByIds(object[] ids)
    {
        return _dbBase.Deleteable<TEntity>().In(ids);
    }
    public bool DeleteEntitiesByIds(object[] ids)
    {
        return DeleteListEntityByIds(ids).ExecuteCommandHasChange();
    }
    public async Task<bool> DeleteEntitiesByIdsAsync(object[] ids)
    {
        return await DeleteListEntityByIds(ids).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region update entity

    private IUpdateable<TEntity> UpdateEntity(TEntity entity)
    {
        return _dbBase.Updateable(entity);
    }
    public bool Update(TEntity entity)
    {
        return UpdateEntity(entity).ExecuteCommandHasChange();
    }
    public async Task<bool> UpdateAsync(TEntity entity)
    {
        return await _dbBase.Updateable(entity).ExecuteCommandHasChangeAsync();
    }

    #endregion

    #region update bulk entities

    public int UpdateBulkEntities(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? _dbBase.Fastest<TEntity>().PageSize(pageSize.Value).BulkUpdate(entities)
            : _dbBase.Fastest<TEntity>().BulkUpdate(entities);
    }
    
    public async Task<int> UpdateBulkEntitiesAsync(List<TEntity> entities,int? pageSize=null)
    {
        return pageSize != null
            ? await _dbBase.Fastest<TEntity>().PageSize(pageSize.Value).BulkUpdateAsync(entities)
            : await _dbBase.Fastest<TEntity>().BulkUpdateAsync(entities);
    }

    #endregion
    
    public List<TEntity> QueryTree(
        Expression<Func<TEntity,IEnumerable<object>>> childListExpression,
        Expression<Func<TEntity, object>> parentIdExpression,
        object rootValue,
        object[]? childIds = null)
    {
        return _dbBase.Queryable<TEntity>().ToTree(childListExpression, parentIdExpression, rootValue, childIds);
    }
    
    public async Task<List<TEntity>> QueryTreeAsync(
        Expression<Func<TEntity,IEnumerable<object>>> childListExpression,
        Expression<Func<TEntity, object>> parentIdExpression,
        object rootValue,
        object[]? childIds = null)
    {
        return await _dbBase.Queryable<TEntity>().ToTreeAsync(childListExpression, parentIdExpression, rootValue, childIds);
    }
    
}