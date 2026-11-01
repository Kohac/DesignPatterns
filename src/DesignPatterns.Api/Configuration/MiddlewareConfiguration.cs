namespace DesignPatterns.Api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication UseMiddlewareConfiguration(this WebApplication app)
    {
        app.UseHttpsRedirection();
        return app;
    }
}
