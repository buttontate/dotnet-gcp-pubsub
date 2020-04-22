using Microsoft.Extensions.DependencyInjection;
using shared.Data.Item;
using shared.Factories;

namespace shared
{
    public static class DependencyHelper
    {
        public static void RegisterSharedDependencies(this IServiceCollection services)
        {
            services.AddScoped<IItemData, ItemData>();
            services.AddScoped<IConnectionFactory, ConnectionFactory>();
        }
    }
}