using System.Linq.Expressions;

namespace PRN231_Group11_Assignment1_Repo.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    void Insert(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    void Delete(object id);
    TEntity? GetById(object? id , params Expression<Func<TEntity, object>>[] includeProperties);

    IEnumerable<TEntity> Get(
        int? pageIndex = null,
        int? pageSize = null,
        params Expression<Func<TEntity, object>>[] includeProperties);
        
    IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression, int? pageIndex, int? pageSize, params Expression<Func<TEntity, object>>[]? includeProperties);
    IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression, params Expression<Func<TEntity, object>>[]? includeProperties);
}