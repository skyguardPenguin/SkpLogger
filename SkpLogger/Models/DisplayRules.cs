namespace SkpLogger.Models;

public class DisplayRules
{
    public bool Info { get; set; }
    public bool Debug { get; set; }
    public bool Warning { get; set; }
    public bool LogError { get; set; }
    public bool Error { get; set; }
    public bool Success { get; set; }

    public DisplayRules()
    {
        Info = Debug = Warning = LogError = Success = Error = true;
    }

    public DisplayRules(bool info, bool debug, bool warning, bool logError, bool error, bool success)
    {
        Info = info;
        Debug = debug;
        Warning = warning;
        LogError = logError;
        Error = error;
        Success = success;
    }
    
}