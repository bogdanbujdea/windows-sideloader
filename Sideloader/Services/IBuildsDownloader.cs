using System.Collections.Generic;
using System.Threading.Tasks;
using Sideloader.ViewModels;

namespace Sideloader.Services
{
    public interface IBuildsDownloader
    {
        Task<List<Build>> GetBuilds();
    }
}