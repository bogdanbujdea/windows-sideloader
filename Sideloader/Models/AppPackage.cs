namespace Sideloader.Models
{
    public class AppPackage
    {
        public AppPackage()
        {
            Platform = new Platform(PlatformType.Unknown);
        }

        public string DownloadUrl { get; set; }
        public string InstallFolderPath { get; set; }
        public Platform Platform { get; set; }
    }

    public class Platform
    {
        public Platform(PlatformType platformType)
        {
            PlatformType = platformType;
            switch (platformType)
            {
                case PlatformType.ARM:
                    Name = "ARM";
                    break;
                case PlatformType.x86:
                    Name = "x86";
                    break;
                case PlatformType.x64:
                    Name = "x64";
                    break;
            }
        }

        public string Name { get; }

        public PlatformType PlatformType { get; }
    }

    public enum PlatformType
    {
        Unknown,
        ARM,
        x86,
        x64
    }
}