﻿using System.Data;
using System.Linq.Expressions;
using Cola.Core.Models.ColaEF;
using SqlSugar;

namespace Cola.ColaEF.EFRepository;
/// <summary>
/// IBaseRepository
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBaseRepository<TEntity> where TEntity : class
{
    ISqlSugarClient DbClient { get; }

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
}