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
            var downloadUrl = appPackage.DownloadUrl;
            var webClient = new System.Net.WebClient();
            var directoryName = "package" + appPackage.Platform.Name;
            try
            {
                File.Delete("package.zip");
                Directory.Delete(directoryName, true);
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
            }
            await webClient.DownloadFileTaskAsync(new Uri(downloadUrl), "package.zip");
            ZipFile.ExtractToDirectory("package.zip", directoryName);
            appPackage.InstallFolderPath = GetFolderPath(directoryName);
            InstallDependencies(appPackage);
            InstallPackage(appPackage);
        }

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
        }

        private string GetDependency(AppPackage package)
        {
            var path = package.InstallFolderPath + @"\\Dependencies\\" + package.Platform.Name;
            var fileNames = Directory.GetFiles(path);
            return Path.GetFileName(fileNames[0]);
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
                    var process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe";
                    process.StartInfo.Arguments = "\"&'" + strCmdText + "'\"";
                    process.StartInfo.WorkingDirectory = package.InstallFolderPath;

                    process.Start();
                    string s = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    using (StreamWriter outfile = new StreamWriter("StandardOutput.txt", true))
                    {
                        outfile.Write(s);
                    }

                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }
}