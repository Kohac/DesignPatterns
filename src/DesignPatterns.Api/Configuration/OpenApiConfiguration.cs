using Scalar.AspNetCore;

namespace DesignPatterns.Api.Configuration;

public static class OpenApiConfiguration
{
    public static WebApplication UseOpenApiConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;
        app.MapOpenApi();

        app.UseSwaggerUI(opt =>
        {
            opt.SwaggerEndpoint("/openapi/v1.json", "Design Patterns API");
            opt.RoutePrefix = "swagger";
        });

        app.MapScalarApiReference(opt =>
        {
            opt.ShowDeveloperTools = DeveloperToolsVisibility.Localhost;
            opt.WithTitle("Design Patterns API")
                .ForceDarkMode()
                .WithClassicLayout()
                .ShowOperationId()
                .SortTagsAlphabetically()
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
        return app;
    }
}
