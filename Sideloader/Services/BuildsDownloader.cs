using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
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
                if(report.IsSuccessful == false)
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

        private List<Build> ExtractBuildsFromHtml(string htmlText)
        {
            throw new NotImplementedException();
        }

        /* private List<Build> ExtractBuildsFromHtml(string htmlText)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlText);
            foreach (HtmlNode linkNode in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
            {
                Debug.WriteLine(linkNode.Name + " : " + linkNode.Attributes);
                var attr =
                    linkNode.Attributes.FirstOrDefault(a => a.Name == "href" && a.Value.Contains("SWA-"));
                if (attr != null)
                {
                    if (IsValidUri(attr.Value))
                    {
                        var urlDecode = WebUtility.HtmlDecode(attr.Value);
                        downloadLinks.Add(new AppPackage(urlDecode));
                        Debug.WriteLine(attr.Name + " : " + attr.Value);
                    }
                }
            }
        }*/

        private List<AppPackage> ExtractLinksFromHtml(string htmlText, string ticketNumber)
        {
            var downloadLinks = new List<AppPackage>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlText);
            foreach (HtmlNode linkNode in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
            {
                Debug.WriteLine(linkNode.Name + " : " + linkNode.Attributes);
                var attr =
                    linkNode.Attributes.FirstOrDefault(a => a.Name == "href" && a.Value.Contains("SWA-" + ticketNumber));
                if (attr != null)
                {
                    if (IsValidUri(attr.Value))
                    {
                        var urlDecode = WebUtility.HtmlDecode(attr.Value);
                        downloadLinks.Add(new AppPackage(urlDecode));
                        Debug.WriteLine(attr.Name + " : " + attr.Value);
                    }
                }
            }
            return downloadLinks;
        }

        private bool IsValidUri(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }


    }
}
