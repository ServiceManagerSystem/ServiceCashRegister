﻿using CashRegisterRepairs.Model;
using System;

namespace CashRegisterRepairs.Utilities
{

    public class DocumentDisplay
    {
        public int ID { get; set; }
        public string CLIENT { get; set; }
        public string SITE { get; set; }
        public string DEVICE { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? END_DATE { get; set; }

        public DocumentDisplay(Client client, Site site, Device device, Document document)
        {
            ID = document.CLIENT_ID;
            CLIENT = client.NAME;
            SITE = site.NAME;
            DEVICE = device.DeviceModel.DEVICE_NUM_PREFIX + device.DEVICE_NUM_POSTFIX;
            START_DATE = document.START_DATE ?? null;
            END_DATE = document.END_DATE ?? null;
        }

    }

}
