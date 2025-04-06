using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface ICarsRepository: IBaseRepository<Car> {
   Task<IEnumerable<Car>?> SelectByAttributesAsync(
      string? maker, string? model, int? yearMin, int? yearMax, 
      decimal? priceMin, decimal? priceMax);
   
   Task<IEnumerable<Car>?> SelectByPersonIdAsync(Guid personId);
}