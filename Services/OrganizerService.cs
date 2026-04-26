using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using download_redirector.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace download_redirector.Services;

public class OrganizerService : BackgroundService
{
    private readonly ILogger<OrganizerService> _logger;
    private readonly IConfiguration _configuration;
    
    public bool IsPaused { get; private set; } = false;

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);

    public OrganizerService(ILogger<OrganizerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrganizerService iniciado em: {time}", DateTimeOffset.Now);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var watcherSettings = _configuration.GetSection("WatcherSettings").Get<WatcherSettings>() ?? new WatcherSettings();
                int intervalMinutes = watcherSettings.ExecutionIntervalMinutes > 0 ? watcherSettings.ExecutionIntervalMinutes : 20;

                if (!IsPaused)
                {
                    PerformOrganization(watcherSettings);
                }

                // Esvaziamento Extremo de RAM:
                // Quando o app terminar o trabalho, forçamos o Windows a transferir toda a RAM alocada para o disco de paginação
                TrimMemory();

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal no OrganizerService.");
        }
    }

    private void PerformOrganization(WatcherSettings settings)
    {
        try
        {
            var foldersToWatch = settings.Folders ?? new List<WatchedFolder>();

            foreach (var folder in foldersToWatch)
            {
                var sourcePath = Environment.ExpandEnvironmentVariables(folder.SourcePath);
                var defaultTargetPath = string.IsNullOrWhiteSpace(folder.DefaultTargetPath) 
                    ? Path.Combine(sourcePath, "Outros") 
                    : Environment.ExpandEnvironmentVariables(folder.DefaultTargetPath);

                if (!Directory.Exists(sourcePath))
                {
                    _logger.LogWarning("Pasta monitorada nao encontrada: {path}", sourcePath);
                    continue;
                }

                // Otimização CPU: Cacheamos as strings expandidas fora do loop gigante de arquivos
                var targetCache = new List<(string TargetPath, List<string> Extensions)>();
                if (folder.Targets != null)
                {
                    foreach (var target in folder.Targets)
                    {
                        if (target.Extensions != null && target.Extensions.Any())
                        {
                            targetCache.Add((
                                Environment.ExpandEnvironmentVariables(target.TargetPath),
                                target.Extensions.Select(e => e.ToLowerInvariant()).ToList()
                            ));
                        }
                    }
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
                        continue;
                    }

                    string targetDirectoryPath = defaultTargetPath;

                    foreach (var target in targetCache)
                    {
                        if (target.Extensions.Contains(extension))
                        {
                            targetDirectoryPath = target.TargetPath;
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
            _logger.LogError(ex, "Erro inesperado ao organizar arquivos.");
        }
    }

    private bool IsFileLocked(string filePath)
    {
        try
        {
            using FileStream stream = new FileInfo(filePath).Open(FileMode.Open, FileAccess.Read, FileShare.None);
            stream.Close();
        }
        catch (IOException)
        {
            return true;
        }
        return false;
    }

    private void TrimMemory()
    {
        try
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
        }
        catch
        {
            // Failsafe silencioso
        }
    }
}
