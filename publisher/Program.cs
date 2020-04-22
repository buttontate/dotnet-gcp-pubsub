using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using shared;

namespace publisher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.RegisterSharedDependencies();
                    services.AddScoped<IItemService, ItemService>();
                    services.AddHostedService<Worker>();
                });
    }
}