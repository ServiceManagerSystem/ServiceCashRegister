using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        // Subview disable/enable control field
        private bool canOpenSubviewForm = true;

        // Command disable/enable control field
        private bool canExecute = true;

        // Notify bullshit
        #region NOTIFY
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion

        // DB Context
        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        // Grid collections
        #region Grid Collections
        private ObservableCollection<Template> _templates;
        public ObservableCollection<Template> Templates
        {
            get { return _templates; }
            set { _templates = value; }
        }

        private ObservableCollection<Document> _documents;
        public ObservableCollection<Document> Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }
        #endregion

        // CTOR(NEEDS REFACTOR)
        public TemplatesDocumentsViewModel()
        {
            // Initialize backing datagrid collections
            _documents = new ObservableCollection<Document>();
            Templates = new ObservableCollection<Template>(dbModel.Templates);

            // Initialize backing collections for combo boxes and their content 
            Sites = new List<string>();
            Clients = new List<string>();
            Devices = new List<string>();

            // Move these two groups somwhere so they get detected ( otherwise the constructor is called only once )
            // Initialize combo box states
            // IsClientAuto = (TransitionContext.selectedClient == null);
            IsSiteAuto = (TransitionContext.selectedSite == null);
            IsDeviceAuto = (TransitionContext.selectedDevice == null);

            // Initialize autos
            // SelectedClient = IsClientAuto ? TransitionContext.selectedClient : null ;
            SelectedSite = IsSiteAuto ? TransitionContext.selectedSite : null;
            SelectedDevice = IsDeviceAuto ? TransitionContext.selectedDevice : null;

            // Fill combo box lists
            dbModel.Sites.ToList().ForEach(site => Sites.Add(site.NAME));
            dbModel.Clients.ToList().ForEach(client => Clients.Add(client.NAME));
            dbModel.Devices.ToList().ForEach(device => Devices.Add(device.DeviceModel.DEVICE_NUM_PREFIX + device.DEVICE_NUM_POSTFIX));

            // Initialize commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecute);
            DisplayDocumentsCommand = new TemplateCommand(ShowDocumentsOfTemplate, param => this.canExecute);
            AddTemplateCommand = new TemplateCommand(ShowTemplateAdditionForm, param => this.canExecute);
            AddDocumentCommand = new TemplateCommand(ShowDocumentAdditionForm, param => this.canExecute);
        }

        // COMMANDS
        #region COMMANDS
        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        private ICommand _addTemplateCommand;
        public ICommand AddTemplateCommand
        {
            get { return _addTemplateCommand; }
            set { _addTemplateCommand = value; }
        }

        private ICommand _addDocumentCommand;
        public ICommand AddDocumentCommand
        {
            get { return _addDocumentCommand; }
            set { _addDocumentCommand = value; }
        }

        private ICommand _displayDocumentsCommand;
        public ICommand DisplayDocumentsCommand
        {
            get { return _displayDocumentsCommand; }
            set { _displayDocumentsCommand = value; }
        }
        #endregion

        // METHODS
        #region METHODS - ALL
        private void EnableSubview(object obj)
        {
            canOpenSubviewForm = true;
        }

        // SHOW FORM METHODS
        #region SHOW FORM METHODS (NEEDS REFACTOR)
        private void ShowDocumentsOfTemplate(object obj)
        {
            throw new NotImplementedException();
        }

        private void ShowTemplateAdditionForm(object obj)
        {
            if (canOpenSubviewForm)
            {
                AddTemplateView addTemplatesView = new AddTemplateView();
                addTemplatesView.DataContext = obj;
                addTemplatesView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void ShowDocumentAdditionForm(object obj)
        {
            // Untested, probably needs some work
            // IsClientAuto = (TransitionContext.selectedClient == null);
            IsSiteAuto = (TransitionContext.selectedSite == null);
            IsDeviceAuto = (TransitionContext.selectedDevice == null);

            // SelectedClient = IsClientAuto ? TransitionContext.selectedClient : null;
            SelectedSite = IsSiteAuto ? TransitionContext.selectedSite : null;
            SelectedDevice = IsDeviceAuto ? TransitionContext.selectedDevice : null;

            // Move on to view ( allow one instance at a time )
            if (canOpenSubviewForm)
            {
                AddDocumentView addDocumentsView = new AddDocumentView();
                addDocumentsView.Show();
                canOpenSubviewForm = false;
            }
        }
        #endregion

        // ComboBox filler collections(maybe make them fields - see other tabs)
        #region ComboBox FILLERS
        private List<string> _sitesFromDB;
        public List<string> Sites
        {
            get { return _sitesFromDB; }
            set { _sitesFromDB = value; NotifyPropertyChanged(); }
        }

        private List<string> _clientsFromDB;
        public List<string> Clients
        {
            get { return _clientsFromDB; }
            set { _clientsFromDB = value; NotifyPropertyChanged(); }
        }

        private List<string> _devicesFromDB;
        public List<string> Devices
        {
            get { return _devicesFromDB; }
            set { _devicesFromDB = value; NotifyPropertyChanged(); }
        }
        #endregion

        // SELECTION PROPS
        #region SELECTION PROPS
        private Client _selectedClient;
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; NotifyPropertyChanged(); }
        }

        private Site _selectedSite;
        public Site SelectedSite
        {
            get { return _selectedSite; }
            set { _selectedSite = value; NotifyPropertyChanged(); }
        }

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set { _selectedDevice = value; NotifyPropertyChanged(); }
        }

        private object _selectedTemplateFromGrid;
        public object SelectedTemplate
        {
            get { return _selectedTemplateFromGrid; }
            set { _selectedTemplateFromGrid = value; NotifyPropertyChanged(); }
        }

        private object _selectedDocumentFromGrid;
        public object SelectedDocument
        {
            get { return _selectedDocumentFromGrid; }
            set { _selectedDocumentFromGrid = value; NotifyPropertyChanged(); }
        }
        #endregion

        // PROPS - NEEDS A LOT MORE PROPERTIES SEE VIEW`s BINDINGS...
        #region PROPS
        private bool _isClientAuto;
        public bool IsClientAuto
        {
            get { return _isClientAuto; }
            set { _isClientAuto = value; NotifyPropertyChanged(); }
        }

        private bool _isSiteAuto;
        public bool IsSiteAuto
        {
            get { return _isSiteAuto; }
            set { _isSiteAuto = value; NotifyPropertyChanged(); }
        }

        private bool _isDeviceAuto;
        public bool IsDeviceAuto
        {
            get { return _isDeviceAuto; }
            set { _isDeviceAuto = value; NotifyPropertyChanged(); }
        }
        #endregion
        #endregion
    }

}
