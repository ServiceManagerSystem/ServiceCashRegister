using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using CashRegisterRepairShop.View;
using System.Windows;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.ViewModel;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.View;

namespace CashRegisterRepairShop.ViewModel
{
    public class ClientsSitesViewModel : INotifyPropertyChanged, IViewModel
    {
        private bool canOpenSubviewForm = true;

        // Notify changed bullshit
        #region NOTIFY BLA BLA
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

        // Grid collections
        #region Grid collections
        private ObservableCollection<ClientDisplay> _clients;
        public ObservableCollection<ClientDisplay> Clients
        {
            get { return _clients; }
            set { _clients = value; }
        }

        private ObservableCollection<Site> _sites;
        public ObservableCollection<Site> Sites
        {
            get { return _sites; }
            set { _sites = value; }
        }
        #endregion

        // Combo box filler collections for DEVICES VIEW
        #region CB filler collections DEVICES
        public List<string> Models { get; private set; }
        public List<string> SitesForCB { get; private set; }
        #endregion

        // Main db context object
        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        // CACHES
        #region Local Storage
        // Local storage collections(cache)
        private List<Site> siteStorage;
        private List<Manager> managerStorage;
        private List<Client> clientStorage;
        private List<Device> deviceStorage;
        #endregion

        // CTOR
        public ClientsSitesViewModel()
        {
            // Initializing datagrid backing collections
            _sites = new ObservableCollection<Site>();
            Clients = new ObservableCollection<ClientDisplay>();
            LoadClientsToGrid();

            // Storages init
            managerStorage = new List<Manager>();
            clientStorage = new List<Client>();
            siteStorage = new List<Site>();
            deviceStorage = new List<Device>();

            // Initialize commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecute);

            // Tab VIEW commands
            DisplaySitesCommand = new TemplateCommand(ShowSitesForCLient, param => this.canExecute);
            AddClientCommand = new TemplateCommand(ShowClientAdditionForm, param => this.canExecute);
            AddSiteCommand = new TemplateCommand(ShowSiteAdditionForm, param => this.canExecute);
            AddDeviceCommand = new TemplateCommand(ShowDevicesAdditionForm, param => this.canExecute);

            // Clients
            SaveClientAndManagerCommand = new TemplateCommand(SaveClientRecord, param => this.canExecute);
            CommitClientsAndManagersCommand = new TemplateCommand(CommitClientRecords, param => this.canExecute);

            // Sites
            SaveSiteCommand = new TemplateCommand(SaveSite, param => this.canExecute);
            CommitSiteCommand = new TemplateCommand(CommitSite, param => this.canExecute);

            // Devices
            CommitDevices = new TemplateCommand(CommitDeviceRecords, param => this.canExecute);
            SaveDevice = new TemplateCommand(SaveDeviceRecord, param => this.canExecute);
        }

        // METHODS
        #region CLEAR FIELDS
        public void ClearFieldsSites()
        {
            SiteName = string.Empty;
            SiteAddress = string.Empty;
            SitePhone = string.Empty;
        }
        public void ClearFieldsClients()
        {
            NAME = string.Empty;
            EGN = string.Empty;
            TDD = string.Empty;
            ADDRESS = string.Empty;
            BULSTAT = string.Empty;
            COMMENT = string.Empty;
            MANAGER = string.Empty;
            PHONE = string.Empty;
        }
        private void ClearFieldsDevices()
        {
            SIM = string.Empty;
            DEVICE_NUM_POSTFIX = string.Empty;
            FISCAL_NUM_POSTFIX = string.Empty;
            NAP_DATE = null;
            NAP_NUMBER = string.Empty;
        }
        #endregion

        #region CLIENT SAVE+COMMIT
        public void SaveClientRecord(object obj)
        {
            // Create client
            Client client = new Client();
            client.EGN = EGN;
            client.NAME = NAME;
            client.TDD = TDD;
            client.ADDRESS = ADDRESS;
            client.BULSTAT = BULSTAT;
            client.COMMENT = COMMENT;

            // Create manager
            Manager manager = new Manager();
            manager.NAME = MANAGER;
            manager.PHONE = PHONE;

            // Add to local MANAGER storage
            managerStorage.Add(manager);

            // Assign manager to client
            client.Manager = manager;

            // Add to local CLIENT storage
            clientStorage.Add(client);
            ClearFieldsClients();
        }

