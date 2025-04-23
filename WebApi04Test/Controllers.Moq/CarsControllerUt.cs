using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Mapping;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq;

[Collection(nameof(SystemTestCollectionDefinition))]
public class CarsControllerUt : BaseControllerUt {
   
   [Fact]
   public async Task GetAllAsyncUt_Ok() {
      // Arrange People with Cars
      var (_, expectedCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      
      _mockCarsRepository.Setup(r => r.SelectAllAsync(CancellationToken.None))
         .ReturnsAsync(expectedCars);

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
      
      _mockCarsRepository.Setup(r => r.SelectByPersonIdAsync(_seed.Person1.Id, CancellationToken.None))
         .ReturnsAsync(expectedCars);

      // Act
      var actionResult = await _carsController.GetByPersonIdAsync(_seed.Person1.Id, CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public async Task GetByIdAsyncUt_Ok() {
      // Arrange
      var id = _seed.Car1.Id;
      var expected = _seed.Car1;
      // mock the result of the repository
      _mockCarsRepository.Setup(r => r.FindByIdAsync(id, CancellationToken.None))
         .ReturnsAsync(expected);
      
      // Act
      var actionResult = await _carsController.GetByIdAsync(id, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult, expected.ToCarDto());
   }

   [Fact]
   public async Task GetByIdAsyncUt_NotFound() {
      // Arrange
      var id = Guid.NewGuid();
      _mockCarsRepository.Setup(r => r.FindByIdAsync(id, CancellationToken.None))
         .ReturnsAsync(null as Car);

      // Act
      var actionResult =  await _carsController.GetByIdAsync(id, CancellationToken.None);
      
      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   
   [Fact]
   public async Task CreateAsyncUt_Created() {
      // Arrange
      var car = _seed.Car1;
      
      // mock the repository's FindById method to return null
      _mockCarsRepository.Setup(r => r.FindByIdAsync(car.Id, CancellationToken.None))
         .ReturnsAsync(null as Car);
      
      // Act
      var carDto = car.ToCarDto();
      var actionResult = await _carsController.CreateAsync(_seed.Person1.Id, carDto, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public async Task CreateAsyncUt_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      // mock the peopleRepository's FindById method 
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      // mock the repository's FindById method to return null
      _mockCarsRepository.Setup(r => r.FindByIdAsync(car.Id, CancellationToken.None))
         .ReturnsAsync(car);

      // Act
      var actionResult =  await _carsController.CreateAsync(person.Id, car.ToCarDto(), CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task UpdateAsyncUt_Ok() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var updCar = new Car(car.Id, car.Maker, car.Model, car.Year, 9999m, null, car.PersonId);
      
      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      // mock the carsRepository's FindById method to return an existing car
      _mockCarsRepository.Setup(r => r.FindByIdAsync(car.Id, CancellationToken.None))
         .ReturnsAsync(car);
      // mock the repository's Update method
      _mockCarsRepository.Setup(r => r.Update(updCar))
         .Verifiable();
      // mock the data context's SaveAllChangesAsync method
      _mockDataContext.Setup(c => c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None))
         .ReturnsAsync(true);

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = 
         await _carsController.UpdateAsync(person.Id, car.Id, updCarDto, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult, updCarDto);
      // Verify that the repository's Update method was called once
      _mockCarsRepository.Verify(r => r.Update(It.IsAny<Car>()), Times.Once);
      // Verify that the data context's SaveAllChangesAsync method was called once
      _mockDataContext.Verify(c => 
         c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
   }

   [Fact]
   public async Task UpdateAsyncUt_PersonIdNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      var routeId = Guid.NewGuid();

      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(null as Person);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = 
         await _carsController.UpdateAsync(person.Id, car.Id, updCarDto, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public async Task UpdateAsyncUt_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      
      var badId = Guid.NewGuid();

      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = 
         await _carsController.UpdateAsync(person.Id, badId, updCarDto, CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
  
   [Fact]
   public async Task UpdateAsyncUt_CarNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      
      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      // Setup the repository to return null for the specified id
      _mockCarsRepository.Setup(r => r.FindByIdAsync(car.Id, CancellationToken.None))
         .ReturnsAsync(null as Car);

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = 
         await _carsController.UpdateAsync(person.Id, car.Id, updCarDto, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task DeleteAsyncUt_NoContent() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
 
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      _mockCarsRepository.Setup(r => r.FindByIdAsync(car.Id, CancellationToken.None))
         .ReturnsAsync(car);
      _mockCarsRepository.Setup(r => r.Remove(car))
         .Verifiable();
      _mockDataContext.Setup(c => c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None))
         .ReturnsAsync(true);

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
}