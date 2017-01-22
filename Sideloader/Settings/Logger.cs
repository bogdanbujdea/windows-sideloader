using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sideloader.Settings
{
    public class Logger : ILogger
    {
        private static readonly object SyncRoot = new object();
        private static volatile ILogger _instance;
        private readonly List<string> _events = new List<string>();
        private readonly string _session;

        private Logger()
        {
            _session = DateTime.UtcNow.ToString("f");
        }

        public static ILogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        _instance = new Logger();
                    }
                }
                return _instance;
            }
        }

        public string LogFileName { get; set; }

        public void Info(string info)
        {
            _events.Add(DateTime.UtcNow.ToString("g") + " INFO: " + info);
            Debug.WriteLine(_events.LastOrDefault());
        }

        public void Error(Exception exception)
        {
            _events.Add(DateTime.UtcNow.ToString("g") + " EXCEPTION: " + exception.Message + Environment.NewLine + "STACK TRACE: " + exception.StackTrace);
            Debug.WriteLine(_events.LastOrDefault());
        }

        public void Error(string errorMessage, Exception exception)
        {
            Error(errorMessage);
            _events.Add(DateTime.UtcNow.ToString("g") + " ERROR: " + errorMessage + Environment.NewLine + exception.Message + Environment.NewLine + "STACK TRACE: " + exception.StackTrace);
        }

        public void Error(string errorMessage)
        {
            _events.Add(DateTime.UtcNow.ToString("g") + " ERROR: " + errorMessage);
            Debug.WriteLine(_events.LastOrDefault());
        }

        public void SaveLogSession()
        {
            SaveTextFile();
        }

        private void SaveTextFile()
        {
            var sb = new StringBuilder();
            sb.Append("Sesiune: " + _session);
            foreach (var info in _events)
            {
                sb.Append(info).Append(Environment.NewLine);
            }
            File.WriteAllText(DateTime.Now.ToString("yy-MM-dd") + ".txt", sb.ToString());
        }
    }
}
