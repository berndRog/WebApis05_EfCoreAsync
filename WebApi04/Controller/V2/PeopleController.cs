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
public class PeopleController(
   IPeopleRepository peopleRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   // get all people http://localhost:5200/carshop/people
   [HttpGet("people")]  
   [EndpointSummary("Get all people")] 
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<PersonDto>>> GetAllAsync(
      CancellationToken ctToken = default // default = CancellationToken.None
   ) {
      return await peopleRepository.SelectAllAsync(ctToken) switch {   
         IEnumerable<Person> people => Ok(people.Select(p => p.ToPersonDto())),
         null => NotFound("People not found")
      };
   }
   
   // get by id http://localhost:5200/carshop/v2/people/{id}
   [HttpGet("people/{id:guid}")]
   [EndpointSummary("Get person by id")]
   [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<PersonDto?>> GetByIdAsync(
      [Description("Unique id of the person to be found")]
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      return await peopleRepository.FindByIdAsync(id, ctToken) switch { 
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given id not found")
      };
   }
   
   // create a new person http://localhost:5200/carshop/v2/people
   [HttpPost("people")]
   [EndpointSummary("Create a new person")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<PersonDto?>> CreateAsync(
      [Description("PersonDto with the new person's data")]
      [FromBody] PersonDto personDto,
      CancellationToken ctToken = default
   ) {
      if(await peopleRepository.FindByIdAsync(personDto.Id,ctToken) != null) 
         return BadRequest("Person with given id already exists"); 
      
      // map dto to entity
      var person = personDto.ToPerson();
      
      // add person to repository and save changes
      peopleRepository.Add(person);
      await dataContext.SaveAllChangesAsync("Create Person", ctToken);
      
      // return an absolute URL as location 
      var url = "";
      if (Request != null) 
         url = Request?.Scheme + "://" + Request?.Host
            + Request?.Path.ToString() +$"/{person.Id}";
      else 
         url = $"http://localhost:5200/carshop/v2/people/{person.Id}";
      var uri = new Uri(url, UriKind.Absolute);
      return Created(uri, person.ToPersonDto()); 
   }
 
   // update a person http://localhost:5200/carshop/v2/people/{id}
   [HttpPut("people/{id}")]
   [EndpointSummary("Update a person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<PersonDto?>> UpdateAsync(
      [Description("Unique id of the existing person")]
      [FromRoute] Guid id,
      [Description("PersonDto with the updated person's data")]
      [FromBody] PersonDto updPersonDto,
      CancellationToken ctToken = default
   ) {
      // check if the id in the route and in the body match
      if (id != updPersonDto.Id) 
         return BadRequest("Id in the route and in the body do not match");

      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(id, ctToken);
      if (person == null) return NotFound("Person with given id not found");
      
      // map dto to entity
      var updPerson = updPersonDto.ToPerson();
      // update person in the domain model
      person.Update(updPerson.FirstName, updPerson.LastName, 
         updPerson.Email, updPerson.Phone);
      
      // update person in the repository and save changes
      peopleRepository.Update(person);
      await dataContext.SaveAllChangesAsync("Update Person", ctToken);
      
      return Ok(person.ToPersonDto());
   }
   
   // delete a person http://localhost:5200/carshop/v2/people/{id}
   [HttpDelete("people/{id}")]
   [EndpointSummary("Delete a person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<IActionResult> DeleteAsync(
      [Description("Unique id of the existing person")]
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(id, ctToken);
      if (person == null) return NotFound();
     
      // remove person from the repository and save changes
      peopleRepository.Remove(person);
      await dataContext.SaveAllChangesAsync("Delete Person", ctToken);
      
      return NoContent();
   }
   
   // get person by lastname http://localhost:5200/carshop/v2/people/name?name={name}
   // using sql "like" operation, i.e. name must be a part of the lastname
   [HttpGet("people/name")]
   [EndpointSummary("Get person by name")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<PersonDto>>> GetByNameAsync(
      [Description("Name to be search for")]
      [FromQuery] string name,
      CancellationToken ctToken = default
   ) {
      return await peopleRepository.SelectByNameAsync(name, ctToken) switch {
         IEnumerable<Person> people => Ok(people.Select(p => p.ToPersonDto())),
         null => NotFound("Person with given name not found")
      };
   }

}