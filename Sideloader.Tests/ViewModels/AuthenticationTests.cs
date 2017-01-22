using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Sideloader.Services;
using Sideloader.Settings;
using Sideloader.Tests.Infrastucture;
using Sideloader.ViewModels;

namespace Sideloader.Tests.ViewModels
{
    public class AuthenticationTests
    {
        private Mock<ISettingsRepository> _settingsMock;
        private Mock<IAuthenticationService> _authenticationServiceMock;
        private AuthenticationViewModel _viewModel;

        [SetUp]
        public void SetupTests()
        {
            _settingsMock = new Mock<ISettingsRepository>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _viewModel = new AuthenticationViewModel(_settingsMock.Object, _authenticationServiceMock.Object);
        }

        [Test]
        public async Task login_should_invoke_authorization_service()
        {
            _authenticationServiceMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsTask(new Report());
            _authenticationServiceMock.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            await _viewModel.Login();
            _authenticationServiceMock.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task successful_login_should_hide_login_view()
        {
            _authenticationServiceMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsTask(new Report { IsSuccessful = true });
            await _viewModel.Login();
            _viewModel.UserIsLoggedIn.Should().BeTrue();
        }

        [Test]
        public async Task successful_login_should_save_credentials_in_settings()
        {
            _authenticationServiceMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsTask(new Report { IsSuccessful = true });
            _settingsMock.Verify(s => s.SetValue(It.IsAny<SettingsKey>(), It.IsAny<string>()), Times.Never);
            await _viewModel.Login();
            _settingsMock.Verify(s => s.SetValue(It.IsAny<SettingsKey>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task failed_login_should_return_error_message()
        {
            _authenticationServiceMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsTask(new Report { IsSuccessful = false, ErrorMessage = "error" });
            var loginReport = await _viewModel.Login();
            loginReport.ErrorMessage.Should().Be("error");
        }
    }
}