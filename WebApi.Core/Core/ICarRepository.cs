using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface ICarRepository {

      
   Task<Car?> FindByIdAsync(Guid id);
   Task<IEnumerable<Car>> SelectAllAsync();
   
   Task<IEnumerable<Car>> SelectByAttributesAsync(
      string? maker, string? model, int? yearMin, int? yearMax, 
      double? priceMin, double? priceMax);
   Task<IEnumerable<Car>> SelectByPersonIdAsync(Guid personId);

   Task AddAsync(Car car);
   Task UpdateAsync(Car car);
   Task RemoveAsync(Car car);
}