using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Server.Data;

namespace Server.Services
{
    public class EmailBackgroundService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;
        private Timer _timer;

        public EmailBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<EmailBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email background service started.");
            _timer = new Timer(SendEmails, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async void SendEmails(object state)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var emailRepository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();

                    var emails = await emailRepository.GetInstructorEmailsForLabAsync();

                    foreach (var email in emails)
                    {
                        var subject = "e-Administration: Software License Expiration";
                        var body = "This is a reminder that your software license is about to expire. Please take necessary actions.";

                        await emailRepository.SendEmailAsync(email, subject, body);
                        _logger.LogInformation($"Email sent to {email}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending emails: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("Email background service stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
