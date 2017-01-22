using System.Threading.Tasks;
using Sideloader.Services;
using Sideloader.Settings;

namespace Sideloader.ViewModels
{
    public class AuthenticationViewModel: ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IAuthenticationService _authenticationService;
        private string _username;
        private bool _userIsLoggedIn;

        public AuthenticationViewModel(ISettingsRepository settingsRepository, IAuthenticationService authenticationService)
        {
            _settingsRepository = settingsRepository;
            _authenticationService = authenticationService;
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password { get; set; }

        public bool UserIsLoggedIn
        {
            get { return _userIsLoggedIn; }
            set
            {
                _userIsLoggedIn = value; 
                OnPropertyChanged();
            }
        }

        public async Task<Report> Login()
        {
            var report = await _authenticationService.LoginAsync(Username, Password);
            UserIsLoggedIn = report.IsSuccessful;
            if (report.IsSuccessful)
            {
                _settingsRepository.SetValue(SettingsKey.Username, Username);
                _settingsRepository.SetValue(SettingsKey.Password, Password);
            }
            return report;
        }
    }
}