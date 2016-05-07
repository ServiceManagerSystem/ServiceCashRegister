using System;
using System.Xml;
using CashRegisterRepairs.Model;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class XmlDataFiller
    {
        public static void FillContractXml(Document document, XmlDocument template, int internalId, string[] serviceProfile)
        {
            // Title
            template.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText = " " + internalId.ToString();
            template.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText = " " + DateTime.Today.Date.ToShortDateString();
            template.SelectSingleNode("ContractTemplate/FreeText/Value").InnerText = " " + DateTime.Today.Date.ToShortDateString();

            // Service
            template.SelectSingleNode("ContractTemplate/Service/ServiceName/Value").InnerText = " " + serviceProfile[0];
            template.SelectSingleNode("ContractTemplate/Service/ServiceManager/Value").InnerText = " " + serviceProfile[2];

            // Client
            template.SelectSingleNode("ContractTemplate/Client/ClientName/Value").InnerText = " " + document.Device.Site.Client.NAME;
            template.SelectSingleNode("ContractTemplate/Client/ClientBulstat/Value").InnerText = " " + document.Device.Site.Client.BULSTAT;
            template.SelectSingleNode("ContractTemplate/Client/ClientAddress/Value").InnerText = " " + document.Device.Site.Client.ADDRESS;
            template.SelectSingleNode("ContractTemplate/Client/ClientManager/Value").InnerText = " " + document.Device.Site.Client.Manager.NAME;

            // Device
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceModel/Value").InnerText = " " + document.Device.DeviceModel.MODEL;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceNumber/Value").InnerText = " " + document.Device.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/FiskalNumber/Value").InnerText = " " + document.Device.FISCAL_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/Value").InnerText = " 5500 лв.";

            // Contract
            template.SelectSingleNode("ContractTemplate/ContractText/Text/StartDate").InnerText = " " + document.START_DATE.ToShortDateString();
            template.SelectSingleNode("ContractTemplate/ContractText/Text/EndDate").InnerText = " " + document.END_DATE.ToShortDateString();

            // Annex 1
            template.SelectSingleNode("ContractTemplate/ContractText/ServiceAddres/Value").InnerText = " " + serviceProfile[3];
            template.SelectSingleNode("ContractTemplate/ContractText/ServicePhone/Value").InnerText = " " + serviceProfile[4];
        }

        public static void FillCertificateXml(Document document, XmlDocument template, int internalId, string[] serviceProfile)
        {
            //Client
            template.SelectSingleNode("CertificateTemplate/Title/CurrDate").InnerText = " " + DateTime.Today.ToShortDateString();
            template.SelectSingleNode("CertificateTemplate/Bulstat/Value").InnerText = " " + document.Device.Site.Client.BULSTAT;
            template.SelectSingleNode("CertificateTemplate/EGN/Value").InnerText = " " + document.Device.Site.Client.EGN;
            template.SelectSingleNode("CertificateTemplate/Owner/Client/Value").InnerText = " " + document.Device.Site.Client.NAME;
            template.SelectSingleNode("CertificateTemplate/Owner/Address/Value").InnerText = " " + document.Device.Site.Client.ADDRESS;
            template.SelectSingleNode("CertificateTemplate/Owner/Manager/Value").InnerText = " " + document.Device.Site.Client.Manager.NAME;

            //Site
            template.SelectSingleNode("CertificateTemplate/Owner/Site/Value").InnerText = " " + document.Device.Site.NAME;

            //Device
            template.SelectSingleNode("CertificateTemplate/Device/Model/Value").InnerText = " " + document.Device.DeviceModel.MODEL;
            template.SelectSingleNode("CertificateTemplate/Device/Certificate/Value").InnerText = " " + document.Device.DeviceModel.CERTIFICATE;
            template.SelectSingleNode("CertificateTemplate/Device/DeviceNum/Value").InnerText = " " + document.Device.DeviceModel.DEVICE_NUM_PREFIX + document.Device.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("CertificateTemplate/Device/FiscalNum/Value").InnerText = " " + document.Device.DeviceModel.FISCAL_NUM_PREFIX + document.Device.FISCAL_NUM_POSTFIX;

            //Service
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/BulstatAndName/Value").InnerText = " " + serviceProfile[0] + " " + serviceProfile[1];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/AddressAndPhone/Value").InnerText = " " + serviceProfile[2] + " " + serviceProfile[4];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/ServiceManager/Value").InnerText = " " + serviceProfile[3];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/Value").InnerText = " " + internalId.ToString();
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/StartDate/Value").InnerText = " " + document.START_DATE.ToShortDateString();

            //NAP info
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPNumber/Value").InnerText = " " + document.Device.NAP_NUMBER;
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPDate/Value").InnerText = " " + document.Device.NAP_DATE.ToShortDateString();
        }

        public static void FillProtocolXml(Document document, XmlDocument template, int internalId, string[] serviceProfile)
        {
            throw new NotImplementedException();
        }


    }
}
