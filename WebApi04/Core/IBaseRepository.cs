using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface IBaseRepository<T> where T : AEntity {
   Task<T?> FindByIdAsync(Guid id, CancellationToken ctToken = default);
   Task<IEnumerable<T>> SelectAllAsync(CancellationToken ctToken = default);
   void Add(T entity);
   void AddRange(IEnumerable<T> entities);
   void Update(T updEntity);
   void Remove(T entity);
}