using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
namespace WebApi.Controllers.V2;

[ApiVersion("2.0")]
[Route("carshop/v{version:apiVersion}")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default
public class PersonController(
   ControllerHelper helper,
   IPersonRepository personRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controller
   
   [HttpGet("people")]  
   [EndpointSummary("Get all people")] 
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<PersonDto>>> GetAllAsync() {
      var people = await personRepository.SelectAllAsync();
      return Ok(people.Select(p => p.ToPersonDto()));
   }
   
   [HttpGet("people/{id:guid}")]
   [EndpointSummary("Get person by id")]
   [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<PersonDto>> GetByIdAsync(
      [Description("Unique id of the person to be found")]
      [FromRoute] Guid id
   ) {
      return await personRepository.FindByIdAsync(id) switch { 
    //return await personRepository.FindByIdWithCarsAsync(id) switch {   
         Person person => Ok(person.ToPersonDto()),
         null => helper.DetailsNotFound<PersonDto>("Person with given id not found")
      };
   }
   
   [HttpGet("people/name")]
   [EndpointSummary("Get person by name")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<PersonDto>> GetByNameAsync(
      [Description("Name to be search for")]
      [FromQuery] string name
   ) {
      return await personRepository.FindByNameAsync(name) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given name not found")
      };
   }
   
   [HttpPost("people")]
   [EndpointSummary("Create a new person")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   public async Task<ActionResult<PersonDto>> CreateAsync(
      [Description("PersonDto with the new person's data")]
      [FromBody] PersonDto personDto
   ) {
      if(await personRepository.FindByIdAsync(personDto.Id) != null) 
         helper.DetailsBadRequest<PersonDto>("Person with given id already exists"); 
      
      // map dto to entity
      var person = personDto.ToPerson();
      
      // add person to repository and save changes
      await personRepository.AddAsync(person);
      await dataContext.SaveAllChangesAsync("Create Person");
      
      return Created($"/people/{person.Id}", person.ToPersonDto());
   }
 
   /// <summary>
   /// Update a person
   /// </summary>
   /// <param name="id">Unique id of the person to be updated</param>
   /// <param name="updPersonDto">PersonDto with the updated person's data</param>
   [HttpPut("people/{id}")]
   [EndpointSummary("Update a person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public async Task<ActionResult<PersonDto>> UpdateAsync(
      [Description("Unique id of the existing person")]
      [FromRoute] Guid id,
      [Description("PersonDto with the updated person's data")]
      [FromBody] PersonDto updPersonDto
   ) {
      // find person in the repository
      var person = await personRepository.FindByIdAsync(id);
      if (person == null) return NotFound("Person with given id not found");
      
      // map dto to entity
      var updPerson = updPersonDto.ToPerson();
      // update person in the domain model
      person.Update(updPerson);
      
      // update person in the repository and save changes
      await personRepository.UpdateAsync(person);
      await dataContext.SaveAllChangesAsync("Update Person");
      
      return Ok(person.ToPersonDto());
   }

   
   /// <summary>
   /// Delete a given person
   /// </summary>
   /// <param name="id">Unique id of the person to delete</param>
   [HttpDelete("people/{id}")]
   [EndpointSummary("Delete a person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> Delete(
      [Description("Unique id of the existing person")]
      [FromRoute] Guid id
   ) {
      // find person in the repository
      var person = await personRepository.FindByIdAsync(id);
      if (person == null) return NotFound();
     
      // remove person from the repository and save changes
      await personRepository.RemoveAsync(person);
      await dataContext.SaveAllChangesAsync("Delete Person");
      
      return NoContent();
   }
}