namespace download_redirector.Models;

public class WatcherSettings
{
    public int ExecutionIntervalMinutes { get; set; } = 20;
    public List<WatchedFolder> Folders { get; set; } = new();
}

public class WatchedFolder
{
    public string SourcePath { get; set; } = string.Empty;
    public string DefaultTargetPath { get; set; } = string.Empty;
    public List<TargetFolder> Targets { get; set; } = new();
}

public class TargetFolder
{
    public string TargetPath { get; set; } = string.Empty;
    public List<string> Extensions { get; set; } = new();
}
