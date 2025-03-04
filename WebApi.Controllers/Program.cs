using Microsoft.AspNetCore.HttpLogging;
using WebApi.Controllers.V2;
using WebApi.Core;
using WebApi.Data;
using WebApi.Data.Repositories;
using WebApi.Di;
namespace WebApi;

public class Program {
   public static void Main(string[] args) {
      var builder = WebApplication.CreateBuilder(args);

      // Configure logging
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.AddDebug();
      
      // Configure DI-Container -----------------------------------------
      // add http logging 
      builder.Services.AddHttpLogging();
      
      // Add ProblemDetails, see https://tools.ietf.org/html/rfc7807
      builder.Services.AddProblemDetails();
      
      // Add controllers
      builder.Services.AddControllers();
      builder.Services.AddScoped<ControllerHelper>();
      
      builder.Services.AddCore();
      builder.Services.AddData(builder.Configuration);
      
      // Add versioning
      builder.Services.AddApiVersioning();
      
      // Add OpenApi
      builder.Services.AddOpenApiSettings("v1");
      builder.Services.AddOpenApiSettings("v2");
      
      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.MapOpenApi();
         
         app.UseSwaggerUI(opt => {
            opt.SwaggerEndpoint("/openapi/v1.json", "CarShop API v1");
            opt.SwaggerEndpoint("/openapi/v2.json", "CarShop API v2");
         });
      }

      //app.UseAuthorization();

      app.MapControllers();

      app.Run();
   }
}
