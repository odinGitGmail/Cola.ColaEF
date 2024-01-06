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

    public TEntity QueryPrimaryKey(TEntity entity)
    {
        return baseRepository.QueryPrimaryKey(entity);
    }

    public Task<TEntity> QueryPrimaryKeyAsync(TEntity entity)
    {
        return baseRepository.QueryPrimaryKeyAsync(entity);
    }

    public List<TEntity> QueryPrimaryKeys(List<TEntity> entities)
    {
        return baseRepository.QueryPrimaryKeys(entities);
    }

    public Task<List<TEntity>> QueryPrimaryKeysAsync(List<TEntity> entities)
    {
        return baseRepository.QueryPrimaryKeysAsync(entities);
    }

    public List<TEntity> Query(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true)
    {
        return baseRepository.Query(whereExpression, orderByExpression, isAsc);
    }

    public Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true)
    {
        return baseRepository.QueryAsync(whereExpression, orderByExpression, isAsc);
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
        int pageSize = 20)
    {
        return baseRepository.QueryPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
    }

    public Task<List<TEntity>> QueryPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20)
    {
        return baseRepository.QueryPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
    }

    public ViewModel<TEntity> QueryViewModelPaging(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20)
    {
        return baseRepository.QueryViewModelPaging(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
    }

    public Task<ViewModel<TEntity>> QueryViewModelPagingAsync(
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool isAsc = true,
        int startPage = 1,
        int pageSize = 20)
    {
        return baseRepository.QueryViewModelPagingAsync(
            whereExpression,
            orderByExpression,
            isAsc,
            startPage,
            pageSize);
    }

    public int Add(TEntity entity)
    {
        return baseRepository.Add(entity);
    }

    public async Task<int> AddAsync(TEntity entity)
    {
        return await baseRepository.AddAsync(entity);
    }

    public int AddListEntities(List<TEntity> entities)
    {
        return baseRepository.AddListEntities(entities);
    }

    public async Task<int> AddListEntitiesAsync(List<TEntity> entities)
    {
        return await baseRepository.AddListEntitiesAsync(entities);
    }

    public int AddBulkEntities(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.AddBulkEntities(entities, pageSize);
    }

    public async Task<int> AddBulkEntitiesAsync(List<TEntity> entities, int? pageSize = null)
    {
        return await baseRepository.AddBulkEntitiesAsync(entities, pageSize);
    }

    public bool DeleteEntityById(object id)
    {
        return baseRepository.DeleteEntityById(id);
    }

    public async Task<bool> DeleteEntityByIdAsync(object id)
    {
        return await baseRepository.DeleteEntityByIdAsync(id);
    }

    public bool Delete(TEntity entity)
    {
        return baseRepository.Delete(entity);
    }

    public async Task<bool> DeleteAsync(TEntity entity)
    {
        return await baseRepository.DeleteAsync(entity);
    }

    public bool DeleteEntitiesByIds(object[] ids)
    {
        return baseRepository.DeleteEntitiesByIds(ids);
    }

    public async Task<bool> DeleteEntitiesByIdsAsync(object[] ids)
    {
        return await baseRepository.DeleteEntitiesByIdsAsync(ids);
    }

    public bool Update(TEntity entity)
    {
        return baseRepository.Update(entity);
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        return await baseRepository.UpdateAsync(entity);
    }

    public int UpdateBulkEntities(List<TEntity> entities, int? pageSize = null)
    {
        return baseRepository.UpdateBulkEntities(entities, pageSize);
    }

    public async Task<int> UpdateBulkEntitiesAsync(List<TEntity> entities, int? pageSize = null)
    {
        return await baseRepository.UpdateBulkEntitiesAsync(entities, pageSize);
    }
}