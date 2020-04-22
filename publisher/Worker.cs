using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                DoWork();
                await Task.Delay(5000, stoppingToken);
            }
        }
        
        private void DoWork()
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedItemService = 
                scope.ServiceProvider
                    .GetRequiredService<IItemService>();

            scopedItemService.PublishRecentlyUpdatedItems();
        }
    }
}