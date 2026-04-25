using Microsoft.Extensions.Configuration;
using download_redirector.Models;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker ativou execucao em: {time}", DateTimeOffset.Now);
                }

                try
                {
                    var watcherSettings = _configuration.GetSection("WatcherSettings").Get<WatcherSettings>();
                    var foldersToWatch = watcherSettings?.Folders ?? new List<WatchedFolder>();

                    foreach (var folder in foldersToWatch)
                    {
                        var sourcePath = Environment.ExpandEnvironmentVariables(folder.SourcePath);
                        var defaultTargetPath = string.IsNullOrWhiteSpace(folder.DefaultTargetPath) 
                            ? Path.Combine(sourcePath, "Outros") 
                            : Environment.ExpandEnvironmentVariables(folder.DefaultTargetPath);

                        if (!Directory.Exists(sourcePath))
                        {
                            _logger.LogWarning("A pasta monitorada nao foi encontrada: {path}", sourcePath);
                            continue;
                        }

                        var files = Directory.EnumerateFiles(sourcePath);

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

                            string targetDirectoryPath = defaultTargetPath;

                            foreach (var target in folder.Targets)
                            {
                                if (target.Extensions.Any(e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase)))
                                {
                                    targetDirectoryPath = Environment.ExpandEnvironmentVariables(target.TargetPath);
                                    break;
                                }
                            }

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
                            _logger.LogInformation("Arquivo movido com sucesso: {file} para a pasta {targetDirectory}", fileName, targetDirectoryPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao tentar organizar os arquivos.");
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
