using CashRegisterRepairs.Model;
using System.Collections.Generic;
using System.Linq;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class DocumentWatchdog
    {
        private static bool isContractRequired;
        private static bool isCertificateRequired;
        private static bool isProtocolRequired;

        private static bool hasContract;
        private static bool hasCertificate;
        private static bool hasProtocol;

        private static void DetermineRequiredDocuments()
        {
            using (CashRegisterServiceContext dbModel = new CashRegisterServiceContext())
            {
                isContractRequired = dbModel.Templates.Where(template => template.TYPE.Equals("Договор")).FirstOrDefault().STATUS == "ЗАДЪЛЖИТЕЛЕН";
                isCertificateRequired = dbModel.Templates.Where(template => template.TYPE.Equals("Договор")).FirstOrDefault().STATUS == "ЗАДЪЛЖИТЕЛЕН";
                isProtocolRequired = dbModel.Templates.Where(template => template.TYPE.Equals("Договор")).FirstOrDefault().STATUS == "ЗАДЪЛЖИТЕЛЕН";
            }
        }

        private static bool ValidateDeviceDocuments(Device device)
        {
            List<Document> documentsForDevice = new List<Document>();

            using (CashRegisterServiceContext dbModel = new CashRegisterServiceContext())
            {
                documentsForDevice = dbModel.Documents.Where(doc => doc.DEVICE_ID == device.ID).ToList();

                if (documentsForDevice.Count != 0)
                {
                    hasContract = (documentsForDevice.Where(doc => doc.Template.TYPE.Equals("Договор")).Count() != 0);
                    hasCertificate = (documentsForDevice.Where(doc => doc.DEVICE_ID == device.ID && doc.Template.TYPE.Equals("Свидетелство")).Count() != 0);
                    hasProtocol = (documentsForDevice.Where(doc => doc.DEVICE_ID == device.ID && doc.Template.TYPE.Equals("Протокол")).Count() != 0);
                }
            }

            return (isContractRequired && hasContract) && (isCertificateRequired && hasCertificate) && (isProtocolRequired && hasProtocol);
        }

        public static void InspectDocumentsForDevice(Device device, List<string> devicesMissingRequiredDocuments)
        {
            DetermineRequiredDocuments();

            if (!ValidateDeviceDocuments(device))
            {
                devicesMissingRequiredDocuments.Add(device.DeviceModel.DEVICE_NUM_PREFIX+device.DEVICE_NUM_POSTFIX);
            }
        }

    }
}
