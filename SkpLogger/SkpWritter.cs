using SkpLogger.Exceptions;
using SkpLogger.Foundations;
using SkpLogger.Models;

namespace SkpLogger;

public class SkpWritter:ISkpWritter
{

    /**************************************************************************
     *                          LOG CONTENT PROPERTIES  
     **************************************************************************/
    private string _startLine;
    private StartLineParams _startLineParams;
    public string Header { get; set; }
    public string _serviceName;
    
    public Dictionary<string,string> Properties { get; set; }
    
    /**************************************************************************
     *                           COLOR PROPERTIES
     ***************************************************************************/
    public ConsoleColor InfoColor { get;}
    public ConsoleColor DebugColor { get;}
    public ConsoleColor ErrorColor { get;}
    public ConsoleColor LoggerErrorColor { get; }
    public ConsoleColor WarningColor { get;}
    public ConsoleColor SuccessColor { get; }

    private Dictionary<string, ConsoleColor> _customColorDescriptions;
    
    
    /***************************************************************************
     *                              DISPLAY RULES
     ***************************************************************************/
    
    private DisplayRules _displayRules;
    private DisplayRules _saveRules;
    
    
    
    /***************************************************************************
    *                              Extra properties
    ***************************************************************************/
    private string _logPath;
    private string _actualFile;

    private bool _readyToSave;
    private string _dateFormat = "hh:mm:ss";
    private string _folderSeparator;

    public SkpWritter(OperatingSystemEnum operatingSystem)
    {
        if (operatingSystem == OperatingSystemEnum.LINUX)
            _folderSeparator = "/";
        else 
            _folderSeparator = "\\";
        
        _startLine = "[@ServiceName]->[@DateNow]---->";
        _startLineParams = new StartLineParams();
        _startLineParams.SetServiceName("Module");
        _serviceName = "Module";
        InfoColor = ConsoleColor.White;
        DebugColor = ConsoleColor.Cyan;
        SuccessColor = ConsoleColor.Green;
        WarningColor = ConsoleColor.Yellow;
        LoggerErrorColor = ConsoleColor.DarkRed;
        ErrorColor = ConsoleColor.Red;
        _customColorDescriptions = new Dictionary<string, ConsoleColor>();
        _displayRules = new();
        _saveRules = new();
        _logPath = string.Empty;
        Properties = new Dictionary<string, string>();
        _readyToSave = false;
        
        


    }
    #region BuilderProperties
    public SkpWritter WithDisplayRules(DisplayRules rules)
    {
        _displayRules = rules;
        return this;
    }

    public SkpWritter WithSaveRules(DisplayRules rules)
    {
        _saveRules = rules;
        return this;
    }

    public SkpWritter WithLogPath(string path)
    {
        _logPath = path;
        return this;
    }

    public SkpWritter WithServiceName(string serviceName)
    {
        _serviceName = serviceName;
        _startLineParams.SetServiceName(serviceName); 
        return this;
    }
    public SkpWritter WithLineStart(string lineStart)
    {
        _startLine = lineStart;
        return this;
    }

    public SkpWritter AddLineStartParam(string paramName,string value)
    {
        _startLineParams.Params.Add(paramName);
        _startLineParams.Values.Add(paramName,value);
        return this;
    }

    public SkpWritter WithLineStartParams(string lineStart, StartLineParams parameters)
    {
        _startLine = lineStart;
        _startLineParams = parameters;
        return this;
    }
    #endregion

    private string BuildDefaultHeader()
    {
        string lineOfEquals = "\t\t=================================================== ";
        string header = lineOfEquals;
        
        if (!string.IsNullOrEmpty(_serviceName))
            header += $"      \n\t\t|                      Logs                       |" ;
        else
            header += $"\n\t\t|                   Module Logs                   |";

        header += "\n"+lineOfEquals + "\n\nService information: ";
        Properties.Add("Logger version", "1.0");
        Properties.Keys.ToList().ForEach(key =>
        {
            header += $"\n[{key}] -> {Properties[key]} ";
        });

        header += "\n\n";

        return header;
        
    }

