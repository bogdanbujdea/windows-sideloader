using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Sideloader.Settings
{
    public class SettingsRepository : ISettingsRepository
    {

        private static readonly object SyncRoot = new object();
        private static volatile ISettingsRepository _instance;
        private const string SettingsFile = "settings.json";
        private Dictionary<string, string> _localSettings;

        private SettingsRepository()
        {
            
        }

        public static ISettingsRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        _instance = new SettingsRepository();
                    }
                }
                return _instance;
            }
        }

        public void Load()
        {
            try
            {
                _localSettings = new Dictionary<string, string>();
                var json = File.ReadAllText(SettingsFile);
                _localSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (Exception exception)
            {
                Logger.Instance.Error("Loading settings: ", exception);
            }
        }

        public string GetValue(SettingsKey key)
        {
            if (_localSettings.ContainsKey(key.ToString()))
                return _localSettings[key.ToString()];
            return null;
        }

        public void SetValue(SettingsKey key, string value)
        {
            _localSettings[key.ToString()] = value;
            Save();
        }

        public void Save()
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(_localSettings));
        }
    }
}