        public void CommitClientRecords(object obj)
        {
            managerStorage.ToList().ForEach(manager => dbModel.Managers.Add(manager));
            clientStorage.ToList().ForEach(client => dbModel.Clients.Add(client));
            dbModel.SaveChanges();

            // Reload clients after addition/s
            LoadClientsToGrid();
        }
        #endregion

        private void LoadClientsToGrid()
        {
            foreach (Client client in dbModel.Clients.ToList())
            {
                Manager manager = client.Manager;
                ClientDisplay clientDisplay = new ClientDisplay(client, manager);
                Clients.Add(clientDisplay);
            }
        }

        #region DISPLAY FORM/CONTENT
        private void ShowSitesForCLient(object obj)
        {
            Sites.Clear();

            // Avoid selection of empty row
            if (SelectedClient != null && (SelectedClient is ClientDisplay))
            {
                ClientDisplay selectedClientFromGrid = SelectedClient as ClientDisplay;
                List<Site> sitesFromDB = dbModel.Sites.ToList();

                // Add to Sites grid
                sitesFromDB.Where(site => site.CLIENT_ID == selectedClientFromGrid.ID).ToList().ForEach(Sites.Add);

                if (Sites.Count == 0)
                {
                    MessageBox.Show("Няма обекти към този клиент!", "ИНФО", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ShowClientAdditionForm(object obj)
        {
            if (canOpenSubviewForm)
            {
                AddClientView addClientsView = new AddClientView();
                addClientsView.DataContext = obj;
                addClientsView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void ShowSiteAdditionForm(object obj)
        {
            if (canOpenSubviewForm)
            {
                if (SelectedClient == null)
                {
                    MessageBox.Show("Няма избран клиент!", "ГРЕШКА", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                AddSiteView addSitesView = new AddSiteView();
                addSitesView.DataContext = obj;
                addSitesView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void ShowDevicesAdditionForm(object obj)
        {
            // Initialize combo boxes for devices view
            SitesForCB = new List<string>();
            Models = new List<string>();
            dbModel.Sites.ToList().ForEach(site => SitesForCB.Add(site.NAME));
            dbModel.DeviceModels.ToList().ForEach(model => Models.Add(model.MODEL));

            // Display the selected site name in CB
            SelectedSiteName = (SelectedSite as Site).NAME;

            if (canOpenSubviewForm)
            {
                AddDeviceView addDevicesView = new AddDeviceView();
                addDevicesView.DataContext = obj;
                addDevicesView.Show();
                canOpenSubviewForm = false;
            }

        }
        #endregion

        #region SITE SAVE+COMMIT
        private void SaveSite(object obj)
        {
            Site site = new Site();
            site.NAME = SiteName;
            site.ADDRESS = SiteAddress;
            site.PHONE = SitePhone;
            site.Client = dbModel.Clients.Find((SelectedClient as ClientDisplay).ID);
            siteStorage.Add(site);

            ClearFieldsSites();
        }
        private void CommitSite(object obj)
        {
            siteStorage.ForEach(site => dbModel.Sites.Add(site));
            siteStorage.ForEach(site => Sites.Add(site));
            dbModel.SaveChanges();
        }

        #endregion

        private void EnableSubview(object obj)
        {
            canOpenSubviewForm = true;
        }

        #region DEVICE SAVE+COMMIT
        public void SaveDeviceRecord(object obj)
        {
            // Create device object
            Device device = new Device();
            device.SIM = SIM;
            device.DEVICE_NUM_POSTFIX = DEVICE_NUM_POSTFIX;
            device.FISCAL_NUM_POSTFIX = FISCAL_NUM_POSTFIX;
            device.NAP_NUMBER = NAP_NUMBER;
            device.NAP_DATE = NAP_DATE ?? DateTime.Today;

            // Set linking properties with other tables( FOREIGN KEYS )
            device.Site = dbModel.Sites.Find((SelectedSite as Site).ID);
            device.DeviceModel = dbModel.DeviceModels.ToList().Where(model => model.MODEL == SelectedModel).FirstOrDefault();
            deviceStorage.Add(device);

            ClearFieldsDevices();
        }
        public void CommitDeviceRecords(object obj)
        {
            deviceStorage.ForEach(device => dbModel.Devices.Add(device));
            dbModel.SaveChanges();

            ClearFieldsDevices();
        }
        #endregion

        // COMMANDS
        #region COMMANDS
        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        private ICommand _saveClientAndManagerCommand;
        public ICommand SaveClientAndManagerCommand
        {
            get { return _saveClientAndManagerCommand; }
            set { _saveClientAndManagerCommand = value; }
        }

        private ICommand _commitClientsAndManagersCommand;
        public ICommand CommitClientsAndManagersCommand
        {
            get { return _commitClientsAndManagersCommand; }
            set { _commitClientsAndManagersCommand = value; }
        }

        private ICommand _addClientCommand;
        public ICommand AddClientCommand
        {
            get { return _addClientCommand; }
            set { _addClientCommand = value; }
        }

        private ICommand _addSiteCommand;
        public ICommand AddSiteCommand
        {
            get { return _addSiteCommand; }
            set { _addSiteCommand = value; }
        }

        private ICommand _displaySitesCommand;
        public ICommand DisplaySitesCommand
        {
            get { return _displaySitesCommand; }
            set { _displaySitesCommand = value; }
        }

        private ICommand _addDeviceCommand;
        public ICommand AddDeviceCommand
        {
            get { return _addDeviceCommand; }
            set { _addDeviceCommand = value; }
        }

        private ICommand _saveSiteCommand;
        public ICommand SaveSiteCommand
        {
            get { return _saveSiteCommand; }
            set { _saveSiteCommand = value; }
        }

        private ICommand _commitSiteCommand;
        public ICommand CommitSiteCommand
        {
            get { return _commitSiteCommand; }
            set { _commitSiteCommand = value; }
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
        #endregion

        // PROPERTIES
        #region CLIENT PROPS
        private string _clientName = string.Empty;
        public string NAME
        {
            get { return _clientName; }
            set { _clientName = value; NotifyPropertyChanged(); }
        }

        private string _clientEGN = string.Empty;
        public string EGN
        {
            get { return _clientEGN; }
            set { _clientEGN = value; NotifyPropertyChanged(); }
        }

        private string _clientBulstat = string.Empty;
        public string BULSTAT
        {
            get { return _clientBulstat; }
            set { _clientBulstat = value; NotifyPropertyChanged(); }
        }

        private string _clientAddress = string.Empty;
        public string ADDRESS
        {
            get { return _clientAddress; }
            set { _clientAddress = value; NotifyPropertyChanged(); }
        }

        private string _clientTDD = string.Empty;
        public string TDD
        {
            get { return _clientTDD; }
            set { _clientTDD = value; NotifyPropertyChanged(); }
        }

        private string _managerName = string.Empty;
        public string MANAGER
        {
            get { return _managerName; }
            set { _managerName = value; NotifyPropertyChanged(); }
        }

        private string _managerPhone = string.Empty;
        public string PHONE
        {
            get { return _managerPhone; }
            set { _managerPhone = value; NotifyPropertyChanged(); }
        }

        private string _clientComment = string.Empty;
        public string COMMENT
        {
            get { return _clientComment; }
            set { _clientComment = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region SITE PROPS
        private string _selectedSiteName;

        public string SelectedSiteName
        {
            get { return _selectedSiteName; }
            set { _selectedSiteName = value; NotifyPropertyChanged(); }
        }

        private string _siteName = string.Empty;
        public string SiteName
        {
            get { return _siteName; }
            set { _siteName = value; NotifyPropertyChanged(); }
        }

        private string _siteAddress = string.Empty;
        public string SiteAddress
        {
            get { return _siteAddress; }
            set { _siteAddress = value; NotifyPropertyChanged(); }
        }

        private string _sitePhone = string.Empty;
        public string SitePhone
        {
            get { return _sitePhone; }
            set { _sitePhone = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region DEVICE PROPS
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

        #region SELECTION PROPS(CONTEXT)
        private object _selectedClientFromGrid;
        public object SelectedClient
        {
            get { return _selectedClientFromGrid; }
            set { _selectedClientFromGrid = value; NotifyPropertyChanged(); }
        }

        private object _selectedSiteFromGrid;
        public object SelectedSite
        {
            get { return _selectedSiteFromGrid; }
            set { _selectedSiteFromGrid = value; NotifyPropertyChanged(); }
        }

        private string _selectedModel;
        public string SelectedModel
        {
            get { return _selectedModel; }
            set { _selectedModel = value; NotifyPropertyChanged(); }
        }
        #endregion
    }

}
