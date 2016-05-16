using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Maps.MapControl.WPF;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.Utilities.Helpers;
using CashRegisterRepairs.Utilities.GridDisplayObjects;

namespace CashRegisterRepairs.ViewModel
{
    public class ServiceInfoViewModel : INotifyPropertyChanged, IViewModel
    {
        // FIELDS
        #region FIELDS
        private string serviceLattitude;
        private string serviceLongtitude;
        private Location cachedServiceLocation;

        private string[] serviceProfileItems;

        private readonly MetroWindow placeholder;
        #endregion

        public ServiceInfoViewModel()
        {
            placeholder = App.Current.MainWindow as MetroWindow;

            // Load current profile
            LoadServiceProfile();

            // Load current location
            ServiceLocation = new Location(double.Parse(serviceLattitude),double.Parse(serviceLongtitude));
            cachedServiceLocation = ServiceLocation;

            // Disable editing at first
            IsMapEnabled = false;
            IsTextBoxFocusable = false;
            IsTextBoxUnmodifable = true;

            // Initialize commands
            EnableEditingCommand = new TemplateCommand(EnableEditing, param => this.canExecute);
            SaveServiceProfileCommand = new TemplateCommand(SaveServiceProfile, param => this.canExecute);
            ChangeLocationCommand = new TemplateCommand(ChangeLocationOnMouseDoubleClick, param => this.canExecute);
            ResetLocationCommand = new TemplateCommand(ResetLocation, param => this.canExecute);
        }

        // METHODS
        #region METHODS
        private void ChangeLocationOnMouseDoubleClick(object mouseEventArgs)
        {
            MouseButtonEventArgs mouseEventContext = mouseEventArgs as MouseButtonEventArgs;

            Map currentMap = mouseEventContext.Source as Map;
            Point mousePosition = mouseEventContext.GetPosition(currentMap);

            ServiceLocation = currentMap.ViewportPointToLocation(mousePosition);
            serviceLattitude = ServiceLocation.Latitude.ToString();
            serviceLongtitude = ServiceLocation.Longitude.ToString();
        }

        private void ResetLocation(object commandParameter)
        {
            ServiceLocation = cachedServiceLocation;
        }

        private void LoadServiceProfile()
        {
            serviceProfileItems = PathFinder.FetchServiceProfile();

            if (serviceProfileItems == null)
            {
                MessageBox.Show("Проблем с зареждането на фирмения профил!","ГРЕШКА", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            ProfileDisplay = new ServiceProfileDisplay();
            ProfileDisplay.Name = serviceProfileItems[0];
            ProfileDisplay.Bulstat = serviceProfileItems[1];
            ProfileDisplay.Address = serviceProfileItems[2];
            ProfileDisplay.Manager = serviceProfileItems[3];
            ProfileDisplay.Phone = serviceProfileItems[4];
            serviceLattitude = serviceProfileItems[5];
            serviceLongtitude = serviceProfileItems[6];
        }

        private void EnableEditing(object commandParameter)
        {
            IsMapEnabled = true;
            IsTextBoxFocusable = true;
            IsTextBoxUnmodifable = false;
        }

        private async void SaveServiceProfile(object commandParameter)
        {
            if (FieldValidator.HasAnEmptyField(ProfileDisplay))
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Не са позволени празни полета!");
                return;
            }
            else if (FieldValidator.HasAnIncorrectlyFormattedField(ProfileDisplay))
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Налична е невалидна стойност/формат за поле!");
                return;
            }

            MessageDialogResult choice = await placeholder.ShowMessageAsync("ПОТВЪРЖДЕНИЕ","Наистина ли искате да запазите тези промени?",MessageDialogStyle.AffirmativeAndNegative);
            if(choice == MessageDialogResult.Affirmative)
            {
                StringBuilder profileBuilder = new StringBuilder(string.Join(",", serviceProfileItems).Trim());
                profileBuilder.Replace(serviceProfileItems[0], ProfileDisplay.Name.Trim())
                .Replace(serviceProfileItems[1], ProfileDisplay.Bulstat.Trim())
                .Replace(serviceProfileItems[2], ProfileDisplay.Address.Trim())
                .Replace(serviceProfileItems[3], ProfileDisplay.Manager.Trim())
                .Replace(serviceProfileItems[4], ProfileDisplay.Phone.Trim())
                .Replace(serviceProfileItems[5], serviceLattitude.Trim())
                .Replace(serviceProfileItems[6], serviceLongtitude.Trim());

                File.WriteAllText(PathFinder.serviceProfilePath, profileBuilder.ToString().Trim());

                cachedServiceLocation = ServiceLocation;

                await placeholder.ShowMessageAsync("ПРОМЯНА", "Промените по профила са запазени!");
            }

            IsMapEnabled = false;
            IsTextBoxFocusable = false;
            IsTextBoxUnmodifable = true;
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

        private ICommand _changeLocationCommand;
        public ICommand ChangeLocationCommand
        {
            get { return _changeLocationCommand; }
            set { _changeLocationCommand = value; }
        }

        private ICommand _resetLocationCommand;
        public ICommand ResetLocationCommand
        {
            get { return _resetLocationCommand; }
            set { _resetLocationCommand = value; }
        }
        #endregion

        // PROPERTIES
        #region PROPERTIES
        private Location _serviceLocation;
        public Location ServiceLocation
        {
            get { return _serviceLocation; }
            set { _serviceLocation = value; NotifyPropertyChanged(); }
        }

        private ServiceProfileDisplay _profileDisplay;
        public ServiceProfileDisplay ProfileDisplay
        {
            get { return _profileDisplay; }
            set { _profileDisplay = value; NotifyPropertyChanged(); }
        }

        private bool _isUnmodifable;
        public bool IsTextBoxUnmodifable
        {
            get { return _isUnmodifable; }
            set { _isUnmodifable = value; NotifyPropertyChanged(); }
        }

        private bool _isFocusable;
        public bool IsTextBoxFocusable
        {
            get { return _isFocusable; }
            set { _isFocusable = value; NotifyPropertyChanged(); }
        }

        private bool _isMapEnabled;
        public bool IsMapEnabled
        {
            get { return _isMapEnabled; }
            set { _isMapEnabled = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        private bool canExecute = true;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

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
