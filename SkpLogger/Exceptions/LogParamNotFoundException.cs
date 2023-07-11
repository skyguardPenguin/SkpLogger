namespace SkpLogger.Exceptions;

public class LogParamNotFoundException:Exception
{
    public LogParamNotFoundException(string message):base(message) { }
    
    public LogParamNotFoundException():base("The param kay was not found in dictionary. "){}
}