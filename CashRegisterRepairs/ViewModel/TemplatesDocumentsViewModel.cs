using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        public static DeviceDisplay selectedDeviceTest;
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

        private ObservableCollection<DocumentDisplay> _documents;
        public ObservableCollection<DocumentDisplay> Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }
        #endregion

        // CTOR
        public TemplatesDocumentsViewModel()
        {
            // Initialize backing datagrid collections
            _templates = new ObservableCollection<Template>(dbModel.Templates);
            Documents = new ObservableCollection<DocumentDisplay>();

            // Initialize backing collections for combo boxes and their content 
            Sites = new ObservableCollection<string>();
            Clients = new ObservableCollection<string>();
            Devices = new ObservableCollection<string>();

            // CB enables
            IsClientEnabled = true;
            IsSiteEnabled = false;
            IsDeviceEnabled = false;

            // Fill combo box for clients
            dbModel.Clients.ToList().ForEach(client => Clients.Add(client.NAME));

            AddTemplateCommand = new TemplateCommand(ShowTemplateAdditionForm, param => this.canExecute);
            AddDocumentCommand = new TemplateCommand(ShowDocumentAdditionForm, param => this.canExecute);

            // Initialize commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecute);
            LoadAutosCommand = new TemplateCommand(FillCBAutomatically, param => this.canExecute);
            ClearCBCommand = new TemplateCommand(ClearCB, param => this.canExecute);
            FillCommand = new TemplateCommand(FillCB, param => this.canExecute);
            PreviewDocumentCommand = new TemplateCommand(PreviewDocument, param => this.canExecute);
            DisplayDocumentsCommand = new TemplateCommand(ShowDocumentsOfTemplate, param => this.canExecute);
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

        private ICommand _loadAutosCommand;
        public ICommand LoadAutosCommand
        {
            get { return _loadAutosCommand; }
            set { _loadAutosCommand = value; }
        }

        private ICommand _clearCBCommand;
        public ICommand ClearCBCommand
        {
            get { return _clearCBCommand; }
            set { _clearCBCommand = value; }
        }

        private ICommand _fillCommand;
        public ICommand FillCommand
        {
            get { return _fillCommand; }
            set { _fillCommand = value; }
        }

        private ICommand _viewDocumentCommand;
        public ICommand PreviewDocumentCommand
        {
            get { return _viewDocumentCommand; }
            set { _viewDocumentCommand = value; }
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
            if (SelectedTemplate == null)
            {
                MessageBox.Show("Няма избран шаблон!", "ЛИПСВАЩ ШАБЛОН", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Clear old display
            Documents.Clear();

            // NEEDS FIXING
            foreach (Document doc in dbModel.Documents)
            {
                if (doc.Template.TYPE.Equals((SelectedTemplate as Template).TYPE))
                {
                    // Display all if nothing is selected
                    if (SelectedClient == null)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.Equals(doc.Client)).FirstOrDefault();
                        Device device = dbModel.Devices.Where(dev => dev.Site.Equals(site)).FirstOrDefault();
                        DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);

                        Documents.Add(docDisplay);
                    }
                    // Match if anything is selected
                    else if (doc.Client.NAME == SelectedClient)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.NAME.Equals(SelectedClient)).FirstOrDefault();
                        Device device = dbModel.Devices.Where(dev => dev.Site.Equals(site)).FirstOrDefault();
                        DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);

                        Documents.Add(docDisplay);
                    }
                }


            }

            Documents.OrderByDescending(document => document.END_DATE);
        }

        private void LoadDocumentToGrid(Document doc)
        {
            Device device = dbModel.Devices.Where(dev => (dev.DeviceModel.DEVICE_NUM_PREFIX + dev.DEVICE_NUM_POSTFIX) == SelectedDevice).FirstOrDefault();
            DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, device.Site, device);

            Documents.Add(docDisplay);
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
            // Move on to view ( allow one instance at a time )
            if (canOpenSubviewForm)
            {
                AddDocumentView addDocumentsView = new AddDocumentView();
                addDocumentsView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void FillCBAutomatically(object obj)
        {
            if (selectedDeviceTest != null)
            {
                IsClientEnabled = false;
                IsSiteEnabled = false;
                IsDeviceEnabled = false;

                SelectedClient = selectedDeviceTest.CLIENT_NAME;
                SelectedSite = selectedDeviceTest.SITE_NAME;
                Device actual = dbModel.Devices.Find(selectedDeviceTest.ID);
                SelectedDevice = actual.DeviceModel.DEVICE_NUM_PREFIX + selectedDeviceTest.DEVICE_NUM_POSTFIX;
            }
        }

        private void ClearCB(object obj)
        {
            SelectedClient = null;
            SelectedSite = null;
            SelectedDevice = null;

            IsClientEnabled = true;
        }

        private void FillCB(object obj)
        {
            switch (obj as string)
            {
                case "client":
                    IsSiteEnabled = true;
                    SelectedSite = null;

                    Sites.Clear();
                    dbModel.Sites.ToList().Where(site => site.Client.NAME == SelectedClient).ToList().ForEach(s => Sites.Add(s.NAME));
                    break;
                case "site":
                    IsDeviceEnabled = true;
                    SelectedDevice = null;

                    Devices.Clear();
                    dbModel.Devices.ToList().Where(device => device.Site.NAME == SelectedSite).ToList().ForEach(d => Devices.Add(d.DeviceModel.DEVICE_NUM_PREFIX + d.DEVICE_NUM_POSTFIX));
                    break;
                default:
                    break;
            }

            ShowDocumentsOfTemplate(null);
        }

        private void PreviewDocument(object obj)
        {

        }
        #endregion

        // ComboBox filler collections
        #region ComboBox FILLERS
        private ObservableCollection<string> _sitesFromDB;
        public ObservableCollection<string> Sites
        {
            get { return _sitesFromDB; }
            set { _sitesFromDB = value; }
        }

        private ObservableCollection<string> _clientsFromDB;
        public ObservableCollection<string> Clients
        {
            get { return _clientsFromDB; }
            set { _clientsFromDB = value; }
        }

        private ObservableCollection<string> _devicesFromDB;
        public ObservableCollection<string> Devices
        {
            get { return _devicesFromDB; }
            set { _devicesFromDB = value; }
        }
        #endregion

        // SELECTION PROPS
        #region SELECTION PROPS
        private string _selectedClient;
        public string SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; NotifyPropertyChanged(); }
        }

        private string _selectedSite;
        public string SelectedSite
        {
            get { return _selectedSite; }
            set { _selectedSite = value; NotifyPropertyChanged(); }
        }

        private string _selectedDevice;
        public string SelectedDevice
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
        public bool IsClientEnabled
        {
            get { return _isClientAuto; }
            set { _isClientAuto = value; NotifyPropertyChanged(); }
        }

        private bool _isSiteAuto;
        public bool IsSiteEnabled
        {
            get { return _isSiteAuto; }
            set { _isSiteAuto = value; NotifyPropertyChanged(); }
        }

        private bool _isDeviceAuto;
        public bool IsDeviceEnabled
        {
            get { return _isDeviceAuto; }
            set { _isDeviceAuto = value; NotifyPropertyChanged(); }
        }
        #endregion
        #endregion
    }

}
