using System.Xml;
using Microsoft.Office.Interop.Word;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class MSWordDocumentGenerator
    {
        private static Application wordApp;
        private static readonly XmlDocument xmlWithData = new XmlDocument();

        public static void CloseMSWord()
        {
            wordApp.Quit();
        }

        public static void BuildWordDocumentFromTemplate(Model.Document documentToPreview, Model.Template template)
        {
            string templateType = template.TYPE;

            xmlWithData.LoadXml(documentToPreview.DOC);

            object oMissing = System.Reflection.Missing.Value;
            object oTemplatePath = PathFinder.FetchWordTemplatePath(templateType);

            wordApp = new Application();
            Document wordDoc = new Document();
            wordDoc = wordApp.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, ref oMissing);

            foreach (Field myMergeField in wordDoc.Fields)
            {
                Range rngFieldCode = myMergeField.Code;

                // TODO: Fix these to format correctly
                rngFieldCode.Font.Name = "Arial";
                rngFieldCode.Font.Size = 11;
                rngFieldCode.Font.Bold = -1;

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
            // TODO: Impl contract
            switch (contractField)
            {
                case "ContractNumber":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText);
                    break;
                case "Today":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText);
                    break;
                default:
                    break;
            }
        }

        public static void FillCertificateTemplateField(string certificateField)
        {
            // TODO: Impl certificate 
            switch (certificateField)
            {
                case "....":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("....").InnerText);
                    break;
                case "...":
                    wordApp.Selection.TypeText(xmlWithData.SelectSingleNode("...").InnerText);
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
