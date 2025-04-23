using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Core.DomainModel.Entities;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Data.Repositories;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleRepositoryUt : BaseRepository {

   #region PersonOnly
   [Fact]
   public async Task  FindByIdAsyncUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act
      var actual = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      // Assert
      Assert.Equivalent(_seed.Person1, actual);
   }
   
   [Fact]
   public async Task AddUt() {
      // Arrange
      var person = _seed.Person1;
      // Act
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync();
      // Assert
      var actual = await _peopleRepository.FindByIdAsync(person.Id);
      Assert.Equal(person, actual);
   }

   [Fact]
   public async Task AddRangeUt() {
      // Arrange
      var expected = _seed.People;
      // Act
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Assert
      var actual = await _peopleRepository.SelectAllAsync();
      Assert.Equivalent(expected, actual);
   }
   
   [Fact]
   public async Task UpdateUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act
      // retrieve person from database to track it
      var actualPerson = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      Assert.NotNull(actualPerson);
      // domain model
      actualPerson.Update("Erika","Meier", _seed.Person1.Email, _seed.Person1.Phone);
      // update person in repository
      _peopleRepository.Update(actualPerson);
      await _dataContext.SaveAllChangesAsync();
      // Assert
      var actual = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      Assert.Equivalent(actualPerson, actual);
   }
   
   [Fact]
   public async Task RemoveUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act
      _peopleRepository.Remove(_seed.Person1);
      await _dataContext.SaveAllChangesAsync();
      // Assert
      var actual = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      Assert.Null(actual);
   }
   
   [Fact]
   public async Task SelectByNameUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      var expected = new List<Person> { _seed.Person1, _seed.Person2 };
      
      // Act
      var actual = await _peopleRepository.SelectByNameAsync("Muster"); 
      
      // Assert
      Assert.Equivalent(expected, actual);
   }
   #endregion

   /*
   #region PersonJoinCars
   [Fact]
   public void FindByIdJoinCarsUt() {
      // Arrange
      // add person to repository first
      _peopleRepository.Add(_seed.Person1);  
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      // Act
      var person = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(person);
      // Domain model
      var car1 = _seed.Car1;
      var car2 = _seed.Car2;
      person.AddCar(car1);
      person.AddCar(car2);
      _dataContext.LogChangeTracker("AddJoinCarsUt");
      // add the cars to the repository
      _carsRepository.Add(car1);
      _carsRepository.Add(car2);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      // Assert 
      var actual = _peopleRepository.FindByIdJoinCars(person.Id);      
      Assert.NotNull(actual);
      
      Console.WriteLine(BaseRepository.ToPrettyJson("person", person));
     
      Console.WriteLine(BaseRepository.ToPrettyJson("actual", actual));
      
      
      // Assert
      var comparison = new ComparisonBuilder()
         .IgnoreCircularReferences()
         .Create();
      Assert.True(person.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public void DeleteWithCarsCascadingUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      var person = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(person);
      person.AddCar(_seed.Car1);
      person.AddCar(_seed.Car2);
      _carsRepository.Add(_seed.Car1);
      _carsRepository.Add(_seed.Car2);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();

      // Act
      _peopleRepository.Remove(_seed.Person1); 
      _dataContext.SaveAllChanges();

      // Assert
      var actualPerson= _peopleRepository.FindById(_seed.Person1.Id);
      var actualCar1 = _carsRepository.FindById(_seed.Car1.Id);
      var actualCar2 = _carsRepository.FindById(_seed.Car2.Id);

      Assert.Null(actualPerson);
      Assert.Null(actualCar1);
      Assert.Null(actualCar2);

   }
   #endregion
   */
   
}