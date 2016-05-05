using System.IO;
using System.Reflection;

namespace CashRegisterRepairs.Utilities
{
    public static class ExtractionHelper
    {
        public static string[] FetchServiceProfile()
        {
            return File.ReadAllText(FetchServiceProfilePath()).Split('\n');
        }

        public static string FetchServiceProfilePath()
        {
            return ResolveAppPath() + @"\Resources\Service.txt";
        }

        public static string ResolveAppPath()
        {
            string assemblyPathNode = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string binPathNode = Directory.GetParent(assemblyPathNode).FullName;
            return Directory.GetParent(binPathNode).FullName;
        }

    }
}
