using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CashRegisterRepairs.ViewModel
{
    public class AddDeviceViewModel : INotifyPropertyChanged, IAdditionVM
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

        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        // Local storage
        private List<Device> deviceStorage;

        public AddDeviceViewModel()
        {
            // Autofills the appropriate combo box(Site/Model)
            SelectedSite = (TransitionContext.selectedSite != null) ? TransitionContext.selectedSite.NAME : null;
            SelectedModel = (TransitionContext.selectedModel != null) ? TransitionContext.selectedModel.MODEL : null;

            // Initialize storage
            deviceStorage = new List<Device>();

            // Initialize backing collections for combo boxes and their content 
            Sites = new List<string>();
            Models = new List<string>();
            dbModel.Sites.ToList().ForEach(site => Sites.Add(site.NAME));
            dbModel.DeviceModels.ToList().ForEach(model => Models.Add(model.MODEL));

            // Initialize commands with methods
            CommitDevices = new TemplateCommand(CommitRecords, param => this.canExecute);
            SaveDevice = new TemplateCommand(SaveRecord, param => this.canExecute);
            EnableSubviewDisplay = new TemplateCommand(EnableSubviews, param => this.canExecute);
        }

        private ICommand _commitDevices;
        public ICommand CommitDevices
        {
            get { return _commitDevices; }
            set { _commitDevices = value; }
        }

        private ICommand _saveDevice;
        public ICommand SaveDevice
        {
            get { return _saveDevice; }
            set { _saveDevice = value; }
        }

        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        private void EnableSubviews(object obj)
        {
            TransitionContext.ConsumeObjectsAfterUse(SelectedSite, SelectedModel);
            TransitionContext.EnableSubviewOpen();
        }

        public void ClearFields()
        {
            SIM = string.Empty;
            DEVICE_NUM_POSTFIX = string.Empty;
            FISCAL_NUM_POSTFIX = string.Empty;
            NAP_DATE = null;
            NAP_NUMBER = string.Empty;
        }

        public void SaveRecord(object obj)
        {
            // Create device object
            Device device = new Device();
            device.SIM = SIM;
            device.DEVICE_NUM_POSTFIX = DEVICE_NUM_POSTFIX;
            device.FISCAL_NUM_POSTFIX = FISCAL_NUM_POSTFIX;
            device.NAP_NUMBER = NAP_NUMBER;

            // TODO: Validate better here
            device.NAP_DATE = NAP_DATE ?? DateTime.Today;

            // Set linking properties with other tables( FOREIGN KEYS )
            device.Site = dbModel.Sites.ToList().Where(site => site.NAME == SelectedSite).FirstOrDefault();
            device.DeviceModel = dbModel.DeviceModels.ToList().Where(model => model.MODEL == SelectedModel).FirstOrDefault();

            // Save to storage and clear controls to allow for more entries
            deviceStorage.Add(device);
            ClearFields();
        }

        public void CommitRecords(object obj)
        {
            deviceStorage.ForEach(device => dbModel.Devices.Add(device));
            dbModel.SaveChanges();
        }

        private List<string> _sitesFromDB;
        public List<string> Sites
        {
            get { return _sitesFromDB; }
            set { _sitesFromDB = value; NotifyPropertyChanged(); }
        }

        private List<string> _modelsFromDB;
        public List<string> Models
        {
            get { return _modelsFromDB; }
            set { _modelsFromDB = value; NotifyPropertyChanged(); }
        }

        private string _selectedSite;
        public string SelectedSite
        {
            get { return _selectedSite; }
            set { _selectedSite = value; NotifyPropertyChanged(); }
        }

        private string _selectedModel;
        public string SelectedModel
        {
            get { return _selectedModel; }
            set { _selectedModel = value; NotifyPropertyChanged(); }
        }

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

    }
}
