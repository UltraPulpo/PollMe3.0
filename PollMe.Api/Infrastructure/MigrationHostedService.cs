using FluentMigrator.Runner;

namespace PollMe.Api.Infrastructure;

public class MigrationHostedService(IMigrationRunner runner) : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        runner.MigrateUp();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
