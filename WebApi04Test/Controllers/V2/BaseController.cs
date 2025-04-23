using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;
using WebApi.Controllers.V2;
using WebApi.Core;
using WebApi.Data;
using WebApiTest.Di;
using WebApiTest.Persistence.Repositories;
using Xunit;
namespace WebApiTest.Controllers;
[Collection(nameof(SystemTestCollectionDefinition))]
public abstract class BaseController {
   protected readonly PeopleController _peopleController;
   protected readonly CarsController _carsController;
   
   protected readonly IPeopleRepository _peopleRepository;
   protected readonly ICarsRepository _carsRepository;
   protected readonly IDataContext _dataContext;
   protected readonly Seed _seed;
   
   protected string _webRootPath;
   protected string _imagesSource;
   protected string _imagesWwwRootPath;
   private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
   

   protected BaseController() {
      
      // Create test configuration
      var configuration = new ConfigurationBuilder()
         .AddJsonFile("appsettingsTest.json", optional: false)
         .Build();

      // Set up paths
      var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      // set up IWebHostEnvironment ans image files
      SetupImages(configuration, path);
      
      // Test DI-Container
      IServiceCollection services = new ServiceCollection();
   
      // Add Repositories, Test Databases, ...
      var (useDatabase, dataSource) = services.AddDataTest();
      
      // Add ImageService
      // services.AddScoped<ImageService>();
      
      // Build ServiceProvider,
      // and use Dependency Injection or Service Locator Pattern
      var serviceProvider = services.BuildServiceProvider()
         ?? throw new Exception("Failed to create an instance of ServiceProvider");

      //-- Service Locator 
      var dbContext = serviceProvider.GetRequiredService<DataContext>()
         ?? throw new Exception("Failed to create DbContext");

      // In-Memory
      if (useDatabase == "SqliteInMemory") {
         dbContext.Database.OpenConnection();
      } else if (useDatabase == "Sqlite") {
         dbContext.Database.EnsureDeleted();
         // Workaround  SQLite I/O Errors
         SqliteConnection connection = new SqliteConnection(dataSource);
         connection.Open();
         using var command = connection.CreateCommand();
         // Switch the journal mode from WAL to DELETE.
         command.CommandText = "PRAGMA journal_mode = DELETE;";
         var result = command.ExecuteScalar();
         Console.WriteLine("Current journal mode: " + result);
         connection.Close();
      } else {
         dbContext.Database.EnsureDeleted();
      }
      dbContext.Database.EnsureCreated();
      
      _dataContext = serviceProvider.GetRequiredService<IDataContext>()
         ?? throw new Exception("Failed to create an instance of IDataContext");

      _peopleRepository = serviceProvider.GetRequiredService<IPeopleRepository>()
         ?? throw new Exception("Failed create an instance of IPeopleRepository");
      _carsRepository = serviceProvider.GetRequiredService<ICarsRepository>()
         ?? throw new Exception("Failed create an instance of ICarsRepository");
      
      _peopleController = new PeopleController(_peopleRepository, _dataContext);
      _carsController = new CarsController(_peopleRepository,_carsRepository,_dataContext);
      
      _seed = new Seed();

   }

   private void SetupImages(IConfiguration configuration, string homePath ) {
      var localFolder = configuration.GetSection("LocalFolder").Value ??
         throw new Exception("LocalFolder is not available");
      localFolder.Split('/').ToList().ForEach(folder => {
         homePath = Path.Combine(homePath, folder);
      });
      if (!Directory.Exists(homePath)) Directory.CreateDirectory(homePath);

      // Set up source and destination directories
      _imagesSource = Path.Combine(homePath, "Images");
      if (!Directory.Exists(_imagesSource)) Directory.CreateDirectory(_imagesSource);
      _webRootPath = Path.Combine(homePath, $"wwwroot");
      if (!Directory.Exists(_webRootPath)) Directory.CreateDirectory(_webRootPath);
      _imagesWwwRootPath = Path.Combine(_webRootPath, "images");
      if(!Directory.Exists(_imagesWwwRootPath)) Directory.CreateDirectory(_imagesWwwRootPath);

      // Create web environment
      _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
      _mockWebHostEnvironment.Setup(env => env.WebRootPath).Returns(_webRootPath);
      _mockWebHostEnvironment.Setup(env => env.ContentRootPath).Returns(_webRootPath);
      _mockWebHostEnvironment.Setup(env => env.EnvironmentName).Returns("Test");
      _mockWebHostEnvironment.Setup(env => env.WebRootFileProvider).Returns(new PhysicalFileProvider(_webRootPath));

   } 
   
   
   public static string ToPrettyJson(
      string text, 
      object obj
   ) {
      return 
         text + "\n" +
         JsonSerializer.Serialize(obj, new JsonSerializerOptions {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
            //  MaxDepth = 2
         });
   }
}