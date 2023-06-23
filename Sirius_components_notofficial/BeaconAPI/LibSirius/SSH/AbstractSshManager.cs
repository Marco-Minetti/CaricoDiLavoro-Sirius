using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Net;
using System.Text.RegularExpressions;
using SshNetException = Renci.SshNet.Common.SshException;

namespace LibSirius.SSH;

public abstract class AbstractSshManager
{
    public bool IsConnected => IsAllConnected();

    public Dictionary<string, ForwardedPortLocal> ForwardedPorts = new Dictionary<string, ForwardedPortLocal>();

    protected readonly SshClient sshClient;
    protected readonly SftpClient sftpClient;
    private readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    private readonly string Username = "";
    private readonly IPAddress IPAddress = IPAddress.Loopback;
    private readonly int Port = 22;

    protected AbstractSshManager(string userName, string password, IPAddress ipAddress, uint port = 22)
    {
        Username = userName;
        IPAddress = ipAddress;
        Port = (int)port;

        // utilizzo di eventuali private key
        PrivateKeyFile[] keys = Array.Empty<PrivateKeyFile>();
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.pub");
            keys = new PrivateKeyFile[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                try 
                {
                    string privateKeyName = files[i].Replace(".pub", "");
                    keys[i] = new PrivateKeyFile(privateKeyName);
                } catch (SshNetException sshNetException) 
                {
                    if (sshNetException.Message == "openssh key type: ssh-rsa is not supported")
                        continue;
                    else
                        throw sshNetException;
                } 
            }
        }

        if (!keys.All(key => key != null)) /* if all the keys are not initialized */
            keys = keys.Where(key => key != null).ToArray(); /* take only the initialized keys */

        // parametri di connessione
        ConnectionInfo connectionInfo = new ConnectionInfo(ipAddress.ToString(), Port, userName,
            new PrivateKeyAuthenticationMethod(userName, keys),
            new PasswordAuthenticationMethod(userName, password));
        connectionInfo.Timeout = Timeout;
        connectionInfo.ChannelCloseTimeout = Timeout;

