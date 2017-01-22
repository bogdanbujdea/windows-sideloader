using System.Threading.Tasks;
using Sideloader.Services;
using Sideloader.Settings;

namespace Sideloader.ViewModels
{
    public class MainViewModel: ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepository;

        public MainViewModel()
        {
            AuthenticationViewModel = new AuthenticationViewModel(null, null);
            AuthenticationViewModel.UserIsLoggedIn = true;
        }

        public MainViewModel(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            AuthenticationViewModel = new AuthenticationViewModel(settingsRepository, new AuthenticationService());
        }

        public AuthenticationViewModel AuthenticationViewModel { get; set; }

        public async Task LoginUser()
        {
            await AuthenticationViewModel.Login();
        }

        public void Initialize()
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
            }
        }
    }
}