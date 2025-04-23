using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Mapping;
using WebApiTest.Controllers.Moq;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Controllers.V2;

[Collection(nameof(SystemTestCollectionDefinition))]
public class CarsControllerUt : BaseController {
   
   [Fact]
   public async Task GetAllAsyncUt_Ok() {
      // Arrange People with Cars
      var (_, expectedCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      _carsRepository.AddRange(expectedCars);
      await _dataContext.SaveAllChangesAsync("Add cars", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      // Act
      var actionResult = await _carsController.GetAllAsync(CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public async Task SelectByPersonIdAsyncUt_Ok() {
      // Arrange People with Cars
      var (_, updCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      var expectedCars = updCars.Where(c => c.PersonId == _seed.Person1.Id);
      _carsRepository.AddRange(updCars);
      await _dataContext.SaveAllChangesAsync("Add cars", CancellationToken.None);
      _dataContext.ClearChangeTracker();


      // Act
      var actionResult = await _carsController.GetByPersonIdAsync(_seed.Person1.Id, CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public async Task GetByIdAsyncUt_Ok() {
      // Arrange
      _seed.Person1.AddCar(_seed.Car1);
      _carsRepository.Add(_seed.Car1);
      await _dataContext.SaveAllChangesAsync("Add cars", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      var id = _seed.Car1.Id;
      var expected = _seed.Car1;
      
      // Act
      var actionResult = await _carsController.GetByIdAsync(id, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult, expected.ToCarDto());
   }

   [Fact]
   public async Task GetByIdAsyncUt_NotFound() {
      // Arrange
      _seed.Person1.AddCar(_seed.Car1);
      _carsRepository.Add(_seed.Car1);
      await _dataContext.SaveAllChangesAsync("Add cars", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      var id = Guid.NewGuid();
      
      // Act
      var actionResult =  await _carsController.GetByIdAsync(id, CancellationToken.None);
      
      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   
   [Fact]
   public async Task CreateAsyncUt_Created() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      // Domain model
      var car = _seed.Car1;
      person.AddCar(car);
      
      // Act
      var carDto = car.ToCarDto();
      
      
      var actionResult = 
         await _carsController.CreateAsync(person.Id, carDto, CancellationToken.None);

      // Assert
      THelper.IsCreated(actionResult, carDto);
   }

   [Fact]
   public async Task CreateAsyncUt_PersonNotFound() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      // Domain model
      var car = _seed.Car1;
      person.AddCar(car);
      
      // Act
      var carDto = car.ToCarDto();
      
      var actionResult = 
         await _carsController.CreateAsync(Guid.NewGuid(), carDto, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public async Task CreateAsyncUt_CarAlreadyExists() {
      // Arrange
      var person = _seed.Person1;
      // Domain model
      var car = _seed.Car1;
      person.AddCar(car);
      
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      // Act
      var carDto = car.ToCarDto();
      
      var actionResult = 
         await _carsController.CreateAsync(person.Id, carDto, CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task UpdateAsyncUt_Ok() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      // Domain model
      person.AddCar(car);
      // add person adn car to the repository
      _carsRepository.Add(car);
      await _dataContext.SaveAllChangesAsync("Add cars", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      var updCar = new Car(car.Id, car.Maker, car.Model, car.Year, 9999m, null, car.PersonId);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = 
         await _carsController.UpdateAsync(person.Id, car.Id, updCarDto, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult, updCarDto);
   }
   
   [Fact]
   public async Task UpdateAsyncUt_PersonIdNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      // Domain model
      person.AddCar(car);
      // add person adn car to the repository
      _carsRepository.Add(car);
      await _dataContext.SaveAllChangesAsync("Add cars");
      _dataContext.ClearChangeTracker();
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = await _carsController.UpdateAsync(Guid.NewGuid(), car.Id, updCarDto, CancellationToken.None);  

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

  
   
   /*
   [Fact]
   public async Task DeleteAsyncUt_NoContent() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
 

      // Act
      var actionResult = await _carsController.DeleteAsync(person.Id, car.Id);

      // Assert
      Assert.IsType<NoContentResult>(actionResult);
   }
   
   [Fact]
   public async Task DeleteAsyncUt_PersonNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var nonExistentPersonId = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(null as Person);
      
      // Act
      var actionResult = 
         await _carsController.DeleteAsync(nonExistentPersonId, car.Id, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult);
   }
   
   [Fact]
   public async Task GetCarsByAttributesAsyncUt_Ok() {
      // Arrange People with Cars
      var (_, updCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);

      var yearNow = DateOnly.FromDateTime(DateTime.Now).Year;
      var expectedCars = updCars.Where(car => 
         car.Maker == "BMW" && 
         car.Model == "X5" &&
         car.Year >= 2020 && car.Year <= yearNow && 
         car.Price >= 45_000 && car.Price <= 50_000
      ).ToList();
      
      _mockCarsRepository.Setup(r => 
            r.SelectByAttributesAsync("BMW", "X5", 2020, yearNow, 45_000, 50_000, CancellationToken.None))
         .ReturnsAsync(expectedCars);

      // Act
      var actionResult = await _carsController.GetByAttributesAsync(
         maker:"BMW",
         model:"X5",
         yearMin: 2020,
         yearMax: yearNow,
         priceMin: 45_000,
         priceMax: 50_000,
         ctToken: CancellationToken.None
      );
      
      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public async Task GetCarsByPersonIdUt_Ok() {
      // Arrange People with Cars
      var (_, updCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      var expectedCars = updCars.Where(c => c.PersonId == _seed.Person1.Id);
      
      _mockCarsRepository.Setup(r => r.SelectByPersonIdAsync(_seed.Person1.Id,CancellationToken.None))
         .ReturnsAsync(expectedCars);

      // Act
      var actionResult = 
         await _carsController.GetByPersonIdAsync(_seed.Person1.Id, CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   */
}