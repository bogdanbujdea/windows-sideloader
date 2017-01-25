using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sideloader.Models;
using Sideloader.Services;
using Sideloader.Settings;
using Sideloader.ViewModels;

namespace Sideloader.Views
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(SettingsRepository.Instance, new BuildsDownloader(new AuthenticationService()), new PackageManager());
            Loaded += ViewLoaded;
        }

        private async void ViewLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.Initialize();
        }

        public MainViewModel ViewModel => DataContext as MainViewModel;

        private async void Login(object sender, RoutedEventArgs e)
        {
            ViewModel.AuthenticationViewModel.Password = Password.Password;
            await ViewModel.LoginUser();
        }

        private async void DownloadArmPackage(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var build = textBlock?.DataContext as Build;
            await ViewModel.DownloadPackage(build?.ARMBuild);
        }

        private async void Downloadx86Package(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var build = textBlock?.DataContext as Build;
            await ViewModel.DownloadPackage(build?.X86Build);
        }

        private async void Downloadx64Package(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var build = textBlock?.DataContext as Build;
            await ViewModel.DownloadPackage(build?.X64Build);
        }
    }
}