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
public class CarsController(
   IPeopleRepository peopleRepository,
   ICarsRepository carsRepository,
   IDataContext dataContext
) : ControllerBase {
   
   // get all cars http://localhost:5200/carshop/v2/cars
   [HttpGet("cars")]
   [EndpointSummary("Get all cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<CarDto>>> GetAllAsync(
      CancellationToken ctToken = default // default = CancellationToken.None
   ) {
      // get all cars 
      var cars = await carsRepository.SelectAllAsync(ctToken);
      return Ok(cars?.Select(c => c.ToCarDto()));
   }

   // get car by id http://localhost:5200/carshop/v2/cars/{id}
   [HttpGet("cars/{id:guid}")]
   [EndpointSummary("Get car by id")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CarDto?>> GetByIdAsync(
      [Description("Unique id of the car to be search for")] [FromRoute]
      Guid id,
      CancellationToken ctToken = default
   ) {
      return await carsRepository.FindByIdAsync(id, ctToken) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }

   // create a new car for a given person http://localhost:5200/carshop/people/{personId}/cars
   [HttpPost("people/{personId:guid}/cars")]
   [EndpointSummary("Create a new car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CarDto?>> CreateAsync(
      [Description("Unique id of the given person")] [FromRoute]
      Guid personId,
      [Description("CarDto of the new car's data")] [FromBody]
      CarDto carDto,
      CancellationToken ctToken = default
   ) {
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(personId, ctToken);
      if (person == null)
         return NotFound("personId doesn't exist");

      // check if car with given Id already exists
      if (await carsRepository.FindByIdAsync(carDto.Id, ctToken) != null)
         return BadRequest("Car with given Id already exists");

      // map Dto to entity
      var car = carDto.ToCar();
      // add car to person in the domain model
      person.AddCar(car);

      // add car to repository and save changes
      carsRepository.Add(car);
      await dataContext.SaveAllChangesAsync("Create Car",ctToken);

      // return an absolute URL as location 
      var url = "";
      if (Request != null) 
         url = Request?.Scheme + "://" + Request?.Host
            + Request?.Path.ToString() +$"/{car.Id}";
      else 
         url = $"http://localhost:5200/carshop/v2/cars/{car.Id}";
      var uri = new Uri(url, UriKind.Absolute);
      return Created(uri, car.ToCarDto()); 
   }

   // update a car for a given person http://localhost:5200/carshop/v2/people/{personId}/cars/{id}
   [HttpPut("people/{personId:guid}/cars/{id:guid}")]
   [EndpointSummary("Update a car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CarDto?>> UpdateAsync(
      [Description("Unique id for the given person")] [FromRoute]
      Guid personId,
      [Description("Unique id for the car to be updated")] [FromRoute]
      Guid id,
      [Description("CarDto of the updated car's data")] [FromBody]
      CarDto updCarDto,
      CancellationToken ctToken = default
   ) {
      
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(personId, ctToken);
      if (person == null)
         return NotFound("Person with given personId not found");
      
      // check if Id in the route and body match
      if (id != updCarDto.Id)
         return BadRequest("Id in the route and body do not match");
      
      // find car in the repository
      var car = await carsRepository.FindByIdAsync(id, ctToken);
      if (car == null) 
         return NotFound("Car with given id not found");

      // map dto to entity
      var updCar = updCarDto.ToCar();
      // update car in the domain model
      car.Update(updCar.Maker, updCar.Model, updCar.Year, updCar.Price);

      // save to repository and write changes 
      carsRepository.Update(car);
      await dataContext.SaveAllChangesAsync("Update Car", ctToken);

      return Ok(car.ToCarDto());
   }

   // delete a car for a given person http://localhost:5200/carshop/people/{personId}/cars/{id}
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [EndpointSummary("Delete a car for a given person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<IActionResult> DeleteAsync(
      [Description("Unique id for the given person")] [FromRoute]
      Guid personId,
      [Description("Unique id for the given car")] [FromRoute]
      Guid id,
      CancellationToken ctToken = default
   ) {
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(personId, ctToken);
      if (person == null)
         return NotFound("Person given id not found.");
      
      // find car in the repository
      var car = await carsRepository.FindByIdAsync(id, ctToken);
      if (car == null)
         return NotFound("Car not found.");

      // remove car from person in the domain model
      person.RemoveCar(car);

      // save to repository and write changes 
      carsRepository.Remove(car);
      await dataContext.SaveAllChangesAsync("Delete Car", ctToken);

      // return no content
      return NoContent();
   }

   // filter cars by attributes http://localhost:5200/carshop/v2/cars/attributes
   // filter criteria are passed in the header
   [HttpGet("cars/attributes")]
   [EndpointSummary("Get cars by attributes")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<CarDto>>> GetByAttributesAsync(
      [Description("maker of the car to be search for (can be null)")] [FromHeader]
      string? maker,
      [Description("model of the car to be search for (can be null)")] [FromHeader]
      string? model,
      [Description("year >= yearMin of the car to be search for (can be null)")] [FromHeader]
      int? yearMin,
      [Description("year <= yearMax of the car to be search for (can be null)")] [FromHeader]
      int? yearMax,
      [Description("price >= priceMin of the car to be search for (can be null)")] [FromHeader]
      decimal? priceMin,
      [Description("price <= priceMax of the car to be search for (can be null)")] [FromHeader]
      decimal? priceMax,
      CancellationToken ctToken = default
   ) {
      // get all cars by attributes
      var cars = await carsRepository.SelectByAttributesAsync(maker, model,
         yearMin, yearMax, priceMin, priceMax, ctToken);
      return Ok(cars?.Select(c => c.ToCarDto()) ?? []);
   }

   // get all cars of a given person http://localhost:5200/carshop/v2/people/{personId}/cars
   [HttpGet("people/{personId:guid}/cars")]
   [EndpointSummary("Get all cars of a given person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<CarDto>>> GetByPersonIdAsync(
      [Description("Unique id for the given person")] [FromRoute]
      Guid personId,
      CancellationToken ctToken = default
   ) {
      // get all cars of a given person
      var cars = await carsRepository.SelectByPersonIdAsync(personId, ctToken);
      return Ok(cars?.Select(c => c.ToCarDto()) ?? []);
   }
}