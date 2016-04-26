using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.View;
using CashRegisterRepairs.Config;

namespace CashRegisterRepairs.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private bool canExecute = true;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public LoginViewModel()
        {
            Login = new TemplateCommand(TryLogin, param => this.canExecute);
        }

        private void TryLogin(object obj)
        {
            if(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Липсват данни", "ГРЕШКА", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (LoginInfo.username.Equals(Username.Trim()) && LoginInfo.password.Equals(Password.Trim()))
            {
                var loginView = Application.Current.Windows[0] as Window;

                PlaceholderView home = new PlaceholderView();
                App.Current.MainWindow = home;
                home.Show();

                loginView.Close();
            }
            else
            {
                MessageBox.Show("Грешни данни", "ГРЕШКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ICommand _login;
        public ICommand Login
        {
            get { return _login; }
            set { _login = value; }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; NotifyPropertyChanged(); }
        }

    }
}
