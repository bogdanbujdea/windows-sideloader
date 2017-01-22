using System;
using System.Windows;
using Sideloader.Settings;

namespace Sideloader
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsRepository.Instance.Load();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);            
            SettingsRepository.Instance.Save();
        }
    }
}
