using System.Windows;
using Sideloader.Settings;
using Sideloader.ViewModels;

namespace Sideloader.Views
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(SettingsRepository.Instance);
            Loaded += ViewLoaded;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Initialize();
        }

        public MainViewModel ViewModel => DataContext as MainViewModel;

        private async void Login(object sender, RoutedEventArgs e)
        {
            ViewModel.AuthenticationViewModel.Password = Password.Password;
            await ViewModel.LoginUser();
        }
    }
}