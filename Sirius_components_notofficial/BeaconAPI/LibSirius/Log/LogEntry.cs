using System.Text.Json.Serialization;

namespace LibSirius.Log;

public class LogEntry
{
    [JsonIgnore]
    public LogLevel Level { get; set; } = LogLevel.Info;
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
    public string Message { get; set; } = "";
    [JsonIgnore]
    public Exception? Exception { get; set; }
    public string? SenderInfo { get; set; } = "";
    public string SenderLocation { get; set; } = "";

    public string Type => Level.ToString();

    public override string ToString()
    {
        string logMessage = DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + Message;

        if (Exception != null)
        {
            Exception e = Exception;
            if (e.InnerException != null)
                e = e.InnerException;

            logMessage += "\n" + e.Message + "\n" + e.GetType() + "\n" + e.StackTrace;
        }

        return logMessage;
    }
}
