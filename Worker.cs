using Microsoft.Extensions.Configuration;

namespace download_redirector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Download Redirector Inicio de servico em: {time}", DateTimeOffset.Now);

        try
        {
            // Obtém o caminho da pasta Downloads do usuário atual do Windows
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            
            // Lê as configurações do appsettings.json
            var organizerSettings = _configuration.GetSection("OrganizerSettings").Get<Dictionary<string, string[]>>() ?? new Dictionary<string, string[]>();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(downloadsPath))
                {
                    _logger.LogWarning("A pasta Downloads não foi encontrada em: {path}", downloadsPath);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker ativou execucao em: {time}", DateTimeOffset.Now);
                }

                try
                {
                    var files = Directory.GetFiles(downloadsPath);

                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file).ToLowerInvariant();
                        var fileName = Path.GetFileName(file);
                        
                        // Ignora arquivos temporários comuns de download em andamento
                        if (extension == ".crdownload" || extension == ".part" || extension == ".tmp")
                            continue;

                        string targetFolderCategory = "Outros"; // Categoria Padrão

                        // Procura uma categoria correspondente no appsettings
                        foreach (var category in organizerSettings)
                        {
                            if (category.Value.Contains(extension))
                            {
                                targetFolderCategory = category.Key;
                                break;
                            }
                        }

                        var targetDirectoryPath = Path.Combine(downloadsPath, targetFolderCategory);
                        var targetFilePath = Path.Combine(targetDirectoryPath, fileName);

                        if (!Directory.Exists(targetDirectoryPath))
                        {
                            Directory.CreateDirectory(targetDirectoryPath);
                        }

                        // Se o arquivo destino já existe, anexa um timestamp para evitar sobrescrever
                        if (File.Exists(targetFilePath))
                        {
                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            targetFilePath = Path.Combine(targetDirectoryPath, $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                        }

                        // Movendo o arquivo
                        File.Move(file, targetFilePath);
                        _logger.LogInformation("Arquivo movido com sucesso: {file} para a pasta {category}", fileName, targetFolderCategory);
                    }
                }
                catch (IOException ioEx)
                {
                    // Erros de IO geralmente indicam que o arquivo ainda está sendo baixado ou está em uso.
                    // Apenas registramos o alerta e tentamos novamente no próximo ciclo.
                    _logger.LogWarning("O arquivo pode estar em uso. Detalhes: {message}", ioEx.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar organizar os arquivos da pasta Downloads.");
                }
                
                // Intervalo de espera entre cada execução: 20 minutos
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // O serviço está sendo parado normalmente.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na execucao com Download Redirector.");
            Environment.Exit(1); // Encerra o processo de forma fatal se houver uma falha crítica.
        }
        finally
        {
            _logger.LogInformation("Download Redirector Fim de servico em: {time}", DateTimeOffset.Now);
        }
    }
}
