using System;
using CashRegisterRepairs.Model;

namespace CashRegisterRepairs.Utilities.GridDisplayObjects
{

    public class DocumentDisplay
    {
        public int ID { get; set; }
        public string TEMPLATE { get; set; }
        public string CLIENT { get; set; }
        public string SITE { get; set; }
        public string DEVICE { get; set; }
        public DateTime START_DATE { get; set; }
        public DateTime END_DATE { get; set; }

        public DocumentDisplay(Document document, Client client, Site site, Device device)
        {
            ID = document.ID;
            TEMPLATE = document.Template.TYPE;
            CLIENT = client.NAME;
            SITE = site.NAME;
            DEVICE = device.DeviceModel.DEVICE_NUM_PREFIX + device.DEVICE_NUM_POSTFIX;
            START_DATE = document.START_DATE;
            END_DATE = document.END_DATE;
        }

    }

}
