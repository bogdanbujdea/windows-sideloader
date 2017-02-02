using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Sideloader.Models;
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
        private readonly IPackageManager _packageManager;
        private string _statusMessage;

        public MainViewModel()
        {
            AuthenticationViewModel = new AuthenticationViewModel(null, null);
            AuthenticationViewModel.UserIsLoggedIn = true;
            Builds = new ObservableCollection<Build>
            {
                new Build {Name = "Branch feature_SWA-1388_update_salesforce_connect_button"}
            };
        }

        public MainViewModel(ISettingsRepository settingsRepository, BuildsDownloader buildsDownloader,
            IPackageManager packageManager)
        {
            _settingsRepository = settingsRepository;
            _buildsDownloader = buildsDownloader;
            _packageManager = packageManager;
            /*var builds = _buildsDownloader.ExtractBuildsFromHtml("");
            builds[0] = new Build
            {
                X86Build =
                    new AppPackage
                    {
                        Platform = new Platform(PlatformType.x86),
                        DownloadUrl =
                            "file:///C:/Users/bogda/Downloads/Showpad.Windows_1.5.5.0_x64_Test.zip"
                    },
                Name = "1.5"
            };
            Builds = new ObservableCollection<Build>(builds);*/
            AuthenticationViewModel = new AuthenticationViewModel(settingsRepository, new AuthenticationService());
            _packageManager.PackageStatusChanged += StatusChanged;
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

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private void FilterBuilds()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                Builds = new ObservableCollection<Build>(_allBuilds);
            else
                Builds =
                    new ObservableCollection<Build>(
                        _allBuilds.Where(b => b.Name.ToLower().Contains(SearchText.ToLower())));
        }

        public async Task LoginUser()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Logging in...";
                var report = await AuthenticationViewModel.Login();
                if (report.IsSuccessful)
                {
                    StatusMessage = "Retrieving builds...";
                    await GetBuilds();
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Error("Login", exception);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task Initialize()
        {
            IsBusy = true;
            try
            {
                StatusMessage = "Logging in...";
                var userName = _settingsRepository.GetValue(SettingsKey.Username);
                var password = _settingsRepository.GetValue(SettingsKey.Password);
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    AuthenticationViewModel.UserIsLoggedIn = false;
                }
                else
                {
                    AuthenticationViewModel.UserIsLoggedIn = true;
                    StatusMessage = "Retrieving builds...";
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

        public async Task DownloadPackage(AppPackage appPackage)
        {
            try
            {
                IsBusy = true;
                await _packageManager.RetrievePackage(appPackage);
            }
            catch (PackageException packageException)
            {
                if (packageException.IsHandled == false)
                {
                    StatusChanged(this, new StatusInfo(MessageType.Error, packageException.Message));
                    Logger.Instance.Error(packageException);
                }
            }
            catch (Exception exception)
            {
                StatusChanged(this, new StatusInfo(MessageType.Error, exception.Message));
                Logger.Instance.Error(exception);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StatusChanged(object sender, StatusInfo e)
        {
            if (e.MessageType == MessageType.Info)
            {
                StatusMessage = e.Message;
            }
            else if (e.MessageType == MessageType.Success)
            {
                MessageBox.Show("The application was successfully installed!");
            }
            else
            {
                StatusMessage = string.Empty;
                MessageBox.Show(e.Message);
                IsBusy = false;
            }
        }
    }
}