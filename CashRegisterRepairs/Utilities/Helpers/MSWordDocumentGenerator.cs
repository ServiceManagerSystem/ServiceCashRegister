using System.Xml;
using Microsoft.Office.Interop.Word;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class MSWordDocumentGenerator
    {
        private static Application wordApp;
        private static readonly XmlDocument xmlWithData = new XmlDocument();

        public static void BuildWordDocumentFromTemplate(Model.Document document, Model.Template template)
        {
            string templateType = template.TYPE;

            xmlWithData.LoadXml(document.DOC);

            object oMissing = System.Reflection.Missing.Value;
            object oTemplatePath = PathFinder.FetchWordTemplatePath(templateType);

            wordApp = new Application();
            Document wordDoc = new Document();
            wordDoc = wordApp.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, ref oMissing);

            foreach (Field myMergeField in wordDoc.Fields)
            {
                Range rngFieldCode = myMergeField.Code;

                string fieldText = rngFieldCode.Text;

                if (fieldText.StartsWith(" MERGEFIELD"))
                {
                    int endMerge = fieldText.IndexOf("\\");
                    int fieldNameLength = fieldText.Length - endMerge;
                    string fieldName = fieldText.Substring(11, endMerge - 11).Trim();

                    myMergeField.Select();

                    switch (templateType)
                    {
                        case "Договор":
                            FillContractTemplateField(fieldName);
                            break;
                        case "Свидетелство":
                            FillCertificateTemplateField(fieldName);
                            break;
                        case "Протокол":
                            FillProtocolTemplateField(fieldName);
                            break;
                        default:
                            break;
                    }
                }
            }

            string tempFilePath = PathFinder.FetchTemporaryWordDocumentFullPath();
            wordDoc.SaveAs(tempFilePath);
            wordApp.Documents.Open(tempFilePath);
        }

        public static void FillContractTemplateField(string contractField)
        {
            switch (contractField)
            {
                case "ContractNumber":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText);
                    break;
                case "CurrDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText);
                    break;
                case "FreeText":
                     wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/FreeText/Value").InnerText);
                    break;
                case "ServiceName":
                     wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Service/ServiceName/Value").InnerText);
                    break;
                case "ServiceManager":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Service/ServiceManager/Value").InnerText);
                    break;
                case "ClientName":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Client/ClientName/Value").InnerText);
                    break;
                case "ClientBulstat":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Client/ClientBulstat/Value").InnerText);
                    break;
                case "ClientAddress":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Client/ClientAddress/Value").InnerText);
                    break;
                case "ClientManager":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Client/ClientManager/Value").InnerText);
                    break;
                case "DeviceModel":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceModel/Value").InnerText);
                    break;
                case "DeviceNumber":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceNumber/Value").InnerText);
                    break;
                case "FiscalNumber":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/DeviceInfo/FiskalNumber/Value").InnerText);
                    break;
                case "Price":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/DeviceInfo/Value").InnerText);
                    break;
                case "StartDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/ContractText/Text/StartDate").InnerText);
                    break;
                case "EndDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/ContractText/Text/EndDate").InnerText);
                    break;
                case "ServiceAddress":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/ContractText/ServiceAddres/Value").InnerText);
                    break;
                case "ServicePhone":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/ContractText/ServicePhone/Value").InnerText);
                    break;
                default:
                    break;
            }
        }

        public static void FillCertificateTemplateField(string certificateField)
        {
            switch (certificateField)
            {
                case "CurrDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Title/CurrDate").InnerText);
                    break;
                case "Bulstat":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Bulstat/Value").InnerText);
                    break;
                case "Client":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Owner/Client/Value").InnerText);
                    break;
                case "Address":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Owner/Address/Value").InnerText);
                    break;
                case "Manager":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Owner/Manager/Value").InnerText);
                    break;
                case "Site":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Owner/Site/Value").InnerText);
                    break;
                case "Model":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Device/Model/Value").InnerText);
                    break;
                case "Certificate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Device/Certificate/Value").InnerText);
                    break;
                case "DeviceNum":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Device/DeviceNum/Value").InnerText);
                    break;
                case "FisicalNum":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/Device/FiscalNum/Value").InnerText);
                    break;
                case "ServiceBulstatAndName":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/ServiceInfo/BulstatAndName/Value").InnerText);
                    break;
                case "AddressAndPhone":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/ServiceInfo/AddressAndPhone/Value").InnerText);
                    break;
                case "ServiceManager":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/ServiceInfo/ServiceManager/Value").InnerText);
                    break;
                case "Contract":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/Value").InnerText);
                    break;
                case "StartDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/StartDate/Value").InnerText);
                    break;
                case "NAPNumber":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/NAPInfo/NAPNumber/Value").InnerText);
                    break;
                case "NAPDate":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("CertificateTemplate/NAPInfo/NAPDate/Value").InnerText);
                    break;
                default:
                    break;
            }
        }

        public static void FillProtocolTemplateField(string protocolField)
        {
            // TODO: Impl protocol
            switch (protocolField)
            {
                case "...":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("...").InnerText);
                    break;
                case "......":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("......").InnerText);
                    break;
                default:
                    break;
            }
        }

    }
}
