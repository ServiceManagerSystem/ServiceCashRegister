using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.View;
using System.Collections.Generic;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Utilities.GridDisplayObjects;
using CashRegisterRepairs.Utilities.Helpers;

namespace CashRegisterRepairs.ViewModel
{
    public class ModelsDevicesViewModel : INotifyPropertyChanged, IViewModel
    {
        // FIELDS & COLLECTIONS
        #region FIELDS
        private readonly MetroWindow placeholder;
        private readonly CashRegisterServiceContext dbModel;
        private readonly PlaceholderViewModel tabNavigator; // reference to the main view model in order to change tab dynamically
        private bool canExecuteCommand = true; // command enable/disable
        private bool canOpenSubviewForm = true; // addition forms enable/disable
        private bool isCommitExecuted = false; // flag whether the commit was executed or not
        #endregion

        #region COLLECTIONS
        #region Local Storage(caches)
        private List<DeviceModel> modelsCache;
        private List<Device> devicesCache;
        #endregion

        #region Grid filling collections
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

        #region Combo box filler collections for DEVICES VIEW
        public List<string> ModelsList { get; private set; }
        public List<string> SitesList { get; private set; }
        #endregion
        #endregion

        public ModelsDevicesViewModel(PlaceholderViewModel tabNav)
        {
            // Initialize DB context and navigator reference
            dbModel = new CashRegisterServiceContext();
            placeholder = App.Current.MainWindow as MetroWindow;
            tabNavigator = tabNav;

            // Initializing datagrid backing collections
            _devices = new ObservableCollection<DeviceDisplay>();
            Models = new ObservableCollection<DeviceModel>(dbModel.DeviceModels);

            // Initialize caches
            modelsCache = new List<DeviceModel>();
            devicesCache = new List<Device>();

            // Generic commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecuteCommand);
            DraftContractCommand = new TemplateCommand(SwitchToDocumentsTab, param => this.canExecuteCommand);

            // Additions commands
            AddModelCommand = new TemplateCommand(ShowModelsAdditionForm, param => this.canExecuteCommand);
            AddDeviceCommand = new TemplateCommand(ShowDevicesAdditionForm, param => this.canExecuteCommand);

            // Models commands
            SaveModelsCommand = new TemplateCommand(SaveModelRecord, param => this.canExecuteCommand);
            CommitModelsCommand = new TemplateCommand(CommitModels, param => this.canExecuteCommand);

            // Devices commands
            DisplayDevicesCommand = new TemplateCommand(ShowDevicesOfSelectedModel, param => this.canExecuteCommand);
            SaveDeviceCommand = new TemplateCommand(SaveDeviceRecord, param => this.canExecuteCommand);
            CommitDevicesCommand = new TemplateCommand(CommitDeviceRecords, param => this.canExecuteCommand);
        }

        // METHODS
        #region METHODS
        #region Utility extension methods implementation
        public void EnableSubview(object comingFromForm)
        {
            canOpenSubviewForm = true;

            ClearCaches();

            ShowAdditionCount(comingFromForm as string);
        }

        public void ClearCaches()
        {
            modelsCache.Clear();
            devicesCache.Clear();
        }

        public async void ShowAdditionCount(string formIdentifier)
        {
            int newEntries = 0;

            if (!isCommitExecuted)
            {
                await placeholder.ShowMessageAsync("ИНФО", "Няма добавени записи!");
                return;
            }

            switch (formIdentifier)
            {
                case "models":
                    newEntries = modelsCache.Count;
                    ClearFieldsModels();
                    break;
                case "device":
                    newEntries = devicesCache.Count;
                    ClearFieldsDevices();
                    break;
                default:
                    break;
            }

            if (newEntries > 0)
            {
                placeholder.ShowMessageAsync("ИНФО", "Добавени " + newEntries + " записа!");
                isCommitExecuted = false;
            }
        }

        private void SwitchToDocumentsTab(object selectedDevice)
        {
            TemplatesDocumentsViewModel.selectedDevice = selectedDevice as DeviceDisplay;
            tabNavigator.SelectedTab = 2;
        }
        #endregion

        #region Grid loading methods
        private void ShowDevicesOfSelectedModel(object commandParameter)
        {
            if (SelectedModel != null)
            {
                DeviceModel selectedModelFromGrid = SelectedModel as DeviceModel;

                LoadDevicesInGrid(selectedModelFromGrid);
            }
        }

        private void LoadDevicesInGrid(DeviceModel selectedModelFromGrid)
        {
            Devices.Clear();

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
        #endregion

        #region Display addition form methods
        private void ShowModelsAdditionForm(object modelsDataContext)
        {
            if (canOpenSubviewForm)
            {
                AddModelView addModelsView = new AddModelView();
                addModelsView.DataContext = modelsDataContext;
                addModelsView.Show();
                canOpenSubviewForm = false;
            }
        }

        private async void ShowDevicesAdditionForm(object modelsDataContext)
        {
            if (SelectedModel == null)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Няма избран модел!");
                return;
            }

            SitesList = new List<string>();
            ModelsList = new List<string>();
            dbModel.Sites.ToList().ForEach(site => SitesList.Add(site.NAME));
            dbModel.DeviceModels.ToList().ForEach(model => ModelsList.Add(model.MODEL));

            SelectedDeviceModel = (SelectedModel as DeviceModel).MODEL;

            if (canOpenSubviewForm)
            {
                AddDeviceView addDevicesView = new AddDeviceView();
                addDevicesView.DataContext = modelsDataContext;
                addDevicesView.Show();
                canOpenSubviewForm = false;
            }
        }
        #endregion

