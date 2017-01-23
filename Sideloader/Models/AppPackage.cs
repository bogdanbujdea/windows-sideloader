namespace Sideloader.ViewModels
{
    public class AppPackage
    {
        public string DownloadUrl { get; set; }
        public PackageType PackageType { get; set; }
    }

    public enum PackageType
    {
        Unknown,
        ARM,
        x86,
        x64
    }
}