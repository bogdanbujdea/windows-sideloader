using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sideloader.Services;
using Sideloader.Settings;

namespace Sideloader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly BuildsDownloader _buildsDownloader;
        private ObservableCollection<Build> _builds;
        private string _searchText;

        public MainViewModel()
        {
            AuthenticationViewModel = new AuthenticationViewModel(null, null);
            AuthenticationViewModel.UserIsLoggedIn = true;
        }

        public MainViewModel(ISettingsRepository settingsRepository, BuildsDownloader buildsDownloader)
        {
            _settingsRepository = settingsRepository;
            _buildsDownloader = buildsDownloader;
            AuthenticationViewModel = new AuthenticationViewModel(settingsRepository, new AuthenticationService());
            Builds = new ObservableCollection<Build>
            {
                new Build {Name = "Branch feature_SWA-1388_update_salesforce_connect_button"}
            };
        }

        public AuthenticationViewModel AuthenticationViewModel { get; set; }

        public ObservableCollection<Build> Builds
        {
            get { return _builds; }
            set
            {
                _builds = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                if (_searchText.Length > 2)
                {
                    FilterBuilds();
                }
                OnPropertyChanged();
            }
        }

        private void FilterBuilds()
        {

        }

        public async Task LoginUser()
        {
            var report = await AuthenticationViewModel.Login();
            if (report.IsSuccessful)
                await GetBuilds();
        }

        public async Task Initialize()
        {
            var userName = _settingsRepository.GetValue(SettingsKey.Username);
            var password = _settingsRepository.GetValue(SettingsKey.Password);
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                AuthenticationViewModel.UserIsLoggedIn = false;
            }
            else
            {
                AuthenticationViewModel.UserIsLoggedIn = true;
                await GetBuilds();
            }
        }

        private async Task GetBuilds()
        {
            try
            {
                var builds = await _buildsDownloader.GetBuilds();
            }
            catch (UnauthorizedAccessException accessException)
            {
                AuthenticationViewModel.UserIsLoggedIn = false;
                Logger.Instance.Error("GetBuilds accessException", accessException);
            }
            catch (Exception exception)
            {
                Logger.Instance.Error("GetBuilds exception", exception);
            }
        }
    }
}