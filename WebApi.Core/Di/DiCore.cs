using Microsoft.Extensions.DependencyInjection;
namespace WebApi.Di; 

public static class DiCore {
   public static IServiceCollection AddCore(
      this IServiceCollection services
   ) {
      return services;
   }
}