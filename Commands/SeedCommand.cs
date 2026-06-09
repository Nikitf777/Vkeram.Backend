using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Commands;

[CliCommand(Name = "seed", Description = "Seed the database with demo data")]
public class SeedCommand
{
    public async Task RunAsync()
    {
        var serviceProvider = AppServiceProvider.Instance;
        if (serviceProvider == null)
        {
            Console.Error.WriteLine("Service provider not available.");
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDemoDataSeeder>();
        await seeder.SeedAsync();
    }
}
