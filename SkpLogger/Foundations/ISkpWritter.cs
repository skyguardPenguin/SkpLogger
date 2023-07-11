using SkpLogger.Models;

namespace SkpLogger.Foundations;

public interface ISkpWritter
{
    public SkpWritter WithDisplayRules(DisplayRules rules);
    public SkpWritter WithSaveRules(DisplayRules rules);
    public SkpWritter WithLogPath(string path);
    public SkpWritter WithServiceName(string serviceName);
    public SkpWritter WithLineStart(string lineStart);
    public SkpWritter AddLineStartParam(string paramName, string value);
    public SkpWritter WithLineStartParams(string lineStart, StartLineParams parameters);
    public void Initialize();
    public void AddColorDescription(string description, ConsoleColor color);
    public void WriteLine(string text, SkpLogTypesEnum type);
    public void WriteInfo(string text);
    public void WriteDebug(string text);
    public void WriteWarning(string text);
    public void WriteError(string text);
    public void WriteSuccess(string text);
    public void WriteCustom(string type, string text);
    public string SaveCustomLog(string text, string name, dynamic data);
}