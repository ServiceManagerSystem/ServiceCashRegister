using System;
using System.IO;
using System.Reflection;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class PathFinder
    {
        public static readonly string serviceProfilePath = ResolveAppPath() + @"\Resources\Service.txt";
        public static readonly string temporaryDocumentsPath = ResolveAppPath() + @"\Resources\TemporaryDocuments\";

        public static readonly string contractWordTemplatePath = ResolveAppPath() + @"\Resources\WordTemplates\ContractTemplate.dotx";
        public static readonly string certificateWordTemplatePath = ResolveAppPath() + @"\Resources\WordTemplates\CertificateTemplate.dotx";
        public static readonly string protocolWordTemplatePath = ResolveAppPath() + @"\Resources\WordTemplates\ProtocolTemplate.dotx";

        public static string[] FetchServiceProfile()
        {
            if (File.Exists(serviceProfilePath))
            {
                if(new FileInfo(serviceProfilePath).Length != 0)
                {
                    return File.ReadAllText(serviceProfilePath).Split(',');
                }
            }

            return null;
        }

        public static string FetchTemporaryWordDocumentFullPath()
        {
           return temporaryDocumentsPath + Guid.NewGuid() + ".doc";
        }

        public static string FetchWordTemplatePath(string templateType)
        {
            switch (templateType)
            {
                case "Договор":
                    return contractWordTemplatePath;
                case "Свидетелство":
                    return certificateWordTemplatePath;
                case "Протокол":
                    return protocolWordTemplatePath;
                default:
                    return null;
            }
        }

        private static string ResolveAppPath()
        {
            string assemblyPathNode = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string binPathNode = Directory.GetParent(assemblyPathNode).FullName;
            return Directory.GetParent(binPathNode).FullName;
        }

    }
}
