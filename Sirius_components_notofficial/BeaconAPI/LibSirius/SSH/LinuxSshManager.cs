using Renci.SshNet;
using System.Net;

namespace LibSirius.SSH;

public class LinuxSshManager : AbstractSshManager
{
    public LinuxSshManager(string userName, string password, IPAddress ipAddress, uint port = 22)
        : base(userName, password, ipAddress, port)
    {
        SudoString = string.Format(SudoString, password);
    }

    private readonly string SudoString = "echo '{0}' | sudo -p ' ' -S ";


    // paths ======================================================================================================================================
    public override string RemoteSoftwareDirectory() => "/home/sirius/software";
    public override string RemoteBackupDirectory() => "/home/sirius/backup";


    // ======================================================================================================================================
    public override string RunCommand(string command, bool suppressExceptions = false)
    {
        command = command.Replace("sudo ", SudoString);
        return base.RunCommand(command, suppressExceptions);
    }

    public override bool IsGatewayRunning()
    {
        if (string.IsNullOrWhiteSpace(RunCommand("ps -awx | grep dotnet | grep -v grep")))
            return false;

        return true;
    }

    public override string StartGateway()
    {
        string command = "sudo dotnet /home/sirius/vrx/microgateway/MicroGateway.dll &";
        command = command.Replace("sudo ", SudoString);

        TimeSpan timeout = TimeSpan.FromSeconds(10);

        using ShellStream shellStream = sshClient.CreateShellStream("bash", 80, 80, 800, 600, int.MaxValue);
        //pulisci buffer
        shellStream.Flush();
        shellStream.ReadLine(timeout);

        //invia comando
        shellStream.WriteLine(command);
        //evita di stampare a schermo la riga appena scritta
        shellStream.ReadLine(timeout);
        shellStream.ReadLine(timeout);

        string outStr = "";

        while (true)
        {
            string result = shellStream.ReadLine(timeout);
            if (result == null)
                break;

            //Console.WriteLine(result);
            outStr += result + "\n";

            if (result.Contains("SYS  - System started"))
                break;
        }

        //leggi ancora una riga per rendere effettivo il detach
        shellStream.ReadLine(timeout);

        return outStr;
    }

    public override void KillGateway()
    {
        RunCommand("sudo killall -s SIGKILL -w dotnet");
    }

    public override string GetSoVersion()
    {
        string result = RunCommand("uname -a");

        if (result.Contains("Ubuntu"))
        {
            string processor = RunCommand("uname -m");
            string ubuntuVer = RunCommand("lsb_release -d | cut -f 2");
            result = ubuntuVer + " " + processor;
        }

        return result;
    }

    public override List<Version> DotnetVersions()
    {
        string versions = RunCommand("dotnet --list-runtimes | grep Microsoft.NETCore.App | cut -d ' ' -f 2", suppressExceptions: true);

        List<Version> result = new List<Version>();

        string[] lines = versions.Split("\n");
        foreach (string line in lines)
        {
            if (Version.TryParse(line, out Version? version))
                result.Add(version);
        }

        return result;
    }

    public override Task<string> ZipRemoteAsync(string remoteZipFile, List<string> filesToZip, List<string>? excludes = null)
    {
        string fileList = "";
        if (filesToZip.Count == 0)
            return Task.FromResult(fileList);

        string destinationPath = filesToZip[0].Substring(0, filesToZip[0].LastIndexOf('/'));

        foreach (string file in filesToZip)
        {
            string f = file;
            if (file.StartsWith(destinationPath))
                f = file.Replace(destinationPath, ".");

            fileList += $" {f} ";
        }

        string excludeList = "";
        if (excludes != null)
        {
            excludeList = "-x";
            foreach (string exclude in excludes)
                excludeList += " " + exclude;
        }

        return RunCommandAsync($"cd {destinationPath} && zip -1 -r -q {remoteZipFile} {fileList} {excludeList}");
    }

    public override string UnzipRemote(string remoteSourceFile, string remoteDestinationDir)
    {
        return RunCommand($"unzip -o {remoteSourceFile} -d {remoteDestinationDir}");
    }

    public override async Task<string> GetProcessListAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}/tasklist_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";
        await RunCommandAsync($"ps aux > {outputFile} ");
        return outputFile;
    }

    public async override Task<string> GetUsersListAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}/userlist_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";
        await RunCommandAsync($"getent passwd {{1000..5000}} > {outputFile} ");
        return outputFile;
    }

    public async override Task<string> GetNetworkInfoAsync(string remoteBackupPath)
    {
        string outputFile = $"{remoteBackupPath}/networkinfo_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm}.txt";

        await RunCommandAsync($"echo '=========== ip a ===========' > {outputFile} ");
        await RunCommandAsync($"ip a >> {outputFile} ");

        await RunCommandAsync($"echo '\n=========== route -n ===========' >> {outputFile} ");
        await RunCommandAsync($"route -n >> {outputFile} ");

        return outputFile;
    }
}
