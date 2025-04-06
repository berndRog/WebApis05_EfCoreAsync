using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface IBaseRepository<T> where T : AEntity {
   Task<T?> FindByIdAsync(Guid id);
   Task<IEnumerable<T>?> SelectAllAsync();
   Task AddAsync(T entity);
   Task AddRangeAsync(IEnumerable<T> entities);
   void Update(T updEntity);
   void Remove(T entity);
}