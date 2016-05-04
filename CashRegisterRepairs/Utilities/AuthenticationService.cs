using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CashRegisterRepairs.Utilities
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool Login(string username, SecureString password)
        {
            IntPtr passwordBSTR = default(IntPtr);
            string insecurePassword = "";
            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                insecurePassword = Marshal.PtrToStringBSTR(passwordBSTR);
            }
            catch
            {
                insecurePassword = "";
            }
            return MockServiceProxyCall(username, insecurePassword);
        }

        private bool MockServiceProxyCall(string username, string password)
        {
            if (username == "Rosen" && password == "1234") return true;
            else return false;
        }
    }
}
