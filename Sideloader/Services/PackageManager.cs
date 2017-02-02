using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Sideloader.Models;
using Sideloader.Settings;

namespace Sideloader.Services
{
    public class PackageManager : IPackageManager
    {
        public async Task RetrievePackage(AppPackage appPackage)
        {
            Cleanup("package" + appPackage.Platform.Name);
            OnPackageStatusChanged("Downloading package...");
            var directoryName = await DownloadFile(appPackage);
            OnPackageStatusChanged("Extracting zip content...");
            ExtractZip(directoryName);
            appPackage.InstallFolderPath = GetFolderPath(directoryName);
            OnPackageStatusChanged("Installing dependencies...");
            InstallDependencies(appPackage);
            OnPackageStatusChanged("Installing package...");
            InstallPackage(appPackage);
            OnPackageStatusChanged("", MessageType.Success);
        }

        private void ExtractZip(string directoryName)
        {
            try
            {
                ZipFile.ExtractToDirectory("package.zip", directoryName);
            }
            catch (Exception exception)
            {
                OnPackageStatusChanged("Unzip failed. " + exception.Message, MessageType.Error);
                throw new PackageException(exception.ToString(), true);
            }
        }

        private async Task<string> DownloadFile(AppPackage appPackage)
        {
            var webClient = new System.Net.WebClient();
            var directoryName = "package" + appPackage.Platform.Name;
            try
            {
                await webClient.DownloadFileTaskAsync(new Uri(appPackage.DownloadUrl), "package.zip");
            }
            catch (Exception exception)
            {
                // The remote server returned an error: (403) Forbidden
                OnPackageStatusChanged("Package download failed. DevInfo: " + exception.Message, MessageType.Error);
                throw new PackageException(exception.ToString(), true);
            }
            return directoryName;
        }

        private void Cleanup(string directoryName)
        {
            try
            {
                File.Delete("package.zip");
                if (Directory.Exists(directoryName))
                    Directory.Delete(directoryName, true);
            }
            catch (Exception exception)
            {
                OnPackageStatusChanged("Cleanup failed. " + exception.Message, MessageType.Error);
                throw new PackageException(exception.ToString(), true);
            }
        }

        public event EventHandler<StatusInfo> PackageStatusChanged;

        private string GetFolderPath(string path)
        {
            var outputDirectory = Directory.GetDirectories(path).FirstOrDefault();
            if (outputDirectory != null)
            {
                var buildDirectory = new DirectoryInfo(outputDirectory).GetDirectories().FirstOrDefault();
                if (buildDirectory != null)
                    return buildDirectory.FullName;
            }
            return string.Empty;
        }

        private void InstallDependencies(AppPackage package)
        {
            var process = new Process
            {
                StartInfo =
                {
                    RedirectStandardOutput = true,
                    FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe",
                    UseShellExecute = false
                }
            };
            process.StartInfo.Arguments = "Add-AppxPackage .\\" + GetDependency(package);
            process.Start();
            string status = process.StandardOutput.ReadToEnd();
            Debug.WriteLine(status);
            process.WaitForExit();
            if (string.IsNullOrWhiteSpace(status) == false)
            {
                OnPackageStatusChanged("Can't install dependency. " + status, MessageType.Error);
                throw new PackageException("Can't install dependency. " + status, true);
            }
        }

        private string GetDependency(AppPackage package)
        {
            try
            {
                var path = package.InstallFolderPath + @"\\Dependencies\\" + package.Platform.Name;
                var fileNames = Directory.GetFiles(path);
                var dependencyPath = ".\\packagex86\\" + Path.Combine(path, Path.GetFileName(fileNames[0])).Split(new[] { "\\packagex86" }, StringSplitOptions.None)[1];
                return dependencyPath;
            }
            catch (Exception exception)
            {
                OnPackageStatusChanged("Can't resolve dependency. " + exception.Message, MessageType.Error);
                Logger.Instance.Error("Dependency. " + exception);
                throw new PackageException("Can't resolve dependency. " + exception.Message, true);
            }
        }

        private void InstallPackage(AppPackage package)
        {
            try
            {
                var path = package.InstallFolderPath + @"\\Add-AppDevPackage.ps1";
                if (File.Exists(path))
                {
                    File.GetAttributes(path);
                    string strCmdText = Path.Combine(package.InstallFolderPath, "Add-AppDevPackage.ps1");
                    var process = new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe",
                            Arguments = "\"&'" + strCmdText + "'\"",
                            WorkingDirectory = package.InstallFolderPath
                        }
                    };

                    process.Start();
                    string installInfo = process.StandardOutput.ReadToEnd();
                    Logger.Instance.Info(installInfo);
                    process.WaitForExit();
                    if (installInfo.Contains("Error: "))
                    {
                        var message = installInfo;
                        if (installInfo.Contains("The current user has already installed"))
                        {
                            message = "Please uninstall the application first";
                        }
                        else if (installInfo.Contains("running scripts is disabled"))
                        {
                            message =
                                "Please allow powershell scripts to be executed on your machine before using this app."
                                + Environment.NewLine +
                                "Open Powershall as an administrator, and run this command: Set-ExecutionPolicy -Scope CurrentUser Unrestricted";
                        }
                        OnPackageStatusChanged(message, MessageType.Error);
                        throw new PackageException("Package installation failed", true);
                    }
                    using (StreamWriter outfile = new StreamWriter("install_log.txt", true))
                    {
                        outfile.Write(installInfo);
                    }

                }
            }
            catch (Exception exception)
            {
                OnPackageStatusChanged("Package installation failed. " + exception.Message, MessageType.Error);
                Logger.Instance.Error("Package instllation: ", exception);
                throw new PackageException("Package installation failed", true);
            }
        }

        protected virtual void OnPackageStatusChanged(string message, MessageType messageType = MessageType.Info)
        {
            Logger.Instance.Info("STATUS: " + message);
            var messageInfo = new StatusInfo(messageType, message);
            PackageStatusChanged?.Invoke(this, messageInfo);
        }
    }

    public class StatusInfo
    {
        public MessageType MessageType { get; }
        public string Message { get; }

        public StatusInfo(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }

    public enum MessageType
    {
        Info,
        Error,
        Success
    }
}