        #region Clear fields methods
        public void ClearFieldsModels()
        {
            Manufacturer = string.Empty;
            Model = string.Empty;
            Certificate = string.Empty;
            DeviceNumPre = string.Empty;
            FiscalNumPre = string.Empty;
        }

        private void ClearFieldsDevices()
        {
            SelectedDeviceModel = string.Empty;
            SIM = string.Empty;
            DEVICE_NUM_POSTFIX = string.Empty;
            FISCAL_NUM_POSTFIX = string.Empty;
            NAP_DATE = null;
            NAP_NUMBER = string.Empty;
        }
        #endregion

        #region Models(SAVE+COMMIT)
        private void SaveModelRecord(object commandParameter)
        {
            DeviceModel devModel = new DeviceModel();
            devModel.MANUFACTURER = Manufacturer;
            devModel.MODEL = Model;
            devModel.CERTIFICATE = Certificate;
            devModel.DEVICE_NUM_PREFIX = DeviceNumPre;
            devModel.FISCAL_NUM_PREFIX = FiscalNumPre;

            if (!FieldValidator.HasAnEmptyField(devModel))
            {
                modelsCache.Add(devModel);
            }

            ClearFieldsModels();
        }

        private async void CommitModels(object commandParameter)
        {
            try
            {
                modelsCache.ForEach(model => dbModel.DeviceModels.Add(model));
                dbModel.SaveChanges();

                modelsCache.ForEach(model => Models.Add(model));

                isCommitExecuted = true;
            }
            catch (Exception e)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", e.InnerException.InnerException.Message);
            }
            finally
            {
                modelsCache.Clear();
            }
        }
        #endregion

        #region Devices(SAVE+COMMIT)
        public void SaveDeviceRecord(object commandParameter)
        {
            Device device = new Device();
            device.SIM = SIM;
            device.DEVICE_NUM_POSTFIX = DEVICE_NUM_POSTFIX;
            device.FISCAL_NUM_POSTFIX = FISCAL_NUM_POSTFIX;
            device.NAP_NUMBER = NAP_NUMBER;
            device.NAP_DATE = NAP_DATE ?? DateTime.Today;
            device.Site = dbModel.Sites.Where(site => site.NAME.Equals(SelectedSiteName)).FirstOrDefault();
            device.DeviceModel = dbModel.DeviceModels.Find((SelectedModel as DeviceModel).ID);

            if (!FieldValidator.HasAnEmptyField(device))
            {
                devicesCache.Add(device);
            }

            ClearFieldsDevices();
        }

        private async void CommitDeviceRecords(object commandParameter)
        {
            try
            {
                devicesCache.ForEach(device => dbModel.Devices.Add(device));
                dbModel.SaveChanges();

                LoadDevicesInGrid(SelectedModel as DeviceModel);

                isCommitExecuted = true;
            }
            catch (Exception e)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", e.InnerException.InnerException.Message);
            }
            finally
            {
                devicesCache.Clear();
            }  
        }


        #endregion
        #endregion

        // COMMANDS
        #region COMMANDS
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

        #region Model commands
        private ICommand _addModelCommand;
        public ICommand AddModelCommand
        {
            get { return _addModelCommand; }
            set { _addModelCommand = value; }
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
        #endregion

        #region Devices commands
        private ICommand _addDeviceCommand;
        public ICommand AddDeviceCommand
        {
            get { return _addDeviceCommand; }
            set { _addDeviceCommand = value; }
        }

        private ICommand _displayDevicesCommand;
        public ICommand DisplayDevicesCommand
        {
            get { return _displayDevicesCommand; }
            set { _displayDevicesCommand = value; }
        }

        private ICommand _saveDevice;
        public ICommand SaveDeviceCommand
        {
            get { return _saveDevice; }
            set { _saveDevice = value; }
        }

        private ICommand _commitDevices;
        public ICommand CommitDevicesCommand
        {
            get { return _commitDevices; }
            set { _commitDevices = value; }
        }
        #endregion
        #endregion

        // SELECTION PROPERTIES (CURRENT CONTEXT)
        #region SELECTION PROPERTIES (CURRENT CONTEXT)
        private string _selectedSiteName;
        public string SelectedSiteName
        {
            get { return _selectedSiteName; }
            set { _selectedSiteName = value; NotifyPropertyChanged(); }
        }

        private string _selectedDeviceModel;
        public string SelectedDeviceModel
        {
            get { return _selectedDeviceModel; }
            set { _selectedDeviceModel = value; NotifyPropertyChanged(); }
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
        #endregion

        // PROPERTIES
        #region PROPERTIES

        #region Model properties
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

        #region Device properties
        private string _deviceNumber = string.Empty;
        public string DEVICE_NUM_POSTFIX
        {
            get { return _deviceNumber; }
            set { _deviceNumber = value; NotifyPropertyChanged(); }
        }

        private string _deviceFiscalNumer = string.Empty;
        public string FISCAL_NUM_POSTFIX
        {
            get { return _deviceFiscalNumer; }
            set { _deviceFiscalNumer = value; NotifyPropertyChanged(); }
        }

        private string _deviceSIM = string.Empty;
        public string SIM
        {
            get { return _deviceSIM; }
            set { _deviceSIM = value; NotifyPropertyChanged(); }
        }

        private string _napNumber = string.Empty;
        public string NAP_NUMBER
        {
            get { return _napNumber; }
            set { _napNumber = value; NotifyPropertyChanged(); }
        }

        private DateTime? _napDate;
        public DateTime? NAP_DATE
        {
            get { return _napDate; }
            set { _napDate = value; NotifyPropertyChanged(); }
        }
        #endregion
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
