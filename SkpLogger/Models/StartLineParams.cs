using SkpLogger.Exceptions;

namespace SkpLogger.Models;

public class StartLineParams
{
    /**************************************************************************
    *                           STATIC FIELDS
    ***************************************************************************/
    
    /// <summary>
    /// Default param for display date
    /// </summary>
    public static string DateNow {get => "@DateNow"; }
    
    /// <summary>
    /// Default param for display service or api name
    /// </summary>
    public static string ServiceName { get=> "@ServiceName"; }
    
    
    
    /**************************************************************************
    *                           PUBLIC FIELDS
    ***************************************************************************/
    
    /// <summary>
    /// List of custom params to display
    /// </summary>
    public List<string> Params { get; set; }
    
    /// <summary>
    /// List of values of all params to display
    /// </summary>\
    public Dictionary<string,string> Values { get; set; }
    
    
    
    /**************************************************************************
    *                           PRIVATE FIELDS
    ***************************************************************************/
    /// <summary>
    /// Default datetime format to display in date params
    /// </summary>
    private string _defaultDateTimeFormat = "h:mm:ss";

    public StartLineParams()
    {
        Params = new();
        Params.Add(DateNow);
        Params.Add(ServiceName);
        Values = new()
        {
            { DateNow, DateTime.MinValue.ToString("h:mm:ss") },
            { ServiceName,"Module"}
        };
    }
    
    /// <summary>
    /// Replace the line start template with all params in te values dictionary.
    /// </summary>
    /// <param name="lineStartTemplate"></param>
    /// <returns></returns>
    public string ReplaceParams(string lineStartTemplate)
    {
        string aux = lineStartTemplate;
        Values.Keys.ToList().ForEach(key =>
        {
            aux = aux.Replace(key, Values[key]);
        });
        return aux;
    }

    /// <summary>
    /// Sets the date value to display in the lineStartTemplate
    /// </summary>
    /// <param name="value"></param>
    public void SetDateNow(string value) => Values[DateNow] = value;
    
    /// <summary>
    /// Sets the name of service that is using SkpLog
    /// </summary>
    /// <param name="value"></param>
    public void SetServiceName(string value) => Values[ServiceName] = value;

    /// <summary>
    /// Sets parameter value by his name
    /// </summary>
    /// <param name="param">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <exception cref="LogParamNotFoundException">When the parameter name not found in Values dictionary</exception>
    public void SetParamValue(string param, string value)
    {
        if (!Values.ContainsKey(param))
            throw new LogParamNotFoundException();
        Values[param] = value;
    } 







}