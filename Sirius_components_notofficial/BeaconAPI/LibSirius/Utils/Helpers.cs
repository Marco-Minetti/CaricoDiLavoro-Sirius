using System.Text.RegularExpressions;

namespace LibSirius.Utils;

public static class Helpers
{
    public static bool GetStringByRegex(string inputString, Regex regex, out string result)
    {
        result = inputString;

        Match match = regex.Match(inputString);

        if (match.Success)
        {
            if (match.Groups.Count > 1)
            {
                Group group = match.Groups[1];
                result = group.Value;
                return true;
            }
        }

        return false;
    }

    public static bool GetStringByRegex(string inputString, string regexString, out string result)
    {
        Regex regex = new Regex(regexString);
        return GetStringByRegex(inputString, regex, out result);
    }

    public static bool IsFileLocked(string inputFileFullPath)
    {
        return IsFileLocked(new FileInfo(inputFileFullPath));
    }

    public static bool IsFileLocked(FileInfo inputFile)
    {
        try
        {
            using FileStream _ = inputFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        catch
        {
            return true;
        }

        return false;
    }

    /*
    * https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
    * The Thread Pool Hack:
    *   The call to Task.Run executes the asynchronous method on a thread pool thread. 
    *   Here it will run without a context, thus avoiding the deadlock. 
    *   One of the problems with this approach is the asynchronous method can’t depend on executing within a specific context. 
    *   So, it can’t use UI elements or the ASP.NET HttpContext.Current.
    */
    public static void WaitNoDeadLock(Func<Task> toWait)
    {
        Task.Run(() =>
            toWait()
        )
        .GetAwaiter()
        .GetResult();
    }

    /*
    public static void Start([CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null)

    {
        if (callerFilePath != null)
            callerFilePath = Path.GetFileNameWithoutExtension(callerFilePath);

        Debug.WriteLine("▛ {0}.{1} START", callerFilePath, callerMemberName);
    }

    public static void Stop([CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null)
    {
        if (callerFilePath != null)
            callerFilePath = Path.GetFileNameWithoutExtension(callerFilePath);

        Debug.WriteLine("▙ {0}.{1} STOP", callerFilePath, callerMemberName);
    }

    public static void Print(string message, [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null)
    {
        if (callerFilePath != null)
            callerFilePath = Path.GetFileNameWithoutExtension(callerFilePath);

        Debug.WriteLine("{0}.{1}: {2}", callerFilePath, callerMemberName, message);
    }

    public static void PrintEx(Exception e, [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null)
    {
        if (callerFilePath != null)
            callerFilePath = Path.GetFileNameWithoutExtension(callerFilePath);

        Debug.WriteLine("EX {0}.{1}: {2}\n{3}", callerFilePath, callerMemberName, e.Message, e.StackTrace);
    }
    */    

    /// <summary>
    /// Sanitizes only filenames! Not paths!
    /// </summary>
    /// <param name="fileNameWithoutPath"></param>
    /// <returns></returns>
    public static string SanitizeFileName(string fileNameWithoutPath)
    {
        char[] invalids = Path.GetInvalidFileNameChars();

        string sanitizedPath = string.Join("_", fileNameWithoutPath.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).Trim();

        return sanitizedPath;
    }
}
