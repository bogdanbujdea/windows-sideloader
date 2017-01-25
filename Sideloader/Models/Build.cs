namespace Sideloader.Models
{
    public class Build
    {
        public string Name { get; set; }

        public AppPackage ARMBuild { get; set; }

        public AppPackage X86Build { get; set; }

        public AppPackage X64Build { get; set; }
    }
}