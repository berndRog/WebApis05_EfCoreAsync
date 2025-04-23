using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
using WebApiTest.Controllers.Moq;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Controllers;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleControllerUt : BaseController {
   
   [Fact]
   public async Task GetAllAsyncUt_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync("Add people",CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var expectedDtos = _seed.People.Select(p => p.ToPersonDto()); 
      
      // Act
      var actionResult = await _peopleController.GetAllAsync(CancellationToken.None);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedDtos);
   }
   
   [Fact]
   public async Task GetByIdAsyncUt_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync("Add people", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var person = _seed.Person1;

      // Act
      var actionResult = await _peopleController.GetByIdAsync(person.Id, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult!, person.ToPersonDto());
   }
   
   [Fact]
   public async Task GetByIdAsyncUt_NotFound() {
      // Arrange
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync("Add people");
      _dataContext.ClearChangeTracker();
      
      // Act
      var actionResult =  await _peopleController.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task GetByNameAsyncUt_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync("Add people", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var name = "Muster";
      var expectedPeople = _seed.People
         .Where(p => p.LastName.Contains(name))
         .Select(p => p.ToPersonDto())
         .ToList();

      // Act
      var actionResult = await _peopleController.GetByNameAsync(name, CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedPeople);
   }

   [Fact]
   public async Task GetByNameAsyncUt_NotFound() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      await _dataContext.SaveAllChangesAsync("Add people", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var name = "NonExistentName";
      
      // Act
      var actionResult = await _peopleController.GetByNameAsync(name,CancellationToken.None);

      // Assert emptyList as result
      THelper.IsEnumerableOk(actionResult, new List<PersonDto>());
   }

   [Fact]
   public async Task CreateAsyncUt_Created() {
      // Arrange
      var person = _seed.Person1;
      
      // Act
      var personDto = person.ToPersonDto();
      var actionResult = await _peopleController.CreateAsync(personDto, CancellationToken.None);

      // Assert
      THelper.IsCreated(actionResult, personDto);
   }

   [Fact]
   public async Task CreateAsyncUt_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      
      // Act
      var personDto = person.ToPersonDto();
      var actionResult =  await _peopleController.CreateAsync(personDto);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);

   }
   
   [Fact]
   public async Task UpdateAsyncUt_Ok() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var updPerson = new Person(person.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");
      
      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = 
         await _peopleController.UpdateAsync(person.Id, updPersonDto, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult!, updPersonDto);
   }

   [Fact]
   public async Task UpdateAsyncUt_BadRequest() {
      // Arrange
      var routeId = Guid.NewGuid();
      // updPerson has an id different from routeId
      var updPerson = new Person(_seed.Person1.Id, "Erna", "Meier", "0511/6543-2109", "e.meier@icloud.com");

      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = await _peopleController.UpdateAsync(routeId, updPersonDto, CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }

   [Fact]
   public async Task UpdateUt_NotFound() {
      // Arrange
      var person = _seed.Person2;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var updPerson = new Person(_seed.Person1.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");
      
      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = 
         await _peopleController.UpdateAsync(updPerson.Id, updPersonDto, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task DeleteUt_NoContent() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();

      // Act
      var actionResult = await _peopleController.DeleteAsync(person.Id, CancellationToken.None);

      // Assert
      Assert.IsType<NoContentResult>(actionResult);
   }
   
   [Fact]
   public async Task DeleteUt_NotFound() {
      // Arrange
      var person = _seed.Person2;
      _peopleRepository.Add(person);
      await _dataContext.SaveAllChangesAsync("Add person", CancellationToken.None);
      _dataContext.ClearChangeTracker();
      var nonExistentPersonId = Guid.NewGuid();
      
      // Act
      var actionResult = 
         await _peopleController.DeleteAsync(nonExistentPersonId, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundResult>(actionResult);
   }
}