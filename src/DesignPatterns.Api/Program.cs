using System.Text.Json.Serialization;
using DesignPatterns.Api.Configuration;
using DesignPatterns.Api.Controller;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddValidation()
    .AddStrategyConfiguration()
    .AddDecoratorConfiguration(Path.Combine(builder.Environment.ContentRootPath, "reports"));

builder
    .Build()
    .UseOpenApiConfiguration()
    .RegisterStrategyControllers()
    .RegisterDecoratorControllers()
    .UseMiddlewareConfiguration()
    .Run();

// Entry point for WebApplicationFactory integration tests.
public partial class Program { }
