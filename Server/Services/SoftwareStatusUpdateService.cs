using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Server.Repositories;

namespace Server.Services
{
    public class SoftwareStatusUpdateService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SoftwareStatusUpdateService> _logger;
        private Timer _timer;

        public SoftwareStatusUpdateService(IServiceProvider serviceProvider, ILogger<SoftwareStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule the task to run every minute for testing
            _timer = new Timer(UpdateSoftwareStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            _logger.LogInformation("SoftwareStatusUpdateService started.");
            return Task.CompletedTask;
        }

        private async void UpdateSoftwareStatus(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var softwareRepository = scope.ServiceProvider.GetRequiredService<ISoftwareRepository>();
                await softwareRepository.UpdateStatusForExpiredLicenses();
            }
            _logger.LogInformation("UpdateSoftwareStatus method executed by SoftwareStatusUpdateService.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("SoftwareStatusUpdateService stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
