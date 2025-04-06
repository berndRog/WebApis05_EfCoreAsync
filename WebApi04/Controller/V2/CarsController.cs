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
   ICarsRepository carRepository,
   IDataContext dataContext
) : ControllerBase {

   // get all cars http://localhost:5200/carshop/v2/cars
   [HttpGet("cars")]
   [EndpointSummary("Get all cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<CarDto>?>> GetCarsAsync() {
      // get all cars 
      var cars = await carRepository.SelectAllAsync();
      return Ok(cars?.Select(c => c.ToCarDto()));
   }
   
   // get car by id http://localhost:5200/carshop/v2/cars/{id}
   [HttpGet("cars/{id:guid}")]
   [EndpointSummary("Get car by id")]
   public async Task<ActionResult<CarDto?>> GetById(
      [Description("Unique id of the car to be search for")] 
      [FromRoute] Guid id
   ) {
      return await carRepository.FindByIdAsync(id) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }
   
   // create a new car for a given person http://localhost:5200/carshop/people/{personId}/cars
   [HttpPost("people/{personId:guid}/cars")]
   [EndpointSummary("Create a new car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<CarDto?>> CreateAsync(
      [Description("Unique id of the given person")] 
      [FromRoute] Guid personId,
      [Description("CarDto of the new car's data")]
      [FromBody]  CarDto carDto
   ) {
      if(await carRepository.FindByIdAsync(carDto.Id) != null)
         return BadRequest("Car with given Id already exists");
      
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(personId);
      if (person == null)
         return BadRequest("personId doesn't exist");
      
      // map Dto to entity
      var car = carDto.ToCar();
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save changes
      await carRepository.AddAsync(car); 
      await dataContext.SaveAllChangesAsync("Create Car");
      
      // return created car as Dto
      var requestPath = Request?.Path ?? $"http://localhost:5200/carshop/v2/cars/{car.Id}";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Relative);
      return Created(uri, car.ToCarDto()); 
   }

   // update a car for a given person http://localhost:5200/carshop/v2/people/{personId}/cars/{id}
   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   [EndpointSummary("Update a car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CarDto?>> Update(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the car to be updated")] 
      [FromRoute] Guid id,
      [Description("CarDto of the updated car's data")]
      [FromBody]  CarDto updCarDto
   ) {

      // check if Id in the route and body match
      if(personId != updCarDto.Id) 
         return BadRequest("Id in the route and body do not match");
      // check if person with given Id exists
      var car = await carRepository.FindByIdAsync(id);
      if (car == null) return NotFound("Car with given id not found");

      // map dto to entity
      var updCar = updCarDto.ToCar();
      // update car in the domain model
      car.Update(updCar.Maker, updCar.Model,
         updCar.Year, updCar.Price);
      
      // save to repository and write changes 
      carRepository.Update(car);
      await dataContext.SaveAllChangesAsync("Update Car");
      
      return Ok(car.ToCarDto());
   }
   
   // delete a car for a given person http://localhost:5200/carshop/people/{personId}/cars/{id}
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [EndpointSummary("Delete a car for a given person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ProblemDetails>> Delete(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the given car")] 
      [FromRoute] Guid id
   ) {
      // find person in the repository
      var person = await peopleRepository.FindByIdAsync(personId);
      if(person == null) 
         return NotFound("Person given id not found.");
      // find car in the repository
      var car = await carRepository.FindByIdAsync(id); 
      if(car == null) 
         return NotFound("Car not found.");
      
      // remove car from person in the doimain model
      person.RemoveCar(car);
      
      // save to repository and write changes 
      carRepository.Remove(car);
      await dataContext.SaveAllChangesAsync("Delete Car");

      // return no content
      return NoContent(); 
   }
   
   // filter cars by attributes http://localhost:5200/carshop/v2/cars/attributes
   // filter criteria are passed in the header
   [HttpGet("cars/attributes")]
   [EndpointSummary("Get cars by attributes")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<CarDto>?>> GetCarsByAttrubutesAsync(
      [Description("maker of the car to be search for (can be null)")]
      [FromHeader] string? maker,
      [Description("model of the car to be search for (can be null)")]
      [FromHeader] string? model,
      [Description("year >= yearMin of the car to be search for (can be null)")]
      [FromHeader] int? yearMin,
      [Description("year <= yearMax of the car to be search for (can be null)")]
      [FromHeader] int? yearMax,
      [Description("price >= priceMin of the car to be search for (can be null)")]
      [FromHeader] decimal? priceMin,
      [Description("price <= priceMax of the car to be search for (can be null)")]
      [FromHeader] decimal? priceMax
   ) {
      // get all cars by attributes
      var cars = await carRepository.SelectByAttributesAsync(maker, model, 
         yearMin, yearMax, priceMin, priceMax);
      return Ok(cars?.Select(c => c.ToCarDto()));
   }
  
   // get all cars of a given person http://localhost:5200/carshop/v2/people/{personId}/cars
   [HttpGet("people/{personId:guid}/cars")]
   [EndpointSummary("Get all cars of a given person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<CarDto>?>> GetCarsByPersonAsync(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = await carRepository.SelectByPersonIdAsync(personId);
      return Ok(cars?.Select(c => c.ToCarDto()));
   }
   
}
