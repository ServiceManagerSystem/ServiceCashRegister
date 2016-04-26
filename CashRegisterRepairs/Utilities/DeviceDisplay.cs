using CashRegisterRepairs.Model;
using System;

namespace CashRegisterRepairs.Utilities
{
    public class DeviceDisplay
    {
        public int ID { get; set; }
        public string SITE_NAME { get; set; }
        public string CLIENT_NAME { get; set; }
        public string DEVICE_NUM_POSTFIX { get; set; }
        public string FISCAL_NUM_POSTFIX { get; set; }
        public string NAP_NUMBER { get; set; }
        public DateTime NAP_DATE { get; set; }
        public string SIM { get; set; }

        public DeviceDisplay(Device device, Site site, Client client)
        {
            ID = device.ID;
            SITE_NAME = site.NAME;
            CLIENT_NAME = client.NAME;
            DEVICE_NUM_POSTFIX = device.DEVICE_NUM_POSTFIX;
            FISCAL_NUM_POSTFIX = device.FISCAL_NUM_POSTFIX;
            NAP_NUMBER = device.NAP_NUMBER;
            NAP_DATE = device.NAP_DATE;
            SIM = device.SIM;
        }
    }
}