        // client
        sshClient = new SshClient(connectionInfo);
        sftpClient = new SftpClient(connectionInfo);
    }

    // paths ======================================================================================================================================
    public virtual string SevenZipExe() => @"C:\Program Files\7-Zip\7z.exe";
    public abstract string RemoteSoftwareDirectory();
    public abstract string RemoteBackupDirectory();


    // publics ======================================================================================================================================
    public async Task<string> RunCommandAsync(string command, bool suppressExceptions = false)
    {
        return await Task.Run(() =>
            RunCommand(command, suppressExceptions)
        );
    }

    public uint GetTunneledPort(IPAddress toReachAddress, uint toBeTunneledPort)
    {
        string key = $"{toReachAddress}:{toBeTunneledPort}";

        if (ForwardedPorts.TryGetValue(key, out ForwardedPortLocal? forwardedPortLocal) == false)
        {
            forwardedPortLocal = new ForwardedPortLocal("127.0.0.1", toReachAddress.ToString(), toBeTunneledPort);

            ForwardedPorts.Add(key, forwardedPortLocal);
            sshClient.AddForwardedPort(forwardedPortLocal);

            forwardedPortLocal.Start();
        }

        return forwardedPortLocal.BoundPort;
    }

    public override string ToString()
    {
        string result = $"{Username}@{IPAddress}:{Port}";

        if (ForwardedPorts.Count > 0)
            result += $"~tun:{ForwardedPorts.Count}";

        return result;
    }

    // virtuals ======================================================================================================================================
    public virtual bool Connect()
    {
        sshClient.Connect();
        sftpClient.Connect();

        return IsConnected;
    }

    public virtual bool Disconnect()
    {
        sshClient.Disconnect();
        sftpClient.Disconnect();

        return IsConnected;
    }

    public virtual string RunCommand(string command, bool suppressExceptions = false)
    {
        SshCommand sshCommand = sshClient.RunCommand(command);
        string result = "";
        string error = sshCommand.Error;
        if (string.IsNullOrWhiteSpace(error) == false)
        {
            error = error.Substring(0, error.Length - 1); //  Remove \n character returned by command
            if (suppressExceptions)
                result = error + "\n";
            else
                throw new SshException(error);
        }

        result += sshCommand.Result;
        if (result.Length > 0)
            result = result.Substring(0, result.Length - 1); // Remove \n character returned by command
        return result;
    }

    public IEnumerable<SftpFile> ListFiles(string remotePath, string fileFilter, int count = int.MaxValue, bool includeSubDirectories = false)
    {
        List<SftpFile> remoteFiles = new List<SftpFile>();
        RecursiveListDirectory(remotePath, fileFilter, includeSubDirectories, ref remoteFiles);

        IEnumerable<SftpFile> filteredRemoteFiles = remoteFiles
                        .OrderByDescending(f => f.LastWriteTime)
                        .Take(count);

        return filteredRemoteFiles;
    }

    public Task<FileSystemInfo> DownloadFileAsync(SftpFile remoteFile, DirectoryInfo destination)
    {
        return Task.Run(() => DownloadFile(remoteFile, destination));
    }

    public FileSystemInfo DownloadFile(SftpFile remoteFile, DirectoryInfo destination)
    {
        FileSystemInfo result;
        string remotePath = remoteFile.FullName.Replace(remoteFile.Name, "");
        string destinationFileName = Path.Combine(destination.FullName, remoteFile.FullName.Replace(remotePath, "").TrimStart('/'));

        if (remoteFile.IsDirectory)
        {
            result = Directory.CreateDirectory(destinationFileName);
        }
        else
        {
            using FileStream fileStream = new FileStream(destinationFileName, FileMode.Create);
            sftpClient.DownloadFile(remoteFile.FullName, fileStream);
            result = new FileInfo(destinationFileName);
        }

        return result;
    }

    public List<FileInfo> DownloadFiles(string remotePath, DirectoryInfo destination, string fileFilter = "", int count = int.MaxValue, bool includeSubDirectories = false)
    {
        IEnumerable<SftpFile> filteredRemoteFiles = ListFiles(remotePath, fileFilter, count, includeSubDirectories);

        List<FileInfo> result = new List<FileInfo>();

        foreach (SftpFile remoteFile in filteredRemoteFiles)
        {
            FileSystemInfo downloaded = DownloadFile(remoteFile, destination);
            if (downloaded is FileInfo fi)
                result.Add(fi);
        }

        return result;
    }

    public virtual void UploadFile(FileInfo toUpload, string remoteFilePath)
    {
        toUpload.Refresh();
        if (toUpload.Exists == false)
            return;

        using FileStream stream = toUpload.OpenRead();
        sftpClient.UploadFile(stream, remoteFilePath, canOverride: true);
    }

    public virtual void CreateDirectory(string remotePath)
    {
        if (sftpClient.Exists(remotePath) == false)
            sftpClient.CreateDirectory(remotePath);
    }

    public virtual void DeleteFile(string remoteFilePath)
    {
        if (sftpClient.Exists(remoteFilePath))
            sftpClient.DeleteFile(remoteFilePath);
    }

    public virtual void DeleteDirectory(string remoteDirectoryPath)
    {
        if (sftpClient.Exists(remoteDirectoryPath) == false)
            return;

        foreach (SftpFile file in sftpClient.ListDirectory(remoteDirectoryPath))
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsDirectory)
                    DeleteDirectory(file.FullName); //recursive call
                else
                    sftpClient.DeleteFile(file.FullName);
            }
        }

        sftpClient.DeleteDirectory(remoteDirectoryPath);
    }

    public virtual bool ExistsFile(string remoteFilePath)
    {
        return sftpClient.Exists(remoteFilePath);
    }

    public virtual Task<string> GetHostnameAsync()
    {
        return RunCommandAsync("hostname");
    }

    // abstracts ======================================================================================================================================
    public abstract bool IsGatewayRunning();

    public abstract string StartGateway();

    public abstract void KillGateway();

    public abstract string GetSoVersion();

    public abstract List<Version> DotnetVersions();

    public Task<string> ZipRemoteAsync(string remoteZipFile, string fileToZip, List<string>? excludes = null)
    {
        return ZipRemoteAsync(remoteZipFile, new List<string> { fileToZip }, excludes);
    }

    public abstract Task<string> ZipRemoteAsync(string remoteZipFile, List<string> filesToZip, List<string>? excludes = null);

    public abstract string UnzipRemote(string remoteSourceFile, string remoteDestinationDir);

    public abstract Task<string> GetProcessListAsync(string remoteBackupPath);

    public abstract Task<string> GetUsersListAsync(string remoteBackupPath);

    public abstract Task<string> GetNetworkInfoAsync(string remoteBackupPath);


    // privates ======================================================================================================================================
    private bool IsAllConnected()
    {
        return sshClient.IsConnected && sftpClient.IsConnected;
    }

    private void RecursiveListDirectory(string remotePath, string fileFilter, bool includeSubDirectories, ref List<SftpFile> remoteFiles)
    {
        foreach (SftpFile entry in sftpClient.ListDirectory(remotePath))
        {
            if (entry.IsDirectory)
            {
                if (entry.Name != "." && entry.Name != ".." && includeSubDirectories)
                {
                    remoteFiles.Add(entry);
                    RecursiveListDirectory(entry.FullName, fileFilter, includeSubDirectories, ref remoteFiles);
                }
            }
            else
            {
                if (Regex.Match(entry.Name, fileFilter).Success)
                    remoteFiles.Add(entry);
            }
        }
    }

    /// <summary>
    /// Converte un path tipo C:\VireoX\Gateway\cfg nel formato SFTP: /C:/VireoX/Gateway/cfg
    /// </summary>
    /// <param name="windowsPath"></param>
    /// <returns></returns>
    public static string ConvertWindowsPathToSftpPath(string windowsPath)
    {
        string sftpPath = windowsPath;

        if (sftpPath.StartsWith("/") == false)
            sftpPath = '/' + sftpPath;

        sftpPath = sftpPath.Replace(@"\", "/");

        return sftpPath;
    }

    /// <summary>
    /// Converte un path dal formato SFTP /C:/VireoX/Gateway/cfg ad uno standard Windows C:\VireoX\Gateway\cfg
    /// </summary>
    /// <param name="sftpPath"></param>
    /// <returns></returns>
    public static string ConvertSftpPathToWindowsPath(string sftpPath)
    {
        string windowsPath = sftpPath;

        if (sftpPath.StartsWith("/"))
            windowsPath = sftpPath.Substring(1);

        windowsPath = windowsPath.Replace("/", @"\");

        return windowsPath;
    }
}
