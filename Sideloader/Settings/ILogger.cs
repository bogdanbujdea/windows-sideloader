using System;

namespace Sideloader.Settings
{
    public interface ILogger
    {
        void Info(string info);

        void Error(Exception exception);

        void Error(string errorMessage, Exception exception);

        string LogFileName { get; set; }

        void Error(string errorMessage);

        void SaveLogSession();
    }
}