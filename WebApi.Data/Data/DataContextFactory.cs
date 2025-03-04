using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WebApi.Data;
namespace WebApi.Persistence;
internal class DataContextFactory : IDesignTimeDbContextFactory<DataContext> {

   public DataContext CreateDbContext(string[] args) {
      // Nuget:  Microsoft.Extensions.Configuration
      //       + Microsoft.Extensions.Configuration.Json
      var configuration = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appSettingsMigrations.json", false)
                         .Build();

      var (useDatabase, dataSource) = 
         DataContext.EvalDatabaseConfiguration(configuration);

      var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
      switch (useDatabase) {
         case "LocalDb":
            Console.WriteLine("....: LocalDb");
            Console.WriteLine($"....: ConnectionsString {dataSource}");
            optionsBuilder.UseSqlServer(dataSource);
            break;
         case "SqlServer":
            Console.WriteLine("....: SQLServer");
            Console.WriteLine($"....: ConnectionsString {dataSource}");
            optionsBuilder.UseSqlServer(dataSource);
            break;
         case "Sqlite":
            Console.WriteLine("....: Sqlite");
            Console.WriteLine($"....: ConnectionsString {dataSource}");
            optionsBuilder.UseSqlite(dataSource);
            break;
         default:
            throw new Exception($"appsettings.json UseDatabase {useDatabase} not available");
      }
      return new DataContext(optionsBuilder.Options);
   }
}