using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

internal class CarRepository(
   DataContext dataContext
) : ICarRepository {
   
   private readonly IDataContext _dataContext = dataContext;
   private readonly DbSet<Person> _dbSetPeople = dataContext.People; // => Set<Person>
   private readonly DbSet<Car> _dbSetCars = dataContext.Cars; // => Set<Car>

   public async Task<IEnumerable<Car>> SelectAllAsync() =>
      await _dbSetCars.ToListAsync();
   
   public async Task<Car?> FindByIdAsync(Guid id) =>
      await _dbSetCars.FirstOrDefaultAsync(car => car.Id == id);

   public async Task AddAsync(Car car) =>
      await _dbSetCars.AddAsync(car);
   
   public async Task UpdateAsync(Car updCar) {
      var car = await _dbSetCars.FirstOrDefaultAsync(car => car.Id == updCar.Id);
      if (car == null) throw new Exception("Car to be updated not found");
      car.Update(updCar);
   }

   public async Task RemoveAsync(Car car) {
      var cFound = await _dbSetCars.FirstOrDefaultAsync(c => c.Id == car.Id);
      if (cFound == null) throw new Exception("Car to be removed not found");
      _dbSetCars.Remove(cFound);
   }
   
   public async Task<IEnumerable<Car>> SelectByPersonIdAsync(Guid personId) =>
      _dbSetPeople
         .Where(person => person.Id == personId)
         .SelectMany(person =>  person.Cars)
         .ToList();
   
   public async Task<IEnumerable<Car>> SelectByAttributesAsync(
      string? maker = null, 
      string? model = null,
      int? yearMin = null,
      int? yearMax = null,
      double? priceMin = null,
      double? priceMax = null
   ) {
      var query = _dbSetCars.AsQueryable();
   
      if (!string.IsNullOrEmpty(maker))
         query = query.Where(car => car.Maker == maker);
      if (!string.IsNullOrEmpty(model))
         query = query.Where(car => car.Model == model);
      if (yearMin.HasValue)
         query = query.Where(car => car.Year >= yearMin.Value);
      if (yearMin.HasValue)
         query = query.Where(car => car.Year <= yearMax.Value);
      if (priceMin.HasValue)
         query = query.Where(car => car.Price >= priceMin.Value);
      if (priceMax.HasValue)
         query = query.Where(car => car.Price <= priceMax.Value);

      return await query.ToListAsync();
   }
}