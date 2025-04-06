using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories_refactored;

public abstract class ABaseRepository<T>(
   DataContext dc
) where T : AEntity {
   
   // fields
   protected readonly DbSet<T> _dbSet = dc.Set<T>();
   protected readonly DataContext _dataContext = dc;
   
   // virtual methods, can be overridden in derived classes
   public virtual async Task<T?> FindByIdAsync(Guid id) {
      var entity = await _dbSet.FindAsync(id);
      _dataContext.LogChangeTracker($"{typeof(T).Name}: FindById");
      return entity;
   }

   public virtual async Task<IEnumerable<T>?> SelectAllAsync() {
      var entities = await _dbSet.ToListAsync();
      _dataContext.LogChangeTracker($"{typeof(T).Name}: SelectAll");
      return entities;
   }

   public virtual async Task AddAsync(T entity) =>
      await _dbSet.AddAsync(entity);

   public virtual async Task AddRangeAsync(IEnumerable<T> entities) =>
      await _dbSet.AddRangeAsync(entities);

   // Update 
   public virtual void Update(T entity) {
      var entry = _dataContext.Entry(entity);
      if (entry == null)
         throw new ApplicationException($"Update failed, entity with given id not found");
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Modified;
   }
   
   public void Remove(T entity) {
      var entry = _dataContext.Entry(entity);
      if (entry == null) throw new Exception($"{typeof(T).Name} to be removed not found");
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Deleted;
   }
}