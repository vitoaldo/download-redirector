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
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            
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
                    var files = Directory.EnumerateFiles(downloadsPath);

                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file).ToLowerInvariant();
                        var fileName = Path.GetFileName(file);
                        
                        if (extension == ".crdownload" || extension == ".part" || extension == ".tmp")
                            continue;

                        if (IsFileLocked(file))
                        {
                            _logger.LogDebug("Arquivo ignorado por estar em uso (presumivelmente baixando): {file}", fileName);
                            continue;
                        }

                        string targetFolderCategory = "Outros";

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

                        if (File.Exists(targetFilePath))
                        {
                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            targetFilePath = Path.Combine(targetDirectoryPath, $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                        }

                        File.Move(file, targetFilePath);
                        _logger.LogInformation("Arquivo movido com sucesso: {file} para a pasta {category}", fileName, targetFolderCategory);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao tentar organizar os arquivos da pasta Downloads.");
                }
                
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na execucao com Download Redirector.");
            Environment.Exit(1);
        }
        finally
        {
            _logger.LogInformation("Download Redirector Fim de servico em: {time}", DateTimeOffset.Now);
        }
    }

    /// <summary>
    /// Verifica se um arquivo está bloqueado por outro processo (ex: navegador efetuando o download ou um arquivo temporário gravando).
    /// </summary>
    private bool IsFileLocked(string filePath)
    {
        try
        {
            using (FileStream stream = new FileInfo(filePath).Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            return true;
        }
        
        return false;
    }
}
