using PRN231_Group11_Assignment1_Repo.Repository;

namespace PRN231_Group11_Assignment1_Repo.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    void Save();
}