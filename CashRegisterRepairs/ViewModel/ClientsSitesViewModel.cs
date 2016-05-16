using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.View;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.ViewModel.Interfaces;
using CashRegisterRepairs.Utilities.Helpers;
using CashRegisterRepairs.Utilities.GridDisplayObjects;

namespace CashRegisterRepairs.ViewModel
{
    public class ClientsSitesViewModel : INotifyPropertyChanged, IViewModel, IViewModelUtilityExtentions
    {
        // FIELDS & COLLECTIONS
        #region FIELDS
        private readonly MetroWindow placeholder;
        private readonly PlaceholderViewModel tabNavigator;
        private readonly CashRegisterServiceContext dbModel;
        private bool canExecuteCommand = true; // command enable/disable
        private bool canOpenSubviewForm = true; // addition forms enable/disable
        private bool isCommitExecuted = false; // flag whether the commit was executed or not
        #endregion

        #region COLLECTIONS
        #region Local Storage(caches)
        private List<Manager> managersCache;
        private List<Client> clientsCache;
        private List<Site> sitesCache;
        private List<Device> devicesCache;
        #endregion

        #region Grid filling collections
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

        #region Combo box filler collections for DEVICES VIEW
        public List<string> ModelsList { get; private set; }
        public List<string> SitesList { get; private set; }
        #endregion
        #endregion

        public ClientsSitesViewModel(PlaceholderViewModel tabNav)
        {
            // Initialize DB context
            dbModel = new CashRegisterServiceContext();
            placeholder = App.Current.MainWindow as MetroWindow;
            tabNavigator = tabNav;

            // Initializing datagrid backing collections
            _sites = new ObservableCollection<Site>();
            _clients = new ObservableCollection<ClientDisplay>();
            LoadClientsInGrid();

            // Initialize caches
            managersCache = new List<Manager>();
            clientsCache = new List<Client>();
            sitesCache = new List<Site>();
            devicesCache = new List<Device>();

            // Generic commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecuteCommand);
            DisplaySitesCommand = new TemplateCommand(ShowSitesOfSelectedCLient, param => this.canExecuteCommand);

            // Additions commands
            AddClientCommand = new TemplateCommand(ShowClientsAdditionForm, param => this.canExecuteCommand);
            AddSiteCommand = new TemplateCommand(ShowSitesAdditionForm, param => this.canExecuteCommand);
            AddDeviceCommand = new TemplateCommand(ShowDevicesAdditionForm, param => this.canExecuteCommand);

            // Clients commands
            SaveClientAndManagerCommand = new TemplateCommand(SaveClient, param => this.canExecuteCommand);
            CommitClientsAndManagersCommand = new TemplateCommand(CommitClientRecords, param => this.canExecuteCommand);

            // Sites commands
            SaveSiteCommand = new TemplateCommand(SaveSite, param => this.canExecuteCommand);
            CommitSiteCommand = new TemplateCommand(CommitSites, param => this.canExecuteCommand);

            // Devices commands
            SaveDeviceCommand = new TemplateCommand(SaveDevice, param => this.canExecuteCommand);
            CommitDevicesCommand = new TemplateCommand(CommitDevicesRecords, param => this.canExecuteCommand);
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
            clientsCache.Clear();
            managersCache.Clear();
            sitesCache.Clear();
            devicesCache.Clear();
        }

        public async void ShowAdditionCount(string formIdentifier)
        {
            int newEntries = 0;

            if (!isCommitExecuted)
            {
                await placeholder.ShowMessageAsync("ИНФО" , "Няма добавени записи!");
                return;
            }

            switch (formIdentifier)
            {
                case "client":
                    newEntries = clientsCache.Count;
                    ClearFieldsClients();
                    break;
                case "site":
                    newEntries = sitesCache.Count;
                    ClearFieldsSites();
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
                await placeholder.ShowMessageAsync("ИНФО" , "Добавени " + newEntries + " записа!");
                isCommitExecuted = false;
            }
        }
        #endregion

        #region Grid loading methods
        private void LoadClientsInGrid()
        {
            Clients.Clear();

            foreach (Client client in dbModel.Clients.ToList())
            {
                Manager manager = client.Manager;
                ClientDisplay clientDisplay = new ClientDisplay(client, manager);
                Clients.Add(clientDisplay);
            }
        }

