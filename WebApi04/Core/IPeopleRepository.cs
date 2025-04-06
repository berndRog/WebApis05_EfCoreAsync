using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface IPeopleRepository: IBaseRepository<Person> {
   
   Task<IEnumerable<Person>?> SelectByNameAsync(string namePattern);
   Task<Person?> FindByIdJoinCarsAsync(Guid id);
   
}