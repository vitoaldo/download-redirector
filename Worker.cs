namespace download_redirector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Download Redirector Service started at: {time}", DateTimeOffset.Now);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO: Adicione a lógica do seu serviço aqui.
                // Exemplo: verificar arquivos, processar downloads, etc.

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker trigger executing at: {time}", DateTimeOffset.Now);
                }
                
                // Intervalo de espera entre cada execução (ex: 5 segundos)
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // O serviço está sendo parado normalmente.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred in the execution loop.");
            Environment.Exit(1); // Encerra o processo de forma fatal se houver uma falha crítica.
        }
        finally
        {
            _logger.LogInformation("Download Redirector Service stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
