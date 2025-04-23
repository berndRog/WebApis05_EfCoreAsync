using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleControllerUt : BaseControllerUt {
   
   
   [Fact]
   public async Task GetAllAsyncUt_Ok() {
      // Arrange
      var expectedPeople = _seed.People;
      _mockPeopleRepository.Setup(r => r.SelectAllAsync(CancellationToken.None))
         .ReturnsAsync(expectedPeople);

      // Act
      var actionResult = await _peopleController.GetAllAsync();

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedPeople.Select(p => p.ToPersonDto()));
   }
   
   [Fact]
   public async Task GetByIdAsynUt_Ok() {
      // Arrange
      var id = _seed.Person1.Id;
      var expected = _seed.Person1;
      // mock the result of the repository
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(id, CancellationToken.None))
         .ReturnsAsync(expected);
      
      // Act
      var actionResult = await _peopleController.GetByIdAsync(id, CancellationToken.None);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsOk<PersonDto?>(actionResult, expected.ToPersonDto());
   }

   [Fact]
   public async Task GetByIdAsync_NotFound() {
      // Arrange
      var id = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(id, CancellationToken.None))
         .ReturnsAsync(null as Person);

      // Act
      var actionResult =  await _peopleController.GetByIdAsync(id, CancellationToken.None);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task GetByNameAsync_Ut_Ok() {
      // Arrange
      var name = "ValidName";
      var expectedPeople = new List<Person> { _seed.Person1 };
      _mockPeopleRepository.Setup(r => r.SelectByNameAsync(name,CancellationToken.None))
         .ReturnsAsync(expectedPeople);

      // Act
      var actionResult = await _peopleController.GetByNameAsync(name, CancellationToken.None);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedPeople.Select(p => p.ToPersonDto()));
   }
   
   
   [Fact]
   public async Task GetByNameAsyncUt_Ok_EmptyList() {
      // Arrange
      var expectedPeople = new List<Person>();
      var nonExistentName = "NonExistentName";
      _mockPeopleRepository.Setup(r => r.SelectByNameAsync(nonExistentName,CancellationToken.None))
         .ReturnsAsync(new Collection<Person>());

      // Act
      var actionResult = await _peopleController.GetByNameAsync(nonExistentName);

      // Assert
      Assert.IsType<OkObjectResult>(actionResult.Result);
      var actual =  (actionResult.Result as OkObjectResult)?.Value as IEnumerable<PersonDto>;
      Assert.NotNull(actual);
      Assert.Equal(expectedPeople?.Count, actual.Count());
   }

   [Fact]
   public async Task CreateAsyncUt_Created() {
      // Arrange
      var person = _seed.Person1;

      // mock the repository's FindById method to return null
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(null as Person);
      // mock the repository's Add method
      _mockPeopleRepository.Setup(r => r.Add(It.IsAny<Person>()))
         .Verifiable();
      // mock the data context's SaveAllChanges method
      _mockDataContext.Setup(c => c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None))
         .ReturnsAsync(true);

      // Act
      var personDto = person.ToPersonDto();
      var actionResult = await _peopleController.CreateAsync(personDto, CancellationToken.None);

      // Assert
      THelper.IsCreated(actionResult, personDto);
      // Verify that the repository's Add method was called once
      _mockPeopleRepository.Verify(r => r.Add(It.IsAny<Person>()), Times.Once);
      // Verify that the data context's SaveAllChanges method was called once
      _mockDataContext.Verify(c => c.SaveAllChangesAsync(It.IsAny<string>(),CancellationToken.None), Times.Once);
   }

   [Fact]
   public async Task CreateAsyncUt_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      // mock the repository's FindById method to return an existing owner
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);

      // Act
      var personDto = person.ToPersonDto();
      var actionResult =  await _peopleController.CreateAsync(personDto, CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task UpdateAsyncUt_Ok() {
      // Arrange
      var person = _seed.Person1;
      var updPerson = new Person(person.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");
      
      // mock the repository's FindByIdAsync method to return an existing owner
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      // mock the repository's Update method
      _mockPeopleRepository.Setup(r => r.Update(updPerson))
         .Verifiable();
      // mock the data context's SaveAllChangesAsync method
      _mockDataContext.Setup(c => c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None))
         .ReturnsAsync(true);

      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = 
         await _peopleController.UpdateAsync(person.Id, updPersonDto, CancellationToken.None);

      // Assert
      THelper.IsOk(actionResult, updPersonDto);
      // Verify that the repository's Update method was called once
      _mockPeopleRepository.Verify(r => r.Update(It.IsAny<Person>()), Times.Once);
      // Verify that the data context's SaveAllChangesAsync method was called once
      _mockDataContext.Verify(c => 
         c.SaveAllChangesAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
   }

   [Fact]
   public async Task UpdateAsyncUt_BadRequest() {
      // Arrange
      var routeId = Guid.NewGuid();
      // updPerson has an id different from routeId
      var updPerson = new Person(_seed.Person1.Id, "Erna", "Meier", "0511/6543-2109", "e.meier@icloud.com");

      // Act
      var actionResult = await _peopleController.UpdateAsync(
         routeId, updPerson.ToPersonDto(),CancellationToken.None);

      // Assert
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task Update_NotFound() {
      // Arrange
      var person = _seed.Person1;
      // Setup the repository to return null for the specified id
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(null as Person);

      // Act
      var actionResult = 
         await _peopleController.UpdateAsync(person.Id, person.ToPersonDto(), CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public async Task Delete_NoContent() {
      // Arrange
      var person = _seed.Person1;
      // Setup the repository to return null for the specified id
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(person.Id, CancellationToken.None))
         .ReturnsAsync(person);
      _mockPeopleRepository.Setup(r => r.Remove(person))
         .Verifiable();
      _mockDataContext.Setup(c => c.SaveAllChangesAsync(It.IsAny<string>(),CancellationToken.None))
         .ReturnsAsync(true);

      // Act
      var actionResult= await _peopleController.DeleteAsync(person.Id);

      // Assert
      Assert.IsType<NoContentResult>(actionResult);
   }
   
   [Fact]
   public async Task Delete_NotFound() {
      // Arrange
      var nonExistentPersonId = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindByIdAsync(nonExistentPersonId,CancellationToken.None))
         .ReturnsAsync(null as Person);

      // Act
      var actionResult = await _peopleController.DeleteAsync(nonExistentPersonId, CancellationToken.None);

      // Assert
      Assert.IsType<NotFoundResult>(actionResult);
   }

}