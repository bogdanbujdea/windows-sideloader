using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            if (WebClient.HttpClient != null)
            {
                var packagesResponse = await WebClient.HttpClient.GetAsync("http://builds.showpad.com/showpad/showpad/windows?allBranches=1&download=1");
                var html = await packagesResponse.Content.ReadAsStringAsync();
                if (html.Contains("action=\"login.php\""))
                    throw new UnauthorizedAccessException();
            }
            return new List<Build>();
        }
    }
}
