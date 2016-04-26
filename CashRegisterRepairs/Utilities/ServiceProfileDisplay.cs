namespace CashRegisterRepairs.Utilities
{
    public class ServiceProfileDisplay
    {
        public string Name { get; set; }
        public string Bulstat { get; set; }
        public string Address { get; set; }
        public string Manager { get; set; }
        public string Phone { get; set; }
        public string LocationImagePath { get; } = @"/Resources/plus-icon.png";
    }
}
