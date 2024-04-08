using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN231_Group11_Assignment1_API.DBContext;

namespace PRN231_Group11_Assignment1_Repo.Repository;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly FStoreDBContext _context;
        private readonly DbSet<TEntity> dbSet;

        public Repository(FStoreDBContext context)
        {
            _context = context;
            dbSet = _context.Set<TEntity>();
        }

        public void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                _context.Set<TEntity>().Attach(entity);
            }

            _context.Set<TEntity>().Remove(entity);
        }

        public void Delete(object id)
        {
            TEntity? entity = _context.Set<TEntity>().Find(id);
            if (entity != null) Delete(entity);
        }

        public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression,
            int? pageIndex = null,
            int? pageSize = null,
            params Expression<Func<TEntity, object>>[]? includeProperties)
        {
            IQueryable<TEntity> query = dbSet.Where(expression);

            if (includeProperties is not null)
            {
                query = includeProperties.Aggregate(query,
                    (current, includeProperty) => current.Include(includeProperty));
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;
                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query;
        }

        public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression,
            params Expression<Func<TEntity, object>>[]? includeProperties)
        {
            IQueryable<TEntity> query = dbSet.Where(expression);

            if (includeProperties is not null)
            {
                query = includeProperties.Aggregate(query,
                    (current, includeProperty) => current.Include(includeProperty));
            }

            return query;
        }

        public TEntity? GetById(object? id) => _context.Set<TEntity>().Find(id);

        public void Insert(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }


        public virtual IEnumerable<TEntity> Get(
            int? pageIndex = null,
            int? pageSize = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            if (!pageIndex.HasValue || !pageSize.HasValue) return query.ToList();
            int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;
            query = query.Skip(validPageIndex).Take(validPageSize);

            return query.ToList();
        }
    }