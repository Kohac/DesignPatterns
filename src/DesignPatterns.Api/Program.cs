using System.Text.Json.Serialization;
using DesignPatterns.Api.Configuration;
using DesignPatterns.Api.Controller;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidation();
builder.Services.AddOpenApi()
    .AddStrategyConfiguration();

WebApplication app = builder
    .Build()
    .UseOpenApiConfiguration()
    .RegisterStrategy();

app.UseHttpsRedirection();

app.Run();
