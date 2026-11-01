using System.Net;
using System.Net.Http.Json;
using System.Text;
using DesignPatterns.Api.Configuration;
using DesignPatterns.Application.Data.DecoratorPattern.Reports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DesignPatterns.Tests;

public class ReportsApiTests
{
    [Fact]
    public async Task Valid_report_runs_decorator_chain_and_saves_readable_gzip()
    {
        using var directory = new TestDirectory();
        await using var factory = new ReportsFactory(directory.Path);
        using HttpClient client = CreateClient(factory);
        byte[] content = Encoding.UTF8.GetBytes("Monthly sales čřž");
        using HttpResponseMessage response = await client.PostAsJsonAsync("/Decorator/Reports", new { name = "sales.txt", content });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        string file = Assert.Single(Directory.GetFiles(directory.Path));
        Assert.Equal("sales.txt.gz", Path.GetFileName(file));
        Assert.Equal(content, await ReportStorageTests.Decompress(await File.ReadAllBytesAsync(file)));
    }

    [Theory]
    [InlineData("", "SGVsbG8=")]
    [InlineData("../escape.txt", "SGVsbG8=")]
    [InlineData("..\\escape.txt", "SGVsbG8=")]
    [InlineData("report.txt", "")]
    [InlineData("report.txt", null)]
    [InlineData(null, "SGVsbG8=")]
    public async Task Invalid_report_returns_400_without_creating_files(string? name, string? content)
    {
        using var directory = new TestDirectory();
        await using var factory = new ReportsFactory(directory.Path);
        using HttpClient client = CreateClient(factory);
        using HttpResponseMessage response = await client.PostAsJsonAsync("/Decorator/Reports", new { name, content });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(Directory.Exists(directory.Path));
    }

    [Theory]
    [InlineData("{\"name\":\"a.txt\",\"content\":\"not base64!\"}")]
    [InlineData("{")]
    [InlineData("null")]
    public async Task Invalid_json_or_base64_returns_400(string json)
    {
        using var directory = new TestDirectory();
        await using var factory = new ReportsFactory(directory.Path);
        using HttpClient client = CreateClient(factory);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync("/Decorator/Reports", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(Directory.Exists(directory.Path));
    }

    private static HttpClient CreateClient(ReportsFactory factory) => factory.CreateClient(
        new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private sealed class ReportsFactory(string directory) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReportStorage>();
                // Use the production decorator registration with an isolated output directory.
                services.AddDecoratorConfiguration(directory);
            });
        }
    }
}
