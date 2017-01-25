using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sideloader.Models;
using Sideloader.Settings;
using Sideloader.ViewModels;

namespace Sideloader.Services
{
    public class BuildsDownloader : IBuildsDownloader
    {
        private readonly IAuthenticationService _authenticationService;

        public BuildsDownloader(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<List<Build>> GetBuilds()
        {
            if (WebClient.HttpClient == null)
            {
                var report = await
                    _authenticationService.LoginAsync(SettingsRepository.Instance.GetValue(SettingsKey.Username), SettingsRepository.Instance.GetValue(SettingsKey.Password));
                if (report.IsSuccessful == false)
                    throw new UnauthorizedAccessException();
            }
            string htmlText = string.Empty;

            if (WebClient.HttpClient != null)
            {
                var packagesResponse = await WebClient.HttpClient.GetAsync("http://builds.showpad.com/showpad/showpad/windows?allBranches=1&download=1");
                if (packagesResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception(packagesResponse.StatusCode.ToString());
                }
                htmlText = await packagesResponse.Content.ReadAsStringAsync();
                if (htmlText.Contains("action=\"login.php\""))
                    throw new UnauthorizedAccessException();
            }
            return ExtractBuildsFromHtml(htmlText);
        }

        public List<Build> ExtractBuildsFromHtml(string htmlText)
        {
            if (string.IsNullOrWhiteSpace(htmlText))
                htmlText = File.ReadAllText("builds.html");
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlText);
            var builds = new List<Build>();
            var packagesList = new Dictionary<string, List<AppPackage>>();
            var htmlNodeCollection = htmlDocument.DocumentNode.Descendants("h4").ToList();
            var orderedLists = htmlDocument.DocumentNode.Descendants("ol").ToList();
            for (int index = 0; index < orderedLists.Count; index++)
            {
                packagesList[htmlNodeCollection[index].InnerText] = new List<AppPackage>();
                var list = orderedLists[index];
                var links = list.Descendants("a").ToList();
                foreach (var node in links)
                {
                    Debug.WriteLine(node.Name + " : " + node.Attributes);
                    if (IsValidUri(node.Attributes[0].Value))
                    {
                        var decodedUrl = WebUtility.HtmlDecode(node.Attributes[0].Value);
                        var appPackage = new AppPackage() { DownloadUrl = decodedUrl };
                        if (node.InnerText.Contains("Download (RT)"))
                        {
                            appPackage.Platform = new Platform(PlatformType.ARM);
                        }
                        else if (node.InnerText.Contains("Download (x86)"))
                        {
                            appPackage.Platform = new Platform(PlatformType.x86);
                        }
                        else if (node.InnerText.Contains("Download (x64)"))
                        {
                            appPackage.Platform = new Platform(PlatformType.x64);
                        }
                        packagesList[htmlNodeCollection[index].InnerText].Add(appPackage);
                    }
                }
            }
            foreach (var package in packagesList)
            {
                var build = new Build { Name = package.Key };
                foreach (var appPackage in package.Value)
                {
                    switch (appPackage.Platform.PlatformType)
                    {
                        case PlatformType.ARM:
                            build.ARMBuild = appPackage;
                            break;
                        case PlatformType.x86:
                            build.X86Build = appPackage;
                            break;
                        case PlatformType.x64:
                            build.X64Build = appPackage;
                            break;
                    }
                }
                builds.Add(build);
            }
            return builds;
        }

        private bool IsValidUri(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}