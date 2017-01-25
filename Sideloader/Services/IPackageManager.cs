using System;
using System.Threading.Tasks;
using Sideloader.Models;

namespace Sideloader.Services
{
    public interface IPackageManager
    {
        Task RetrievePackage(AppPackage appPackage);
        event EventHandler<StatusInfo> PackageStatusChanged;
    }
}