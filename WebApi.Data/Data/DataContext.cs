using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("WebApiTest")]
namespace WebApi.Data; 

internal class DataContext(
   DbContextOptions<DataContext> options
) : DbContext(options), IDataContext {
   
   #region fields
   private ILogger<DataContext>? _logger;
   #endregion
   
   #region properties
   // Note that DbContext caches the instance of DbSet returned from the
   // Set method so that each of these properties will return the same
   // instance every time it is called.
   public DbSet<Person> People => Set<Person>(); // call to a method, not a field 
   public DbSet<Car> Cars => Set<Car>(); 
   #endregion
   
   #region methods
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      
      var loggerFactory = LoggerFactory.Create(builder => {
         builder.AddDebug();
      });
      _logger = loggerFactory.CreateLogger<DataContext>();
      
      // Configure logging
      optionsBuilder
         .UseLoggerFactory(loggerFactory)
         .EnableSensitiveDataLogging(true)
         .EnableDetailedErrors();
   }
   
   public async Task<bool> SaveAllChangesAsync() {
      
      // log repositories before transfer to the database
      var view = ChangeTracker.DebugView.LongView;
      Console.WriteLine(view);
      _logger?.LogDebug("\n{1}",view);
      
      // save all changes to the database, returns the number of rows affected
      var result = await SaveChangesAsync();
      
      // log repositories after transfer to the database
      _logger?.LogDebug("SaveChanges {1}",result);
      _logger?.LogDebug("\n{1}",ChangeTracker.DebugView.LongView);
      
      return result>0;
   }
   
   public void ClearChangeTracker() =>
      ChangeTracker.Clear();

   public void LogChangeTracker(string text) =>
      _logger?.LogDebug("{1}\n{2}", text, ChangeTracker.DebugView.LongView);
   #endregion
   
   #region static methods
// "UseDatabase": "Sqlite",
// "ConnectionStrings": {
//    "LocalDb": "WebApi04",
//    "SqlServer": "Server=localhost,2433; Database=WebApi04; User=sa; Password=P@ssword_geh1m;",
//    "Sqlite": "WebApi04"
// },
   public static (string useDatabase, string dataSource) EvalDatabaseConfiguration(
      IConfiguration configuration
   ) {

      // read active database configuration from appsettings.json
      var useDatabase = configuration.GetSection("UseDatabase").Value ??
         throw new Exception("UseDatabase is not available");

      // read connection string from appsettings.json
      var connectionString = configuration.GetSection("ConnectionStrings")[useDatabase]
         ?? throw new Exception("ConnectionStrings is not available"); 
      

      // Create the directory if it does not exist /Users/rogallab/Webtech/WebApps/WebApp02
      var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      var directory = Path.Combine(homeDirectory, "Webtech/WebApis/WebApi05");
      if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
      
      switch (useDatabase) {
         case "LocalDb":
            var dbFile = $"{Path.Combine(directory, connectionString)}.mdf";
            var dataSourceLocalDb =
               $"Data Source = (LocalDB)\\MSSQLLocalDB; " +
               $"Initial Catalog = {connectionString}; Integrated Security = True; " +
               $"AttachDbFileName = {dbFile};";
            Console.WriteLine($"....: EvalDatabaseConfiguration: LocalDb {dataSourceLocalDb}");
            return (useDatabase, dataSourceLocalDb);

         case "SqlServer":
            return (useDatabase, connectionString);

         case "Sqlite":
            var dataSourceSqlite =
               "Data Source=" + Path.Combine(directory, connectionString) + ".db";
            Console.WriteLine($"....: EvalDatabaseConfiguration: Sqlite {dataSourceSqlite}");
            return (useDatabase, dataSourceSqlite);
         default:
            throw new Exception("appsettings.json Problems with database configuration");
      }   }
   #endregion
   
}
