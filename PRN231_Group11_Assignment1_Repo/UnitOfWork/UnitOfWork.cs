using PRN231_Group11_Assignment1_API.DBContext;
using PRN231_Group11_Assignment1_Repo.Repository;

namespace PRN231_Group11_Assignment1_Repo.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly FStoreDBContext _context;

    public UnitOfWork(FStoreDBContext context)
    {
        _context = context;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        => new Repository<TEntity>(_context);

    public void Save()
    {
        _context.SaveChanges();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}