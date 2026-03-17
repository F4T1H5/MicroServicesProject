using CORE.APP.Domain;
using Microsoft.EntityFrameworkCore;

namespace CORE.APP.Services
{
    public abstract class Service<TEntity> : ServiceBase, IDisposable where TEntity : Entity, new()
    {
        private readonly DbContext _db;

        protected Service(DbContext db)
        {
            _db = db;
        }

        protected virtual IQueryable<TEntity> DbSet()
        {
            return _db.Set<TEntity>();
        }
        protected virtual int Save() => _db.SaveChanges();

        protected void Create(TEntity entity, bool save = true)
        {
            _db.Set<TEntity>().Add(entity);
            if (save)
                Save();
        }

        protected void Update(TEntity entity, bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                Save();
        }

        protected void Delete(TEntity entity, bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                Save();
        }

        protected virtual async Task<int> SaveAsync(CancellationToken cancellationToken) => await _db.SaveChangesAsync(cancellationToken);

        protected async Task CreateAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Add(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        protected async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        protected async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        public IQueryable<TRelationalEntity> DbSet<TRelationalEntity>() where TRelationalEntity : Entity, new()
        {
            return _db.Set<TRelationalEntity>().AsNoTracking();
        }

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            _db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}