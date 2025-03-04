using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

internal class PersonRepository(
   DataContext dataContext
) : IPersonRepository {
   
   private readonly IDataContext _dataContext = dataContext;
   private readonly DbSet<Person> _dbSetPeople = dataContext.People; // => Set<Person>

   public async Task<IEnumerable<Person>> SelectAllAsync() => 
      await _dbSetPeople.ToListAsync();
   
   public async Task<Person?> FindByIdAsync(Guid id) =>
      await _dbSetPeople.FirstOrDefaultAsync(person => person.Id == id);
   
   public async Task<Person?> FindByNameAsync(string name) {
      var tokens = name.Trim().Split(" ");
      var firstName = string.Join(" ", tokens.Take(tokens.Length - 1));
      var lastName = tokens.Last();
      return await _dbSetPeople.FirstOrDefaultAsync(person =>
         person.FirstName == firstName && person.LastName == lastName);
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

}