using System;
using System.Windows;
using System.Security;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Utilities.PasswordHandlers;

namespace CashRegisterRepairs.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        // FIELDS
        #region FIELDS
        ILoginHandler loginHandler = new LoginHandler();
        private bool canExecuteCommand = true;
        #endregion

        public LoginViewModel()
        {
            LoginCommand = new TemplateCommand(TryLogin, param => this.canExecuteCommand);
        }

        //  METHODS
        private async void TryLogin(object commandParameter)
        {
            MetroWindow loginView = Application.Current.Windows[0] as MetroWindow;

            if (string.IsNullOrEmpty(Username) || Password == null)
            {
                await loginView.ShowMessageAsync("ГРЕШКА", "Липсват данни!");
                return;
            }

            if (loginHandler.Login(Username, Password))
            {
                PlaceholderView home = new PlaceholderView();
                App.Current.MainWindow = home;
                home.Show();

                loginView.Close();
            }
            else
            {
                await loginView.ShowMessageAsync("ГРЕШКА", "Грешни данни!");
            }

        }

        // COMMANDS
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get { return _loginCommand; }
            set { _loginCommand = value; }
        }

        // PROPERTIES
        #region PROPERTIES
        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyPropertyChanged(); }
        }

        private SecureString _password;
        public SecureString Password
        {
            get { return _password; }
            set { _password = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }
}
