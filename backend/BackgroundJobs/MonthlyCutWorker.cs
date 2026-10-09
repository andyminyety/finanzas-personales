using FinanzasApi.Services;

namespace FinanzasApi.BackgroundJobs;

// Automatic cut. Every 6 hours it checks whether any user has an ended period without a cut
// and creates it. Since the cut is idempotent (unique index), checking several times a day is safe
// and it also recovers missed cuts if the API was down.
public class MonthlyCutWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _config;
    private readonly ILogger<MonthlyCutWorker> _logger;

    public MonthlyCutWorker(IServiceScopeFactory scopes, IConfiguration config, ILogger<MonthlyCutWorker> logger)
    {
        _scopes = scopes;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue("Cuts:AutoEnabled", true))
        {
            _logger.LogInformation("Corte automático desactivado (Cuts:AutoEnabled=false).");
            return;
        }

        try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } // let the API finish starting
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ICutService>();
                var created = await service.RunAutomaticAsync();
                _logger.LogInformation("Corte automático: {Count} corte(s) generado(s).", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el corte automático");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
