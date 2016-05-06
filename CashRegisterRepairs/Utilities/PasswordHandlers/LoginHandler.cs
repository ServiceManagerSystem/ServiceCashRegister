using System;
using System.Security;
using System.Runtime.InteropServices;

namespace CashRegisterRepairs.Utilities.PasswordHandlers
{
    public class LoginHandler : ILoginHandler
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
