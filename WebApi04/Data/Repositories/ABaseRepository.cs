using Microsoft.EntityFrameworkCore;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

public abstract class ABaseRepository<T>(
   DataContext dContext
) where T : AEntity {
   
   // fields
   protected readonly DbSet<T> _dbSet = dContext.Set<T>();
   protected readonly DataContext _dataContext = dContext;
   
   // virtual methods, can be overridden in derived classes
   public virtual async Task<T?> FindByIdAsync(Guid id, CancellationToken ctToken = default) {
      var entity = await _dbSet.FindAsync(id, ctToken);
      _dataContext.LogChangeTracker($"{typeof(T).Name}: FindById");
      return entity;
   }

   public virtual async Task<IEnumerable<T>> SelectAllAsync(CancellationToken ctToken = default) {
      var entities = await _dbSet.ToListAsync(cancellationToken: ctToken);
      _dataContext.LogChangeTracker($"{typeof(T).Name}: SelectAll");
      return entities;
   }

   public virtual void Add(T entity) =>
      _dbSet.Add(entity);

   public virtual void AddRange(IEnumerable<T> entities) =>
      _dbSet.AddRange(entities);

   // Update 
   public virtual void Update(T entity) {
      var existingEntity = _dbSet.Find(entity.Id);
      if (existingEntity == null)
         throw new ApplicationException($"Update failed, entity with given id not found");
      var entry = _dataContext.Entry(existingEntity);
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Modified;
   }
   
   public void Remove(T entity) {
      var existingEntity = _dbSet.Find(entity.Id);
      if (existingEntity == null)
         throw new ApplicationException($"Update failed, entity with given id not found");
      var entry = _dataContext.Entry(existingEntity);
      if (entry == null) throw new Exception($"{typeof(T).Name} to be removed not found");
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Deleted;
   }

}