using CashRegisterRepairs.Model;

namespace CashRegisterRepairs.Utilities
{
    public class ClientDisplay
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public string EGN { get; set; }
        public string BULSTAT { get; set; }
        public string ADDRESS { get; set; }
        public string MANAGER { get; set; }
        public string PHONE { get; set; }
        public string TDD { get; set; }
        public string COMMENT { get; set; }

        public ClientDisplay(Client client, Manager manager)
        {
            ID = client.ID;
            NAME = client.NAME;
            EGN = client.EGN;
            BULSTAT = client.BULSTAT;
            ADDRESS = client.ADDRESS;
            MANAGER = manager.NAME;
            PHONE = manager.PHONE;
            TDD = client.TDD;
            COMMENT = client.COMMENT;
        }
    }
}
