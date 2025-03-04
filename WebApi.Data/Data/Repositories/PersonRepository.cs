using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

internal class PersonRepository(
   DataContext dataContext
) : IPersonRepository {
   
   private readonly IDataContext _dataContext = dataContext;
   private readonly DbSet<Person> _dbSetPeople = dataContext.People; // => Set<Person>

   public async Task<IEnumerable<Person>> SelectAllAsync() {
      var people = await _dbSetPeople.ToListAsync();
      dataContext.LogChangeTracker("Person: SelectAllAsync ");
      return people;
   }

   public async Task<Person?> FindByIdAsync(Guid id) {
      var person = await _dbSetPeople.FirstOrDefaultAsync(person => person.Id == id);
      dataContext.LogChangeTracker("Person: FindByIdAsync");
      return person;
   }

   public async Task<Person?> FindByNameAsync(string name) {
      var tokens = name.Trim().Split(" ");
      var firstName = string.Join(" ", tokens.Take(tokens.Length - 1));
      var lastName = tokens.Last();
      var person = await _dbSetPeople.FirstOrDefaultAsync(person =>
         person.FirstName == firstName && person.LastName == lastName);
      dataContext.LogChangeTracker("Person: FindByNameAsync");
      return person;
   }

   public async Task AddAsync(Person person) =>
      await _dbSetPeople.AddAsync(person);

   public async Task UpdateAsync(Person updPerson) {
      var person = await _dbSetPeople.FirstOrDefaultAsync(person => 
         person.Id == updPerson.Id);
      if (person == null) throw new Exception("Person to be updated not found");
      person.Update(updPerson);
   }

   public async Task RemoveAsync(Person person) {
      var pFound = await _dbSetPeople.FirstOrDefaultAsync(p => p.Id == person.Id);
      if (pFound == null) throw new Exception("Person to be removed not found");
      _dbSetPeople.Remove(pFound);
   }

   public async Task<Person?> FindByIdWithCarsAsync(Guid id) {
      var person =
         await _dbSetPeople
            .Where(person => person.Id == id)
            .Include(person => person.Cars)
            .FirstOrDefaultAsync();
      dataContext.LogChangeTracker("Person: FindByIdWithCarsAsync");
      return person;

   }
}