        private void ShowSitesOfSelectedCLient(object commandParameter)
        {
            Sites.Clear();

            if (SelectedClient != null)
            {
                ClientDisplay selectedClientFromGrid = SelectedClient as ClientDisplay;

                IEnumerable<Site> sitesFromDB = dbModel.Sites.ToList().Where(site => site.CLIENT_ID == selectedClientFromGrid.ID);

                // Load sites in grid
                sitesFromDB.ToList().ForEach(Sites.Add);
            }
        }
        #endregion

        #region Display addition form methods
        private void ShowClientsAdditionForm(object clientsTabDataContext)
        {
            if (canOpenSubviewForm)
            {
                AddClientView addClientsView = new AddClientView();
                addClientsView.DataContext = clientsTabDataContext;
                addClientsView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void ShowSitesAdditionForm(object clientsTabDataContext)
        {
            if (canOpenSubviewForm)
            {
                if (SelectedClient == null)
                {
                    // Replace this with metro dialog
                    placeholder.ShowMessageAsync("ИНФО" , "Няма избран клиент!");
                    return;
                }

                AddSiteView addSitesView = new AddSiteView();
                addSitesView.DataContext = clientsTabDataContext;
                addSitesView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void ShowDevicesAdditionForm(object clientsTabDataContext)
        {
            // Initialize&Fill combo boxes for devices view
            SitesList = new List<string>();
            ModelsList = new List<string>();
            dbModel.Sites.ToList().ForEach(site => SitesList.Add(site.NAME));
            dbModel.DeviceModels.ToList().ForEach(model => ModelsList.Add(model.MODEL));

            // Set the selected site name to the combo box
            SelectedSiteName = (SelectedSite as Site).NAME;

            if (canOpenSubviewForm)
            {
                AddDeviceView addDevicesView = new AddDeviceView();
                addDevicesView.DataContext = clientsTabDataContext;
                addDevicesView.Show();
                canOpenSubviewForm = false;
            }
        }
        #endregion

        #region Clear field methods
        private void ClearFieldsClients()
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

        private void ClearFieldsSites()
        {
            SiteName = string.Empty;
            SiteAddress = string.Empty;
            SitePhone = string.Empty;
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

        #region Clients(SAVE+COMMIT)
        public async void SaveClient(object commandParameter)
        {
            if (string.IsNullOrEmpty(EGN) && string.IsNullOrEmpty(BULSTAT))
            {
                await placeholder.ShowMessageAsync("ГРЕШКА","Клиентът трябва да е физическо лице ИЛИ фирма!");
                return;
            }

            if(!string.IsNullOrEmpty(EGN) && !string.IsNullOrEmpty(BULSTAT))
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Невалидни/невъведени данни!");
                return;
            }

            Manager manager = new Manager();
            manager.NAME = MANAGER;
            manager.PHONE = PHONE;

            Client client = new Client();
            client.EGN = EGN;
            client.NAME = NAME;
            client.TDD = TDD;
            client.ADDRESS = ADDRESS;
            client.BULSTAT = BULSTAT;
            client.COMMENT = COMMENT;
            client.Manager = manager;

            if (!FieldValidator.HasAnEmptyField(manager) && !FieldValidator.HasAnIncorrectlyFormattedField(client))
            {
                managersCache.Add(manager);
                clientsCache.Add(client);
                ClearFieldsClients();
            }
            else
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Невалидни/невъведени данни!");
                return;
            }
        }

        public async void CommitClientRecords(object commandParameter)
        {
            try
            {
                managersCache.ForEach(manager => dbModel.Managers.Add(manager));
                clientsCache.ForEach(client => dbModel.Clients.Add(client));
                dbModel.SaveChanges();

                LoadClientsInGrid();

                isCommitExecuted = true;
            }
            catch (Exception e)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "ПРОБЛЕМ С ЗАПАЗВАНЕТО В БД: \n" + e.InnerException.InnerException.Message);
            }
            finally
            {
                managersCache.Clear();
                clientsCache.Clear();
                (App.Current.Windows.OfType<AddClientView>().SingleOrDefault(w => w.Name == "ClientAdditionForm") as MetroWindow).Close();
            }
        }
        #endregion

        #region Sites(SAVE+COMMIT)
        private async void SaveSite(object commandParameter)
        {
            Site site = new Site();
            site.NAME = SiteName;
            site.ADDRESS = SiteAddress;
            site.PHONE = SitePhone;
            site.Client = dbModel.Clients.Find((SelectedClient as ClientDisplay).ID);

            if (!FieldValidator.HasAnEmptyField(site) && !FieldValidator.HasAnIncorrectlyFormattedField(site))
            {
                sitesCache.Add(site);
                ClearFieldsSites();
            }
            else
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Невалидни/невъведени данни!");
            }  
        }

