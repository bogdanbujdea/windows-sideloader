using System;

namespace Sideloader.Services
{
    public class PackageException : Exception
    {
        public bool IsHandled { get; set; }

        public PackageException(string message, bool isHandled): base(message)
        {
            IsHandled = isHandled;
        }
    }
}