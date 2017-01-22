namespace Sideloader.Settings
{
    public interface ISettingsRepository
    {
        void Load();

        string GetValue(SettingsKey key);

        void SetValue(SettingsKey key, string value);
        void Save();
    }
}