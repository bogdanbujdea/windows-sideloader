using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Sideloader.Settings;
using Sideloader.ViewModels;

namespace Sideloader.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private Mock<ISettingsRepository> _settingsMock;
        private MainViewModel _viewModel;

        [SetUp]
        public void SetupTests()
        {
            _settingsMock = new Mock<ISettingsRepository>();
            _viewModel = new MainViewModel(_settingsMock.Object, null, null);
        }

        [Test]
        public async Task unauthorized_user_should_see_login_prompt()
        {
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Username))).Returns((string)null);
            _viewModel.AuthenticationViewModel.UserIsLoggedIn = true;
            await _viewModel.Initialize();
            _viewModel.AuthenticationViewModel.UserIsLoggedIn.Should().Be(false);
        }

        [Test]
        public async Task authorized_user_should_see_list_of_tickets()
        {
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Username))).Returns("test");
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Password))).Returns("test");
            await _viewModel.Initialize();
            _viewModel.AuthenticationViewModel.UserIsLoggedIn.Should().Be(true);
        }        
    }
}