    public void Initialize()
    {
        string folderMonthName = GetFolderMonthName();
        string fullFilePath = $"{folderMonthName}{_folderSeparator}{GetFileName()}";

        try
        {
            Header = BuildDefaultHeader();
            if (!Directory.Exists(folderMonthName))
                Directory.CreateDirectory(folderMonthName);
            
            if (!File.Exists(fullFilePath))
            {
                using (var fc= File.Create(fullFilePath)){};
                using StreamWriter sw = File.CreateText(fullFilePath);
                sw.Write(Header);
                _actualFile = fullFilePath;
            }

            else
            {
                using StreamWriter sw = File.AppendText(fullFilePath);
                sw.WriteLine("Starting a new instance of SkpWritter...");

                _actualFile = fullFilePath;
            }

            _readyToSave = true;
        }
        catch (Exception ex)
        {
            WriteLogError($"Error in logger initialization. Exception message: {ex.Message}","Initialize()");
            _readyToSave = false;
        }
    }

    private string GetFolderMonthName()
    {
        DateTime now = DateTime.Now;
        string year = now.Year.ToString();
        string month = now.Month.ToString();
        string day = now.Day.ToString();
        return $"{_logPath}{_folderSeparator}runtime{_folderSeparator}{month}-{year}";
    }

    private string GetFileName()
    {
        DateTime now = DateTime.Now;
        string month = now.Month.ToString();
        string day = now.Day.ToString();
        return $"log-{month}-{day}.log";
    }

    private string GetFullFilePath() =>$"{GetFolderMonthName()}{_folderSeparator}{GetFileName()}";
    
    private string GetActualLogFile()
    {
        string fullFilePath = GetFullFilePath();
        
        if (fullFilePath != _actualFile)
        {
            _readyToSave = false; //se tiene que hacer algo con los logs que se imprimen entre que se cambia readyToFalse a false y entre que se genera el nuevo archivo para comenzar a guardar de nuevo.
            Initialize();
        }

        if (_readyToSave) return _actualFile;
        else
        {
            throw new RuntimeLogFileGenerationException("Error generating the new log file. ");
        }
    }
    
    public void AddColorDescription(string description, ConsoleColor color)
    {
        if (color == InfoColor && color == DebugColor && color == SuccessColor && color == WarningColor &&
            color == ErrorColor)
        {
            return;
        }

        if (string.IsNullOrEmpty(description))
        {
            return;
        }

        if (_customColorDescriptions.ContainsKey(description))
        {
            return;
        }
        _customColorDescriptions.Add(description,color);
    }


    
    public void WriteLine(string text, SkpLogTypesEnum type)
    {
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        switch (type)
        {
            case SkpLogTypesEnum.INFO:
        
                if (!_displayRules.Info) return;
                Console.ForegroundColor= InfoColor;
                
                if(_saveRules.Info) SaveLog(text);
                break;
            case SkpLogTypesEnum.DEBUG:
                if (!_displayRules.Debug) return;
                Console.ForegroundColor = DebugColor;
                
                if(_saveRules.Debug) SaveLog(text);
                break;
            case SkpLogTypesEnum.WARNING:
                if (!_displayRules.Warning) return;
                Console.ForegroundColor = WarningColor;
                if(_saveRules.Success) SaveLog(text);
                break;
            case SkpLogTypesEnum.ERROR:
                if (!_displayRules.Error) return;
                Console.ForegroundColor = ErrorColor;
                if(_saveRules.Error) SaveLog(text);
                break;
            case SkpLogTypesEnum.SUCCESS:
                if (!_displayRules.Success) return;
                Console.ForegroundColor = SuccessColor;
                if(_saveRules.Success) SaveLog(text);
                break;
            default:
                Console.ForegroundColor = InfoColor;
                break;
        }
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
    }
   
    public void WriteInfo(string text)
    {
        if (!_displayRules.Info) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = InfoColor;
        Console.Write(lineStart);
        Console.WriteLine(text);
        
        
        
        if(_saveRules.Info) SaveLog(text);
    }
    
