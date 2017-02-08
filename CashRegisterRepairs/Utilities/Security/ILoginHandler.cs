using System.Security;

namespace CashRegisterRepairs.Utilities.Security
{
    public interface ILoginHandler
    {
        bool Login(string username, SecureString password);
    }
}
