using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Data;
using WebApi.Data.Repositories;
namespace WebApi.Di;

public static class DiData {
   
   public static IServiceCollection AddData(
      this IServiceCollection services,
      IConfiguration configuration
   ){
      services.AddScoped<IPeopleRepository, PeopleRepository>();
      services.AddScoped<ICarsRepository, CarsRepository>();
      
      // Add DbContext (Database) to DI-Container
      var (useDatabase, dataSource) = DataContext.EvalDatabaseConfiguration(configuration);
      
      switch (useDatabase) {
         case "LocalDb":
         case "SqlServer":
            // services.AddDbContext<IDataContext, DataContext>(options => 
            //    options.UseSqlServer(dataSource)
            // );
            break;
         case "Sqlite": 
         case "SqliteInMemory":
            services.AddDbContext<IDataContext, DataContext>(options => 
               options.UseSqlite(dataSource)
            );
            break;
         default:
            throw new Exception("appsettings.json UseDatabase not available");
      }
      return services;
   }
}