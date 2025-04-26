using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

public class PeopleRepository(
   DataContext dContext
) : ABaseRepository<Person>(dContext), IPeopleRepository {
   
   public async Task<IEnumerable<Person>> SelectByNameAsync(
      string namePattern, 
      CancellationToken ctToken = default
   ){
      if (string.IsNullOrWhiteSpace(namePattern))
         return [];
      var people = await _dbSet
         .Where(person => EF.Functions.Like(person.LastName, $"%{namePattern.Trim()}%"))
         .ToListAsync(ctToken);
      _dataContext.LogChangeTracker("Person: FindByNamePatternAsync");
      return people;
   }   

   public async Task<Person?> FindByIdJoinCarsAsync(
      Guid id,
      CancellationToken ctToken = default
   ) {
      var person = await _dbSet
         .Where(person => person.Id == id)
         .Include(person => person.Cars)
         .FirstOrDefaultAsync(ctToken);
      _dataContext.LogChangeTracker("Person: FindByIdWithCarsAsync");
      return person;

   }
}