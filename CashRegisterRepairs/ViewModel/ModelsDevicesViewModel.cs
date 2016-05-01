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
using System.Collections.Generic;

namespace CashRegisterRepairs.ViewModel
{
    public class ModelsDevicesViewModel : INotifyPropertyChanged, IViewModel
    {
        PlaceholderViewModel navigator;

        private bool canOpenSubview = true;

        #region NOTIFY
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

        #region Grid Collections
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
        #endregion

        // Local storage collections
        private List<DeviceModel> deviceModelStorage;

        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();
        
        // CTOR
        public ModelsDevicesViewModel()
        {
            // Initialize grid collections + storage
            deviceModelStorage = new List<DeviceModel>();
            _devices = new ObservableCollection<DeviceDisplay>();
            Models = new ObservableCollection<DeviceModel>(dbModel.DeviceModels);

            // Model addition Commands
            SaveModelsCommand = new TemplateCommand(SaveModels, param => this.canExecute);
            CommitModelsCommand = new TemplateCommand(CommitModels, param => this.canExecute);

            // Form + display commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubviews, param => this.canExecute);
            DraftContractCommand = new TemplateCommand(SwitchToDocumentsTab, param => this.canExecute);
            DisplayDevicesCommand = new TemplateCommand(ShowDevicesOfModel, param => this.canExecute);
            AddModelCommand = new TemplateCommand(ShowModelAdditionForm, param => this.canExecute);
        }

        public ModelsDevicesViewModel(PlaceholderViewModel nav)
        {
            navigator = nav;

            // Initialize grid collections + storage
            deviceModelStorage = new List<DeviceModel>();
            _devices = new ObservableCollection<DeviceDisplay>();
            Models = new ObservableCollection<DeviceModel>(dbModel.DeviceModels);

            // Model addition Commands
            SaveModelsCommand = new TemplateCommand(SaveModels, param => this.canExecute);
            CommitModelsCommand = new TemplateCommand(CommitModels, param => this.canExecute);

            // Form + display commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubviews, param => this.canExecute);
            DraftContractCommand = new TemplateCommand(SwitchToDocumentsTab, param => this.canExecute);
            DisplayDevicesCommand = new TemplateCommand(ShowDevicesOfModel, param => this.canExecute);
            AddModelCommand = new TemplateCommand(ShowModelAdditionForm, param => this.canExecute);
        }

        #region COMMANDS

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

        private ICommand _saveModelsCommand;
        public ICommand SaveModelsCommand
        {
            get { return _saveModelsCommand; }
            set { _saveModelsCommand = value; }
        }

        private ICommand _commitModelsCommand;
        public ICommand CommitModelsCommand
        {
            get { return _commitModelsCommand; }
            set { _commitModelsCommand = value; }
        }

        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        private ICommand _draftContractCommand;
        public ICommand DraftContractCommand
        {
            get { return _draftContractCommand; }
            set { _draftContractCommand = value; }
        }
        #endregion

        #region MODELS SAVE+COMMIT+SUBVIEW
        public void ClearFields()
        {
            Manufacturer = string.Empty;
            Model = string.Empty;
            Certificate = string.Empty;
            DeviceNumPre = string.Empty;
            FiscalNumPre = string.Empty;
        }

        public void SaveModels(object obj)
        {
            DeviceModel devModel = new DeviceModel();
            devModel.MANUFACTURER = Manufacturer;
            devModel.MODEL = Model;
            devModel.CERTIFICATE = Certificate;
            devModel.DEVICE_NUM_PREFIX = DeviceNumPre;
            devModel.FISCAL_NUM_PREFIX = FiscalNumPre;

            deviceModelStorage.Add(devModel);

            ClearFields();
        }

        public void CommitModels(object obj)
        {
            deviceModelStorage.ForEach(model => dbModel.DeviceModels.Add(model));
            deviceModelStorage.ForEach(model => Models.Add(model));
            dbModel.SaveChanges();
        }

        private void EnableSubviews(object obj)
        {
            canOpenSubview = true;
        }
        #endregion

        #region DISPLAY FROM/CONTENT
        private void ShowDevicesOfModel(object obj)
        {
            Devices.Clear();

            if (SelectedModel != null && (SelectedModel is DeviceModel))
            {
                DeviceModel selectedModelFromGrid = SelectedModel as DeviceModel;

                AddDevicesToGrid(selectedModelFromGrid);

                if (Devices.Count == 0)
                {
                    MessageBox.Show("Няма апарати от този модел!", "ИНФО", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (canOpenSubview)
            {
                AddModelView addModelsView = new AddModelView();
                addModelsView.DataContext = obj;
                addModelsView.Show();
                canOpenSubview = false;
            }
        }

        private void SwitchToDocumentsTab(object obj)
        {
            TemplatesDocumentsViewModel.selectedDevice = obj as DeviceDisplay;
            navigator.SelectedTab = 2;
        }
        #endregion

        // Selection props
        #region SELECTION PROPS(CONTEXT)
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
        #endregion

        // Props
        #region PROPERTIES
        private string _manufacturer = string.Empty;
        public string Manufacturer
        {
            get { return _manufacturer; }
            set { _manufacturer = value; NotifyPropertyChanged(); }
        }

        private string _model = string.Empty;
        public string Model
        {
            get { return _model; }
            set { _model = value; NotifyPropertyChanged(); }
        }

        private string _certificate = string.Empty;
        public string Certificate
        {
            get { return _certificate; }
            set { _certificate = value; NotifyPropertyChanged(); }
        }

        private string _deviceNumPre = string.Empty;
        public string DeviceNumPre
        {
            get { return _deviceNumPre; }
            set { _deviceNumPre = value; NotifyPropertyChanged(); }
        }

        private string _fiscalNumPre = string.Empty;
        public string FiscalNumPre
        {
            get { return _fiscalNumPre; }
            set { _fiscalNumPre = value; NotifyPropertyChanged(); }
        }
        #endregion
    }
}
