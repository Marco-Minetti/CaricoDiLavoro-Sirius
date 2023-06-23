using System.Net;

namespace LibSirius.SSH;

public class WindowsSshManager : AbstractSshManager
{
    public WindowsSshManager(string userName, string password, IPAddress ipAddress, uint port = 22)
        : base(userName, password, ipAddress, port)
    {
    }

    // paths ======================================================================================================================================
    public override string RemoteSoftwareDirectory() => @"C:\VireoX\Software";
    public override string RemoteBackupDirectory() => @"C:\VireoX\Backup";


    // ======================================================================================================================================
    public override bool IsGatewayRunning()
    {
        if (string.IsNullOrWhiteSpace(RunCommand("tasklist | findstr /I \"VireoXgateway.exe\"")))
            return false;

        return true;
    }

    public override void KillGateway()
    {
        RunCommand("taskkill /f /im \"VireoXgateway.exe\"");
    }

    public override string StartGateway()
    {
        (string taskName, string _) = GetTask();

        if (string.IsNullOrWhiteSpace(taskName) == false)
            return RunCommand($"schtasks /run /tn \"{taskName}\"");

        return "ER: Can't find Scheduled Task named \"Start VireoXGateway\" to start Gateway";
    }

    public override string GetSoVersion()
    {
        string version = RunCommand("wmic os get Caption | findstr /v /i Caption").Trim();
        string arch = RunCommand("wmic os get OSArchitecture | findstr /v /i OSArchitecture").Trim();

        return version + " " + arch;
    }

    public override List<Version> DotnetVersions()
    {
        string versions = RunCommand("dotnet --list-runtimes | findstr Microsoft.WindowsDesktop.App");

        List<Version> result = new List<Version>();

        string[] lines = versions.Split("\r\n");
        foreach (string line in lines)
        {
            string[] fields = line.Split(' ');
            if (fields.Length > 1 && Version.TryParse(fields[1].Trim(), out Version? version))
                result.Add(version);
        }

        return result;
    }

    public (string taskName, string state) GetTask()
    {
        string taskLine = RunCommand("schtasks /query /fo CSV | findstr /i vireoxgateway");
        var fields = taskLine.Split(',');
        if (fields.Length < 3)
            return ("", "Disabled");

        return (fields[0].Trim('"'), fields[2].Trim('"'));
    }

    public override Task<string> ZipRemoteAsync(string remoteZipFile, List<string> filesToZip, List<string>? excludes = null)
    {
        string fileList = "";
        if (filesToZip.Count == 0)
            return Task.FromResult(fileList);

        foreach (string file in filesToZip)
            fileList += $" \"{ConvertSftpPathToWindowsPath(file)}\" ";

        remoteZipFile = ConvertSftpPathToWindowsPath(remoteZipFile);

        string excludeList = "";
        if (excludes != null)
        {
            foreach (string exclude in excludes)
                excludeList += " -xr!" + exclude;
        }

        return RunCommandAsync($"\"{SevenZipExe()}\" a \"{remoteZipFile}\" {fileList} {excludeList}");
    }

    public override string UnzipRemote(string remoteSourceFile, string remoteDestinationDir)
    {
        string winSourceFile = ConvertSftpPathToWindowsPath(remoteSourceFile);
        string winDestinationDir = ConvertSftpPathToWindowsPath(remoteDestinationDir);

        return RunCommand($"\"{SevenZipExe()}\" x \"{winSourceFile}\" -y -o\"{winDestinationDir}\""); //NON ci vuole lo spazio dopo il -o
    }

    public async override Task<string> GetProcessListAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}\\tasklist_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";
        await RunCommandAsync($"tasklist > \"{outputFile}\" ");
        return outputFile;
    }

    public async override Task<string> GetUsersListAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}\\userlist_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";
        await RunCommandAsync($"net user > \"{outputFile}\" ");
        return outputFile;
    }

    public async override Task<string> GetNetworkInfoAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}\\networkinfo_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";

        await RunCommandAsync($"echo '=========== ipconfig ===========' > {outputFile} ");
        await RunCommandAsync($"ipconfig >> {outputFile} ");

        await RunCommandAsync($"echo '\n\r=========== route print ===========' >> {outputFile} ");
        await RunCommandAsync($"route print >> {outputFile} ");

        return outputFile;
    }

}

