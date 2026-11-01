using System.IO.Compression;
using System.Text;
using DesignPatterns.Application.Data.DecoratorPattern.Reports;
using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;
using Microsoft.Extensions.Logging;

namespace DesignPatterns.Tests;

public class ReportStorageTests
{
    [Fact]
    public async Task Compression_forwards_complete_gzip_copy_and_token_once()
    {
        var inner = new StubStorage();
        var storage = new CompressionReportStorage(inner);
        var request = new ReportCreateRequest("sales.txt", Encoding.UTF8.GetBytes("Monthly sales čřž"));
        using var cancellation = new CancellationTokenSource();

        await storage.Save(request, cancellation.Token);

        Assert.Equal(1, inner.Calls);
        Assert.Equal(cancellation.Token, inner.Token);
        ReportCreateRequest saved = Assert.IsType<ReportCreateRequest>(inner.Request);
        Assert.Equal("sales.txt.gz", saved.Name);
        Assert.Equal(request.Content, await Decompress(saved.Content));
        Assert.Equal("sales.txt", request.Name);
        Assert.NotSame(request, saved);
    }

    [Fact]
    public async Task Compression_does_not_call_inner_when_cancelled()
    {
        var inner = new StubStorage();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CompressionReportStorage(inner).Save(new ReportCreateRequest("a.txt", [1]), cancellation.Token));
        Assert.Equal(0, inner.Calls);
    }

    [Fact]
    public async Task File_storage_creates_directory_and_preserves_bytes()
    {
        using var directory = new TestDirectory();
        var storage = new FileReportStorage(directory.Path);
        await storage.Save(new ReportCreateRequest("a.bin", [0, 1, 255]), default);
        Assert.Equal(new byte[] { 0, 1, 255 }, await File.ReadAllBytesAsync(System.IO.Path.Combine(directory.Path, "a.bin")));
    }

    [Theory]
    [InlineData("../escape.txt")]
    [InlineData("..\\escape.txt")]
    [InlineData("/escape.txt")]
    [InlineData("a/b.txt")]
    public async Task File_storage_rejects_paths_before_creating_directory(string name)
    {
        using var directory = new TestDirectory();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new FileReportStorage(directory.Path).Save(new ReportCreateRequest(name, [1]), default));
        Assert.False(Directory.Exists(directory.Path));
    }

    [Fact]
    public async Task Logging_forwards_original_request_and_token_and_logs_success()
    {
        var inner = new StubStorage();
        var logger = new TestLogger();
        var request = new ReportCreateRequest("a.txt", [1]);
        using var cancellation = new CancellationTokenSource();
        await new LoggingReportStorage(inner, logger).Save(request, cancellation.Token);
        Assert.Same(request, inner.Request);
        Assert.Equal(cancellation.Token, inner.Token);
        Assert.Equal(1, inner.Calls);
        Assert.Equal(2, logger.Entries.Count);
        Assert.All(logger.Entries, entry => Assert.Equal(LogLevel.Information, entry.Level));
    }

    [Fact]
    public async Task Logging_preserves_original_exception_and_does_not_log_success()
    {
        var failure = new IOException("Disk unavailable");
        var inner = new StubStorage { Failure = failure };
        var logger = new TestLogger();
        IOException actual = await Assert.ThrowsAsync<IOException>(() =>
            new LoggingReportStorage(inner, logger).Save(new ReportCreateRequest("a.txt", [1]), default));
        Assert.Same(failure, actual);
        Assert.Equal(2, logger.Entries.Count);
        Assert.Equal(LogLevel.Error, logger.Entries[1].Level);
        Assert.Same(failure, logger.Entries[1].Exception);
    }

    [Fact]
    public async Task Logging_treats_requested_cancellation_as_information()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var logger = new TestLogger();
        var inner = new StubStorage { Failure = new OperationCanceledException(cancellation.Token) };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new LoggingReportStorage(inner, logger).Save(new ReportCreateRequest("a.txt", [1]), cancellation.Token));
        Assert.Equal(2, logger.Entries.Count);
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    internal static async Task<byte[]> Decompress(byte[] content)
    {
        using var input = new MemoryStream(content);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        await gzip.CopyToAsync(output);
        return output.ToArray();
    }

    private sealed class StubStorage : IReportStorage
    {
        public ReportCreateRequest? Request { get; private set; }
        public CancellationToken Token { get; private set; }
        public int Calls { get; private set; }
        public Exception? Failure { get; init; }
        public Task Save(ReportCreateRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            Request = request;
            Token = cancellationToken;
            return Failure is null ? Task.CompletedTask : Task.FromException(Failure);
        }
    }

    private sealed class TestLogger : ILogger<LoggingReportStorage>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Entries.Add((logLevel, exception));
    }
}

internal sealed class TestDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DesignPatterns-tests", Guid.NewGuid().ToString("N"));
    public void Dispose()
    {
        if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
    }
}
