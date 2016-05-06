using System.Security;

namespace CashRegisterRepairs.Utilities.PasswordHandlers
{
    public interface ILoginHandler
    {
        bool Login(string username, SecureString password);
    }
}
