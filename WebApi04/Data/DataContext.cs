using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data; 

public class DataContext(
   DbContextOptions<DataContext> options
) : DbContext(options), IDataContext {
   
   #region fields
   private ILogger<DataContext>? _logger;
   #endregion
   
   #region properties
   // Note that DbContext caches the instance of DbSet returned from the
   // Set method so that each of these properties will return the same
   // instance every time it is called.
   public DbSet<Person> People => Set<Person>();
   public DbSet<Car> Cars => Set<Car>(); 
   #endregion
   
   #region methods
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      
      // https://learn.microsoft.com/de-de/ef/core/logging-events-diagnostics/simple-logging
      
      var loggerFactory = LoggerFactory.Create(builder => {
         builder
            .SetMinimumLevel(LogLevel.Information)
            .AddDebug()
            .AddConsole();
      });
      _logger = loggerFactory.CreateLogger<DataContext>();
      
      // Configure logging
      optionsBuilder
         .UseLoggerFactory(loggerFactory)
         .LogTo(Console.WriteLine, LogLevel.Information)
         .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
         .EnableSensitiveDataLogging(true)
         .EnableDetailedErrors();
   }
   
   public async Task<bool> SaveAllChangesAsync(string? text = null) {
      
      // log repositories before transfer to the database
      var view = ChangeTracker.DebugView.LongView;
      Console.WriteLine($"{text}\n{view}");
      Debug.WriteLine($"{text}\n{view}");
      _logger.LogInformation("\n{view}",view);
      
      // save all changes to the database, returns the number of rows affected
      var result = await SaveChangesAsync();
      
      // log repositories after transfer to the database
      _logger.LogInformation("SaveChanges {result}",result);

      _logger.LogInformation("\n{view}",ChangeTracker.DebugView.LongView);
      return result>0;
   }
   
   public void ClearChangeTracker() =>
      ChangeTracker.Clear();

   public void LogChangeTracker(string text) =>
      _logger?.LogInformation("{text}\n{change}", text, ChangeTracker.DebugView.LongView);
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

      var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      
      var localFolder = configuration.GetSection("LocalFolder").Value ??
         throw new Exception("LocalFolder is not available");
      localFolder.Split('/').ToList().ForEach(folder => {
         path = Path.Combine(path, folder);
      });
      if (!Directory.Exists(path)) Directory.CreateDirectory(path);
      
      // read active database configuration from appsettings.json
      var useDatabase = configuration.GetSection("UseDatabase").Value ??
         throw new Exception("UseDatabase is not available");

      // read connection string from appsettings.json
      var connectionString = configuration.GetSection("ConnectionStrings")[useDatabase]
         ?? throw new Exception("ConnectionStrings is not available"); 
      
      switch (useDatabase) {
         case "LocalDb":
            var dbFile = $"{Path.Combine(path, connectionString)}.mdf";
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
               "Data Source=" + Path.Combine(path, connectionString) + ".db";
            Console.WriteLine($"....: EvalDatabaseConfiguration: Sqlite {dataSourceSqlite}");
            return (useDatabase, dataSourceSqlite);
         case "SqliteInMemory":
            var dataSourceSqliteInMemory = "Data Source=:memory:";
            Console.WriteLine($"....: EvalDatabaseConfiguration: SqliteInMemory {dataSourceSqliteInMemory}");
            return (useDatabase, dataSourceSqliteInMemory);
         
         default:
            throw new Exception("appsettings.json Problems with database configuration");
      }   }
   #endregion
   
}
