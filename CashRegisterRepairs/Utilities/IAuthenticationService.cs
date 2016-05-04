using System.Security;

namespace CashRegisterRepairs.Utilities
{
    public interface IAuthenticationService
    {
        bool Login(string username, SecureString password);
    }
}
