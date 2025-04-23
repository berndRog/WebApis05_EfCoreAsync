using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Data.Repositories_refactored;
namespace WebApi.Data.Repositories;

public class CarsRepository(
   DataContext dContext
) : ABaseRepository<Car>(dContext), ICarsRepository {
   
   public async Task<IEnumerable<Car>> SelectByPersonIdAsync(
      Guid personId,
      CancellationToken ctToken = default 
   ) {
      var cars = await _dbSet
         .Where(car => car.PersonId == personId)
         .ToListAsync(ctToken);
      _dataContext.LogChangeTracker("Car: SelectByPersonIdAsync ");
      return cars;
   }

   public async Task<IEnumerable<Car>> SelectByAttributesAsync(
      string? maker = null, 
      string? model = null,
      int? yearMin = null,
      int? yearMax = null,
      decimal? priceMin = null,
      decimal? priceMax = null,
      CancellationToken ctToken = default
   ) {
      var query = _dbSet.AsQueryable();
   
      if (!string.IsNullOrEmpty(maker))
         query = query.Where(car => car.Maker == maker);
      if (!string.IsNullOrEmpty(model))
         query = query.Where(car => car.Model == model);
      if (yearMin.HasValue)
         query = query.Where(car => car.Year >= yearMin.Value);
      if (yearMax.HasValue)
         query = query.Where(car => car.Year <= yearMax.Value);
      if (priceMin.HasValue)
         query = query.Where(car => car.Price >= priceMin.Value);
      if (priceMax.HasValue)
         query = query.Where(car => car.Price <= priceMax.Value);

      var cars = await query.ToListAsync(ctToken);
      _dataContext.LogChangeTracker("Car: SelectByAttributesAsync ");
      return cars;
   }
}