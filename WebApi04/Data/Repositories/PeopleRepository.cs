using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Data.Repositories_refactored;
namespace WebApi.Data.Repositories;

public class PeopleRepository(
   DataContext dc
) : ABaseRepository<Person>(dc), IPeopleRepository {
   
   public async Task<IEnumerable<Person>?> SelectByNameAsync(string namePattern) {
      if (string.IsNullOrWhiteSpace(namePattern))
         return null;
      var people = await _dbSet
         .Where(person => EF.Functions.Like(person.LastName, $"%{namePattern.Trim()}%"))
         .ToListAsync();
      _dataContext.LogChangeTracker("Person: FindByNamePatternAsync");
      return people;
   }   

   public async Task<Person?> FindByIdJoinCarsAsync(Guid id) {
      var person = await _dbSet
         .Where(person => person.Id == id)
         .Include(person => person.Cars)
         .FirstOrDefaultAsync();
      _dataContext.LogChangeTracker("Person: FindByIdWithCarsAsync");
      return person;

   }


}