        private async void CommitSites(object commandParameter)
        {
            try
            {
                sitesCache.ForEach(site => dbModel.Sites.Add(site));
                dbModel.SaveChanges();

                sitesCache.ForEach(site => Sites.Add(site));

                isCommitExecuted = true;
            }
            catch (Exception e)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "ПРОБЛЕМ С ЗАПАЗВАНЕТО В БД: \n" + e.InnerException.InnerException.Message);
            }
            finally
            {
                sitesCache.Clear();
                (App.Current.Windows.OfType<AddSiteView>().SingleOrDefault(w => w.Name == "SiteAdditionForm") as MetroWindow).Close();
            }
        }
        #endregion

        #region Devices(SAVE+COMMIT)
        public async void SaveDevice(object commandParameter)
        {
            Device device = new Device();
            device.SIM = SIM;
            device.DEVICE_NUM_POSTFIX = DEVICE_NUM_POSTFIX;
            device.FISCAL_NUM_POSTFIX = FISCAL_NUM_POSTFIX;
            device.NAP_NUMBER = NAP_NUMBER;
            device.NAP_DATE = NAP_DATE ?? DateTime.Today;
            device.Site = dbModel.Sites.Find((SelectedSite as Site).ID);
            device.DeviceModel = dbModel.DeviceModels.ToList().Where(model => model.MODEL == SelectedDeviceModel).FirstOrDefault();

            if (!FieldValidator.HasAnEmptyField(device) && !FieldValidator.HasAnIncorrectlyFormattedField(device))
            {
                devicesCache.Add(device);
                ClearFieldsDevices();
            }
            else
            {
                await placeholder.ShowMessageAsync("ГРЕШКА", "Невалидни/невъведени данни!");
            }      
        }

        public async void CommitDevicesRecords(object commandParameter)
        {
            try
            {
                devicesCache.ForEach(device => dbModel.Devices.Add(device));
                dbModel.SaveChanges();

                isCommitExecuted = true;
            }
            catch (Exception e)
            {
               await placeholder.ShowMessageAsync("ГРЕШКА", "ПРОБЛЕМ С ЗАПАЗВАНЕТО В БД: \n" + e.InnerException.InnerException.Message);
            }
            finally
            {
                devicesCache.Clear();

                (App.Current.Windows.OfType<AddDeviceView>().SingleOrDefault(w => w.Name == "DeviceAdditionForm") as MetroWindow).Close();
                tabNavigator.SelectedTab = 1;
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

        #region Client commands
        private ICommand _addClientCommand;
        public ICommand AddClientCommand
        {
            get { return _addClientCommand; }
            set { _addClientCommand = value; }
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
        #endregion

        #region Site commands
        private ICommand _displaySitesCommand;
        public ICommand DisplaySitesCommand
        {
            get { return _displaySitesCommand; }
            set { _displaySitesCommand = value; }
        }

        private ICommand _addSiteCommand;
        public ICommand AddSiteCommand
        {
            get { return _addSiteCommand; }
            set { _addSiteCommand = value; }
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
        #endregion

        #region Device commands
        private ICommand _addDeviceCommand;
        public ICommand AddDeviceCommand
        {
            get { return _addDeviceCommand; }
            set { _addDeviceCommand = value; }
        }

        private ICommand _saveDeviceCommand;
        public ICommand SaveDeviceCommand
        {
            get { return _saveDeviceCommand; }
            set { _saveDeviceCommand = value; }
        }

        private ICommand _commitDevicesCommand;
        public ICommand CommitDevicesCommand
        {
            get { return _commitDevicesCommand; }
            set { _commitDevicesCommand = value; }
        }
        #endregion
        #endregion

        // SELECTION PROPERTIES (CURRENT CONTEXT)
        #region SELECTION PROPERTIES (CURRENT CONTEXT)
        private string _selectedSiteName = string.Empty;
        public string SelectedSiteName
        {
            get { return _selectedSiteName; }
            set { _selectedSiteName = value; NotifyPropertyChanged(); }
        }

        private string _selectedDeviceModel = string.Empty;
        public string SelectedDeviceModel
        {
            get { return _selectedDeviceModel; }
            set { _selectedDeviceModel = value; NotifyPropertyChanged(); }
        }

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
        #endregion

        // PROPERTIES
        #region PROPERTIES

        #region Client properties
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

        #region Site properties
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
