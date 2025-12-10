namespace EventManagementSystem.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private PeriodicTimer? _timer;

        public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder background service starting");

            // Run immediately on startup
            await DoWork(stoppingToken);

            // Then run every 1 hour
            _timer = new PeriodicTimer(TimeSpan.FromHours(1));

            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Reminder background service stopping");
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background job executing at: {time}", DateTimeOffset.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var reminderService = scope.ServiceProvider.GetRequiredService<IEmailReminderService>();
                await reminderService.SendEventRemindersAsync();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