    public void WriteDebug(string text)
    {
        if (!_displayRules.Debug) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = DebugColor;
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
        
        if(_saveRules.Debug) SaveLog(text);
    }
    
    public void WriteWarning(string text)
    {
        if (!_displayRules.Warning) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = WarningColor;
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
        
        if(_saveRules.Warning) SaveLog(text);
    }
    
    public void WriteError(string text)
    {
        if (!_displayRules.Error) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = ErrorColor;
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
        
        if(_saveRules.Error) SaveLog(text);
    }
    
    private void WriteLogError(string text, string method)
    {
        if (!_displayRules.LogError) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = LoggerErrorColor;
        Console.Write(lineStart);
        Console.WriteLine($"{_startLine}->[LOGGER_ERROR]->[{method}]->{text}");
        Console.ForegroundColor = InfoColor;
        
        // if(_saveRules.LogError && method != "SaveLog()")
        //     SaveLog(text);
    }

    public void WriteSuccess(string text)
    {
        if (!_displayRules.Success) return;
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        
        Console.ForegroundColor = SuccessColor;
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
        
        if(_saveRules.Success)
            SaveLog(text);
    }

    public void WriteCustom(string type, string text)
    {
        if (!_customColorDescriptions.ContainsKey(type))
        {
            WriteLogError("Error: type was not found. ","WriteCustom()");
        }
        _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
        _startLineParams.SetServiceName(_serviceName);
        string lineStart = _startLineParams.ReplaceParams(_startLine);
        Console.ForegroundColor = _customColorDescriptions[type];
        Console.Write(lineStart);
        Console.WriteLine(text);
        Console.ForegroundColor = InfoColor;
    }
    

    public string SaveCustomLog(string text,string name, dynamic data)
    {
        if (!_readyToSave)
        {
            WriteLogError("Could not save, SkpWritter was not be initialized. ", "SaveCustomLog()");
            return string.Empty;
        }
        
        DateTime now = DateTime.Now;
        string year = now.Year.ToString();
        string month = now.Month.ToString();
        string day = now.Day.ToString();

        
        string folderMonthName = $"{_logPath}{_folderSeparator}custom{_folderSeparator}{month}-{year}";
        string folderDayName = $"{folderMonthName}{_folderSeparator}{day}-{month}-logs{_folderSeparator}";
        string fileName = $"log-{name}-{Guid.NewGuid()}.log";
        string fullName = folderDayName + fileName;

        try
        {
            if (!Directory.Exists(folderMonthName))
                Directory.CreateDirectory(folderMonthName);
            if (!Directory.Exists(folderDayName))
                Directory.CreateDirectory(folderDayName);


            using StreamWriter sw = File.CreateText(fullName);
            sw.WriteLine("\t LOG DESCRIPTION: ");
            sw.WriteLine(text);
            sw.WriteLine("\t LOG CONTENT: ");
            sw.WriteLine(data.ToString());
        }
        catch (Exception ex)
        {
           WriteLogError($"Error trying save the log. LogName: {name}. Exception Message: {ex.Message}. Object: {ex.Source}","SaveCustomLog()"); 
        }

        return fullName;
    }
    
    private void SaveLog(string text)
    {        
        if (!_readyToSave)
        {
            WriteLogError("Could not save, SkpWritter was not be initialized. ", "SaveCustomLog()");
            return;
        }
 
        try
        {
            string file = GetActualLogFile();
            if (string.IsNullOrEmpty(file)) throw new Exception("Error getting the full file path");
            
            _startLineParams.SetDateNow(DateTime.Now.ToString(_dateFormat));
            _startLineParams.SetServiceName(_serviceName);
            string lineStart = _startLineParams.ReplaceParams(_startLine);
            
            using StreamWriter fs= File.AppendText(_actualFile);
            
            fs.Write(lineStart);
            fs.WriteLine(text+"\n");

        }
        catch (RuntimeLogFileGenerationException ex)
        {
            WriteLogError(ex.Message, "SaveLog()");
        }
        catch (Exception ex)
        {
            WriteLogError("Error saving the runtime log","SaveLog()");
        }
    }
    



}