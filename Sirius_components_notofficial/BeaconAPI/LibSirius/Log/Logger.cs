using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LibSirius.Log;
public class Logger
{
    private static readonly Logger instance = new Logger();
    public static Logger I => instance;

    private ILogViewModel? DefaultLogbox;
    private readonly Dictionary<int, ILogViewModel> Link = new Dictionary<int, ILogViewModel>();
    private readonly Random Random = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    private Logger() { }

    public void AddDefaultLogBox(ILogViewModel logBox)
    {
        DefaultLogbox = logBox;
    }

    public void AddLogBox(object sender, ILogViewModel receiver)
    {
        Link.TryAdd(sender.GetHashCode(), receiver);
    }
    
    public void CopyLogBox(object originalSender, object newSender)
    {
        int senderKey = originalSender.GetHashCode();

        if (Link.TryGetValue(senderKey, out ILogViewModel? logViewModel))
            AddLogBox(newSender, logViewModel);
    }

    public void RemoveLogBox(ILogViewModel receiver)
    {
        List<int> toRemove = new List<int>();
        foreach (var elem in Link)
        {
            if (elem.Value.GetHashCode() == receiver.GetHashCode())
                toRemove.Add(elem.Key);
        }

        foreach (var i in toRemove)
            Link.Remove(i);
    }


    private string Log(object sender, LogEntry logEntry)
    {
        int senderKey = sender.GetHashCode();

        if (Link.TryGetValue(senderKey, out ILogViewModel? logViewModel))
            logViewModel.Add(logEntry);
        else if (DefaultLogbox != null)
            DefaultLogbox.Add(logEntry);
        else
        {
            string msg = string.Format("{0}->{1} {2}", logEntry.SenderInfo, logEntry.SenderLocation, logEntry.ToString());
            Debug.WriteLine(msg);
            return msg;
        }

        return "";
    }

    /// <summary>
    /// Log message
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerLineNumber"></param>
    /// <returns></returns>
    public string Log(object sender, LogLevel level, string message,
        [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = -1)
    {
        return Log(sender, new LogEntry
        {
            DateTime = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            SenderInfo = Path.GetFileName(callerFilePath),
            SenderLocation = $"{sender.GetType().Name}.{callerMemberName}:{callerLineNumber}",
        });
    }

    /// <summary>
    /// Log exceptions
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="message"></param>
    /// <param name="callerFilePath"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="callerLineNumber"></param>
    /// <returns></returns>
    public string Log(object sender, Exception e, string message = "",
        [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = -1)
    {
        return Log(sender, new LogEntry
        {
            DateTime = DateTimeOffset.UtcNow,
            Level = LogLevel.Exception,
            Exception = e,
            Message = $"Exception in function {callerMemberName}: {message}",
            SenderInfo = Path.GetFileName(callerFilePath),
            SenderLocation = $"{sender.GetType().Name}.{callerMemberName}:{callerLineNumber}",
        });
    }

    public string EasterEgg(object sender, double probability, string message,
        [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = -1)
    {
        if (Random.NextDouble() > probability)
            return "";

        return Log(sender, new LogEntry
        {
            DateTime = DateTimeOffset.UtcNow,
            Level = LogLevel.EasterEgg,
            Message = message,
            SenderInfo = Path.GetFileName(callerFilePath),
            SenderLocation = $"{sender.GetType().Name}.{callerMemberName}:{callerLineNumber}",
        });
    }
}
