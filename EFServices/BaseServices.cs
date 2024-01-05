using System.Data;
using System.Linq.Expressions;
using Cola.ColaEF.EFRepository;
using Cola.Core.Models.ColaEF;
using SqlSugar;

namespace Cola.ColaEF.EFServices;

/// <summary>
/// BaseServices
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class BaseServices<TEntity> : IBaseServices<TEntity> where TEntity : class, new()
{
    //通过在子类的构造函数中注入，这里是基类，不用构造函数
    protected readonly IBaseRepository<TEntity> baseRepository;

    public TEntity QueryPrimaryKey(TEntity entity, bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPrimaryKey(entity, useSqlSugarCache);
    }

    public Task<TEntity> QueryPrimaryKeyAsync(TEntity entity, bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPrimaryKeyAsync(entity, useSqlSugarCache);
    }

    public List<TEntity> QueryPrimaryKeys(List<TEntity> entities, bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPrimaryKeys(entities, useSqlSugarCache);
    }

    public Task<List<TEntity>> QueryPrimaryKeysAsync(List<TEntity> entities, bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPrimaryKeysAsync(entities, useSqlSugarCache);
    }

    public List<TEntity> Query(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        bool useSqlSugarCache = false)
    {
        return baseRepository.Query(whereExpression, orderByExpression, isAsc, useSqlSugarCache);
    }

    public Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        bool useSqlSugarCache = false)
    {
        return baseRepository.QueryAsync(whereExpression, orderByExpression, isAsc, useSqlSugarCache);
    }

    public List<TEntity> QuerySql(string strSql, SugarParameter[]? parameters = null)
    {
        return baseRepository.QuerySql(strSql, parameters);
    }

    public Task<List<TEntity>> QuerySqlAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return baseRepository.QuerySqlAsync(strSql, parameters);
    }

    public DataTable QueryTable(string strSql, SugarParameter[]? parameters = null)
    {
        return baseRepository.QueryTable(strSql, parameters);
    }

    public Task<DataTable> QueryTableAsync(string strSql, SugarParameter[]? parameters = null)
    {
        return baseRepository.QueryTableAsync(strSql, parameters);
    }

    public List<TEntity> QueryPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
    }

    public Task<List<TEntity>> QueryPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return baseRepository.QueryPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
    }

    public ViewModel<TEntity> QueryViewModelPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return baseRepository.QueryViewModelPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
    }

    public Task<ViewModel<TEntity>> QueryViewModelPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20,
        bool useSqlSugarCache = false)
    {
        return baseRepository.QueryViewModelPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize,
            useSqlSugarCache);
    }

    public int Add(TEntity entity)
    {
        return baseRepository.Add(entity);
    }

    public Task<int> AddAsync(TEntity entity)
    {
        return baseRepository.AddAsync(entity);
    }

    public int AddListEntities(List<TEntity> entities)
    {
        return baseRepository.AddListEntities(entities);
    }

    public Task<int> AddListEntitiesAsync(List<TEntity> entities)
    {
        return baseRepository.AddListEntitiesAsync(entities);
    }

    public int AddBulkEntities(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.AddBulkEntities(entities, pageSize);
    }

    public Task<int> AddBulkEntitiesAsync(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.AddBulkEntitiesAsync(entities, pageSize);
    }

    public bool DeleteEntityById(object id)
    {
        return baseRepository.DeleteEntityById(id);
    }

    public Task<bool> DeleteEntityByIdAsync(object id)
    {
        return baseRepository.DeleteEntityByIdAsync(id);
    }

    public bool Delete(TEntity entity)
    {
        return baseRepository.Delete(entity);
    }

    public Task<bool> DeleteAsync(TEntity entity)
    {
        return baseRepository.DeleteAsync(entity);
    }

    public bool DeleteEntitiesByIds(object[] ids)
    {
        return baseRepository.DeleteEntitiesByIds(ids);
    }

    public Task<bool> DeleteEntitiesByIdsAsync(object[] ids)
    {
        return baseRepository.DeleteEntitiesByIdsAsync(ids);
    }

    public bool Update(TEntity entity)
    {
        return baseRepository.Update(entity);
    }

    public Task<bool> UpdateAsync(TEntity entity)
    {
        return baseRepository.UpdateAsync(entity);
    }

    public int UpdateBulkEntities(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.UpdateBulkEntities(entities, pageSize);
    }

    public Task<int> UpdateBulkEntitiesAsync(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.UpdateBulkEntitiesAsync(entities, pageSize);
    }
}