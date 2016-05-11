using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities.Helpers;

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
            CheckRequiredDocumentsCommand = new TemplateCommand(DetermineDevicesMissingRequiredDocument, param => this.canExecuteCommand);
        }

        private async void DetermineDevicesMissingRequiredDocument(object commandParameter)
        {
            MetroWindow placeholder = (App.Current.MainWindow as MetroWindow);
            List<string> devicesMissingRequiredDocument = new List<string>();

            DocumentWatchdog.DetermineRequiredDocuments();

            using(CashRegisterServiceContext dbModel = new CashRegisterServiceContext())
            {
                dbModel.Devices.ToList().ForEach(device => ValidateDeviceIsInOrder(device, devicesMissingRequiredDocument));
            }

            if(devicesMissingRequiredDocument.Count != 0)
            {
                await placeholder.ShowMessageAsync("ПРЕДУПРЕЖДЕНИЕ", "Липсват задължителни документи за следните апарати: \n"+ string.Join("\n", devicesMissingRequiredDocument));
            }
        }

        private void ValidateDeviceIsInOrder(Device device, List<string> devicesMissingRequiredDocument)
        {
            DocumentWatchdog.InspectDocumentsForDevice(device, devicesMissingRequiredDocument);
        }

        private void RemoveTempDocs(object commandParameter)
        {
            string[] tempDocs = Directory.GetFiles(PathFinder.temporaryDocumentsPath);

            if(tempDocs != null)
            {
                tempDocs.ToList().ForEach(temp => File.Delete(temp));
            }         
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
