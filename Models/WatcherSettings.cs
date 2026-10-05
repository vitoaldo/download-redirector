namespace download_redirector.Models;

public enum RenameMode
{
    None,
    Manual,
    AI
}

public class ManualRenameRule
{
    public string Find { get; set; } = string.Empty;
    public string Replace { get; set; } = string.Empty;
}

public class WatcherSettings
{
    public int ExecutionIntervalMinutes { get; set; } = 20;
    public string ActiveAIProvider { get; set; } = "None";
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
    
    public RenameMode RenameMode { get; set; } = RenameMode.None;
    public List<ManualRenameRule> ManualRules { get; set; } = new();
    public string AIPrompt { get; set; } = "Retorne apenas o nome do arquivo final, sem aspas, ignorando a extensão.";
}
