using System.Windows;
using Sideloader.Settings;

namespace Sideloader
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            SettingsRepository.Instance.Load();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error("CRASH: ", e.Exception);
            Logger.Instance.SaveLogSession();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);            
            SettingsRepository.Instance.Save();
            Logger.Instance.SaveLogSession();
        }
    }
}
