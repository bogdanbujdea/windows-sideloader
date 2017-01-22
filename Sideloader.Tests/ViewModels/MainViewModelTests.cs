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
            _viewModel = new MainViewModel(_settingsMock.Object);
        }

        [Test]
        public void unauthorized_user_should_see_login_prompt()
        {
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Username))).Returns((string)null);
            _viewModel.AuthenticationViewModel.UserIsLoggedIn = true;
            _viewModel.Initialize();
            _viewModel.AuthenticationViewModel.UserIsLoggedIn.Should().Be(false);
        }

        [Test]
        public void authorized_user_should_see_list_of_tickets()
        {
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Username))).Returns("test");
            _settingsMock.Setup(sr => sr.GetValue(It.Is<SettingsKey>(s => s == SettingsKey.Password))).Returns("test");
            _viewModel.Initialize();
            _viewModel.AuthenticationViewModel.UserIsLoggedIn.Should().Be(true);
        }        
    }
}