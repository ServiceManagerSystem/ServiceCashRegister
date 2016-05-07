using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.Utilities.Helpers;
using CashRegisterRepairs.Utilities.GridDisplayObjects;

namespace CashRegisterRepairs.ViewModel
{
    public class ServiceInfoViewModel : INotifyPropertyChanged, IViewModel
    {
        private readonly MetroWindow placeholder;

        public ServiceInfoViewModel()
        {
            placeholder = App.Current.MainWindow as MetroWindow;

            // Load current profile
            LoadServiceProfile();

            // Disable editing at first
            IsFocusable = false;
            IsUnmodifable = true;

            // Initialize commands
            EnableEditingCommand = new TemplateCommand(EnableEditing, param => this.canExecute);
            SaveServiceProfileCommand = new TemplateCommand(SaveServiceProfile, param => this.canExecute);
        }

        // METHODS
        #region METHODS
        private void LoadServiceProfile()
        {
            string[] serviceProfileItems = PathFinder.FetchServiceProfile();

            ProfileDisplay = new ServiceProfileDisplay();
            ProfileDisplay.Name = serviceProfileItems[0];
            ProfileDisplay.Bulstat = serviceProfileItems[1];
            ProfileDisplay.Address = serviceProfileItems[2];
            ProfileDisplay.Manager = serviceProfileItems[3];
            ProfileDisplay.Phone = serviceProfileItems[4];
        }

        private void EnableEditing(object commandParameter)
        {
            IsFocusable = true;
            IsUnmodifable = false;
        }

        private async void SaveServiceProfile(object commandParameter)
        {
            string[] serviceProfileItems = PathFinder.FetchServiceProfile();

            StringBuilder profileBuilder = new StringBuilder(string.Join(",", serviceProfileItems).Trim());

            profileBuilder.Replace(serviceProfileItems[0], ProfileDisplay.Name.Trim())
            .Replace(serviceProfileItems[1], ProfileDisplay.Bulstat.Trim())
            .Replace(serviceProfileItems[2], ProfileDisplay.Address.Trim())
            .Replace(serviceProfileItems[3], ProfileDisplay.Manager.Trim())
            .Replace(serviceProfileItems[4], ProfileDisplay.Phone.Trim());

            File.WriteAllText(PathFinder.serviceProfilePath, profileBuilder.ToString().Trim());

            await placeholder.ShowMessageAsync("ПРОМЯНА", "Промените по профила са запазени!");

            IsFocusable = false;
            IsUnmodifable = true;
        }
        #endregion

        // COMMANDS
        #region COMMANDS
        private ICommand _enableEditingCommand;
        public ICommand EnableEditingCommand
        {
            get { return _enableEditingCommand; }
            set { _enableEditingCommand = value; }
        }

        private ICommand _saveServiceProfileCommand;
        public ICommand SaveServiceProfileCommand
        {
            get { return _saveServiceProfileCommand; }
            set { _saveServiceProfileCommand = value; }
        }

        private ICommand _resetFocusCommand;
        public ICommand ResetFocusCommand
        {
            get { return _resetFocusCommand; }
            set { _resetFocusCommand = value; }
        }
        #endregion

        // PROPERTIES
        #region PROPERTIES
        private ServiceProfileDisplay _profileDisplay;
        public ServiceProfileDisplay ProfileDisplay
        {
            get { return _profileDisplay; }
            set { _profileDisplay = value; NotifyPropertyChanged(); }
        }

        private bool _isUnmodifable;
        public bool IsUnmodifable
        {
            get { return _isUnmodifable; }
            set { _isUnmodifable = value; NotifyPropertyChanged(); }
        }

        private bool _isFocusable;
        public bool IsFocusable
        {
            get { return _isFocusable; }
            set { _isFocusable = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        private bool canExecute = true;
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
