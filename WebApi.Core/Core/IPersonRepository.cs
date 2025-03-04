using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface IPersonRepository {
   Task<Person?> FindByIdAsync(Guid id);
   Task<Person?> FindByNameAsync(string name);
   Task<IEnumerable<Person>> SelectAllAsync();
   
   Task AddAsync(Person person);
   Task UpdateAsync(Person updPerson);
   Task RemoveAsync(Person person); 
   
   //-- Join Cars -------------------------------------------------------------- 
   Task<Person?> FindByIdWithCarsAsync(Guid id);
   
}