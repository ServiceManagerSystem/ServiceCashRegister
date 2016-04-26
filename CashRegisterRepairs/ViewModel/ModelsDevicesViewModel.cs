using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.View;

namespace CashRegisterRepairs.ViewModel
{
    public class ModelsDevicesViewModel : INotifyPropertyChanged, IViewModel
    {
        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        private bool canExecute = true;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private ObservableCollection<DeviceModel> _models;
        public ObservableCollection<DeviceModel> Models
        {
            get { return _models; }
            set { _models = value; }
        }

        private ObservableCollection<DeviceDisplay> _devices;
        public ObservableCollection<DeviceDisplay> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        public ModelsDevicesViewModel()
        {
            _devices = new ObservableCollection<DeviceDisplay>();
            Models = new ObservableCollection<DeviceModel>(dbModel.DeviceModels);

            AddModelCommand = new TemplateCommand(ShowModelAdditionForm, param => this.canExecute);
            DisplayDevicesCommand = new TemplateCommand(ShowDevicesOfModel, param => this.canExecute);
        }

        private ICommand _addModel;
        public ICommand AddModelCommand
        {
            get { return _addModel; }
            set { _addModel = value; }
        }

        private ICommand _displayDevicesCommand;
        public ICommand DisplayDevicesCommand
        {
            get { return _displayDevicesCommand; }
            set { _displayDevicesCommand = value; }
        }

        private void ShowDevicesOfModel(object obj)
        {
            // Get rid of previous displays
            Devices.Clear();

            // Avoid selection of empty row
            if (SelectedModel != null && (SelectedModel is DeviceModel))
            {
                DeviceModel selectedModelFromGrid = SelectedModel as DeviceModel;

                AddDevicesToGrid(selectedModelFromGrid);

                // Extract to helper may be
                if (Devices.Count == 0)
                {
                    MessageBox.Show("No devices of this model!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AddDevicesToGrid(DeviceModel selectedModelFromGrid)
        {
            foreach (Device device in dbModel.Devices.ToList())
            {
                if (device.MODEL_ID == selectedModelFromGrid.ID)
                {
                    Site deviceSite = device.Site;
                    Client deviceClient = deviceSite.Client;
                    
                    DeviceDisplay deviceDisplay = new DeviceDisplay(device, deviceSite, deviceClient);
                    Devices.Add(deviceDisplay);
                }
            }
        }

        private void ShowModelAdditionForm(object obj)
        {
            // Move on to view ( allow one instance at a time )
            if (TransitionContext.CanOpenSubview())
            {
                AddModelView addModelsView = new AddModelView();
                addModelsView.Show();
                TransitionContext.DisableSubviewOpen();
            }
        }

        private object _selectedModelFromGrid;
        public object SelectedModel
        {
            get { return _selectedModelFromGrid; }
            set { _selectedModelFromGrid = value; NotifyPropertyChanged(); }
        }

        private object _selectedDeviceFromGrid;
        public object SelectedDevice
        {
            get { return _selectedDeviceFromGrid; }
            set { _selectedDeviceFromGrid = value; NotifyPropertyChanged(); }
        }

        // These might be needed in context( we will see )
        //private string _site;
        //public string SITE
        //{
        //    get { return _site; }
        //    set { _site = value; NotifyPropertyChanged(); }
        //}

        //private string _client;
        //public string CLIENT
        //{
        //    get { return _client; }
        //    set { _client = value; NotifyPropertyChanged(); }
        //}
    }
}
