using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Utilities.Helpers;
using CashRegisterRepairs.Model;
using System.Collections.Generic;

namespace CashRegisterRepairs.ViewModel
{
    public class PlaceholderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<IViewModel> _tabViewModels;
        public ObservableCollection<IViewModel> TabViewModels { get { return _tabViewModels; } }

        public PlaceholderViewModel()
        {
            // Initialize VMs of all TABS
            _tabViewModels = new ObservableCollection<IViewModel>();
            _tabViewModels.Add(new ClientsSitesViewModel(this));
            _tabViewModels.Add(new ModelsDevicesViewModel(this));
            _tabViewModels.Add(new TemplatesDocumentsViewModel());
            _tabViewModels.Add(new ServiceInfoViewModel());

            RemoveTempDocsCommand = new TemplateCommand(RemoveTempDocs,param => this.canExecuteCommand);
            CheckRequiredDocumentsCommand = new TemplateCommand(CheckDevicesForRequiredDocuments, param => this.canExecuteCommand);
        }

        private async void CheckDevicesForRequiredDocuments(object commandParameter)
        {
            MetroWindow placeholder = (App.Current.MainWindow as MetroWindow);
            List<string> problematicDevices = new List<string>();

            DocumentWatchdog.DetermineRequiredDocuments();

            using(CashRegisterServiceContext dbModel = new CashRegisterServiceContext())
            {
                dbModel.Devices.ToList().ForEach(device => CheckDeviceDocuments(device, problematicDevices));
            }

            if(problematicDevices.Count != 0)
            {
                await placeholder.ShowMessageAsync("ПРЕДУПРЕЖДЕНИЕ", "Липсват задължителни документи за следните апарати:" + "\n" + string.Join("\n", problematicDevices));
            }
        }

        private void CheckDeviceDocuments(Device device, List<string> problematicDevices)
        {
            DocumentWatchdog.InspectDocumentsForDevice(device, problematicDevices);
        }

        private void RemoveTempDocs(object commandParameter)
        {
            string[] tempDocs = Directory.GetFiles(PathFinder.temporaryDocumentsPath);

            tempDocs.ToList().ForEach(temp => File.Delete(temp));
        }

        private ICommand _removeTempDocsCommand;
        public ICommand RemoveTempDocsCommand
        {
            get { return _removeTempDocsCommand; }
            set { _removeTempDocsCommand = value; }
        }

        private ICommand _checkReqiredDocumentsCommand;
        public ICommand CheckRequiredDocumentsCommand
        {
            get { return _checkReqiredDocumentsCommand; }
            set { _checkReqiredDocumentsCommand = value; }
        }

        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; NotifyPropertyChanged(); }
        }

        private bool canExecuteCommand = true;
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
