using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace LibSirius.Utils;

public static class DirectoryUtils
{
    public static void DirectoryCopy(this DirectoryInfo dir, string destDirName, bool copySubDirs)
    {
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + dir.FullName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir, tempPath, copySubDirs);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    public static bool HasWritePermission(string path)
    {
        return new DirectoryInfo(path).HasWritePermission();
    }

    [SupportedOSPlatform("windows")]
    public static bool HasWritePermission(this DirectoryInfo directoryInfo)
    {
        directoryInfo.Refresh();
        if (directoryInfo.Exists == false)
            return false;

        var writeAllow = false;
        var writeDeny = false;
        var accessControlList = directoryInfo.GetAccessControl();
        if (accessControlList == null)
            return false;
        var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
        if (accessRules == null)
            return false;

        foreach (FileSystemAccessRule? rule in accessRules)
        {
            if ((FileSystemRights.Write & rule?.FileSystemRights) != FileSystemRights.Write) continue;

            if (rule?.AccessControlType == AccessControlType.Allow)
                writeAllow = true;
            else if (rule?.AccessControlType == AccessControlType.Deny)
                writeDeny = true;
        }

        return writeAllow && !writeDeny;
    }

    [SupportedOSPlatform("windows")]
    public static bool HasFullPermission(string path)
    {
        return new DirectoryInfo(path).HasFullPermission();
    }

    [SupportedOSPlatform("windows")]
    public static bool HasFullPermission(this DirectoryInfo directoryInfo)
    {
        directoryInfo.Refresh();
        if (directoryInfo.Exists == false)
            return false;

        var writeAllow = false;
        var writeDeny = false;
        var accessControlList = directoryInfo.GetAccessControl();
        if (accessControlList == null)
            return false;
        var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
        if (accessRules == null)
            return false;

        foreach (FileSystemAccessRule? rule in accessRules)
        {
            if ((FileSystemRights.FullControl & rule?.FileSystemRights) != FileSystemRights.FullControl) continue;

            if (rule?.AccessControlType == AccessControlType.Allow)
                writeAllow = true;
            else if (rule?.AccessControlType == AccessControlType.Deny)
                writeDeny = true;
        }

        return writeAllow && !writeDeny;
    }

    public static void MoveAllContent(DirectoryInfo source, DirectoryInfo destination, params string[] excludes)
    {
        foreach (DirectoryInfo info in source.EnumerateDirectories())
            info.MoveTo(Path.Combine(destination.FullName, info.Name));

        foreach (FileInfo info in source.EnumerateFiles())
        {
            if (excludes.Contains(info.Name) == false)
                info.MoveTo(Path.Combine(destination.FullName, info.Name));
        }
    }
}
