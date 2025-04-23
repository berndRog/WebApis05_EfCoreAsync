using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeepEqual;
using DeepEqual.Syntax;
using WebApi.Core.DomainModel.Entities;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Data.Repositories;

[Collection(nameof(SystemTestCollectionDefinition))]
public class CarsRepositoryUt : BaseRepository {
   
   [Fact]
   public async Task FindByIdAsyncUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // retrieve person from database to track it
      var actualPerson = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      Assert.NotNull(actualPerson);
      // domain model
      actualPerson.AddCar(_seed.Car1);
      actualPerson.AddCar(_seed.Car2);
      // add cars to database
      _carsRepository.Add(_seed.Car1);
      _carsRepository.Add(_seed.Car2);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act 
      var actual = await _carsRepository.FindByIdAsync(_seed.Car1.Id);
      var comparison = new ComparisonBuilder()
//       .IgnoreCircularReferences()
         .IgnoreProperty<Car>(c=> c.Person)
         .Create();
      Assert.True(_seed.Car1.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public async Task SelectAllAsyncUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // retrieve people from database to track it
      var people = await _peopleRepository.SelectAllAsync();
      Assert.NotNull(people);
      // domain model add cars to people
      var (actualPeople, actualCars) = 
         Seed.InitPeopleWithCars(people,_seed.Cars);
      Assert.NotNull(actualPeople);
      Assert.NotNull(actualCars);
      // add cars to database
      _carsRepository.AddRange(actualCars);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act 
      var actual = await _carsRepository.SelectAllAsync();
      var comparison = new ComparisonBuilder()
         //       .IgnoreCircularReferences()
         .IgnoreProperty<Car>(c=> c.Person)
         .Create();
      Assert.True(actualCars.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public async Task AddUt() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      
      // Act
      // retrieve person from database which is tracked
      var actualPerson =  await _peopleRepository.FindByIdAsync(person.Id);
      Assert.NotNull(actualPerson);
      // domain model
      var car = _seed.Car1;
      actualPerson?.AddCar(car); // car is marked as added, Person remains unchanged from the database perspective 
      // add car to database
      _carsRepository.Add(car);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      
      // Assert
      var actual = await _carsRepository.FindByIdAsync(car.Id);
      var comparison = new ComparisonBuilder()
         //       .IgnoreCircularReferences()
         .IgnoreProperty<Car>(c=> c.Person)
         .Create();
      Assert.True(car.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public async Task AddRangeUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Act
      var actualPeople = await _peopleRepository.SelectAllAsync();
      Assert.NotNull(actualPeople);
      var (_, actualCars) = Seed.InitPeopleWithCars(actualPeople,_seed.Cars);
      Assert.NotNull(actualCars);
      _carsRepository.AddRange(actualCars);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // Assert
      var actual = await _carsRepository.SelectAllAsync();
      var comparison = new ComparisonBuilder()
         //       .IgnoreCircularReferences()
         .IgnoreProperty<Car>(c=> c.Person)
         .Create();
      Assert.True(actualCars.IsDeepEqual(actual, comparison));
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
      var car = _seed.Car1;
      actualPerson.AddCar(car);
      // add car to database
      _carsRepository.Add(car);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      
      // Assert
      var actual = await _carsRepository.FindByIdAsync(_seed.Car1.Id);
      var comparison = new ComparisonBuilder()
         .IgnoreProperty<Car>(c => c.Person)
         .Create();
      Assert.True(car.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public async Task RemoveUt() {
      // Arrange
      var person = _seed.Person1;
      var car1 = _seed.Car1;
      var car2 = _seed.Car2;
      person.AddCar(car1);
      person.AddCar(car2);
      
      // add person and cars to database
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      
      // Act
      _carsRepository.Remove(car1);
      await _dataContext.SaveAllChangesAsync();
      
      // Assert
      var actual = await _carsRepository.FindByIdAsync(car1.Id);
      Assert.Null(actual);
   }
   
   [Fact]
   public async Task SelectCarsByPersonIdUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      // retriv person from database to track it
      var actualPerson = await _peopleRepository.FindByIdAsync(_seed.Person1.Id);
      Assert.NotNull(actualPerson);
      // domain model
      actualPerson.AddCar(_seed.Car1);
      actualPerson.AddCar(_seed.Car2);
      // add cars cars to repository and save all cars to database
      _carsRepository.Add(_seed.Car1);
      _carsRepository.Add(_seed.Car2);
      await _dataContext.SaveAllChangesAsync();
      _dataContext.ClearChangeTracker();
      var expectedCars = new List<Car> {_seed.Car1, _seed.Car2};
      
      // Act
      var actual = await _carsRepository.SelectByPersonIdAsync(_seed.Person1.Id, CancellationToken.None);
      
      // Assert
      var comparison = new ComparisonBuilder()
         //       .IgnoreCircularReferences()
         .IgnoreProperty<Car>(c=> c.Person)
         .Create();
      Assert.True(expectedCars.IsDeepEqual(actual, comparison));
   }
}
