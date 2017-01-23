using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private List<Build> _allBuilds;
        private bool _isBusy;

        public MainViewModel()
        {
            AuthenticationViewModel = new AuthenticationViewModel(null, null);
            AuthenticationViewModel.UserIsLoggedIn = true;
            Builds = new ObservableCollection<Build>
            {
                new Build {Name = "Branch feature_SWA-1388_update_salesforce_connect_button"}
            };
        }

        public MainViewModel(ISettingsRepository settingsRepository, BuildsDownloader buildsDownloader)
        {
            _settingsRepository = settingsRepository;
            _buildsDownloader = buildsDownloader;
            AuthenticationViewModel = new AuthenticationViewModel(settingsRepository, new AuthenticationService());
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
                FilterBuilds();
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private void FilterBuilds()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                Builds = new ObservableCollection<Build>(_allBuilds);
            else
                Builds = new ObservableCollection<Build>(_allBuilds.Where(b => b.Name.ToLower().Contains(SearchText.ToLower())));
        }

        public async Task LoginUser()
        {
            var report = await AuthenticationViewModel.Login();
            if (report.IsSuccessful)
                await GetBuilds();
        }

        public async Task Initialize()
        {
            IsBusy = true;
            try
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
            catch (Exception)
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GetBuilds()
        {
            try
            {
                var builds = await _buildsDownloader.GetBuilds();
                Builds = new ObservableCollection<Build>(builds);
                _allBuilds = builds.ToList();
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