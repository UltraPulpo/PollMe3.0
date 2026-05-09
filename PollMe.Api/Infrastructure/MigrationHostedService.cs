using FluentMigrator.Runner;

namespace PollMe.Api.Infrastructure;

public class MigrationHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
