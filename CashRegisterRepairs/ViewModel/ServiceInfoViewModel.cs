using CashRegisterRepairs.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CashRegisterRepairs.ViewModel
{
    public class ServiceInfoViewModel : INotifyPropertyChanged, IViewModel
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

        private ServiceProfileDisplay _profileDisplay;
        public ServiceProfileDisplay ProfileDisplay
        {
            get { return _profileDisplay; }
            set { _profileDisplay = value; NotifyPropertyChanged(); }
        }

        private readonly string serviceProfileFile;

        public ServiceInfoViewModel()
        {
            serviceProfileFile = ResolveAppPath() + @"\Resources\Service.txt";

            IsFocusable = false;
            IsUnmodifable = true;

            EnableEditingCommand = new TemplateCommand(EnableEditing, param => this.canExecute);
            SaveServiceProfileCommand = new TemplateCommand(SaveServiceProfile, param => this.canExecute);

            LoadServiceProfile();
        }

        private void SaveServiceProfile(object obj)
        {
            StringBuilder profileBuilder = new StringBuilder();
            profileBuilder
                .AppendLine(ProfileDisplay.Name)
                .AppendLine(ProfileDisplay.Bulstat)
                .AppendLine(ProfileDisplay.Address)
                .AppendLine(ProfileDisplay.Manager)
                .AppendLine(ProfileDisplay.Phone);

            File.WriteAllText(serviceProfileFile, profileBuilder.ToString());

            MessageBox.Show("Промените по профила са запазени!","ПРОМЯНА",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        private void EnableEditing(object obj)
        {
            IsFocusable = true;
            IsUnmodifable = false;
        }

        private void LoadServiceProfile()
        {

            string[] serviceProfileItems = File.ReadAllText(serviceProfileFile).Split('\n');

            ProfileDisplay = new ServiceProfileDisplay();
            ProfileDisplay.Name = serviceProfileItems[0];
            ProfileDisplay.Bulstat = serviceProfileItems[1];
            ProfileDisplay.Manager = serviceProfileItems[2];
            ProfileDisplay.Address = serviceProfileItems[3];
            ProfileDisplay.Phone = serviceProfileItems[4];
        }

        // May be move to a helper if you need to use it somewhere else
        private string ResolveAppPath()
        {
            string assemblyPathNode = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string binPathNode = Directory.GetParent(assemblyPathNode).FullName;
            return Directory.GetParent(binPathNode).FullName;
        }

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

    }
}
