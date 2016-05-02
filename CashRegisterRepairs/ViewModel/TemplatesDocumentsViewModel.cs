using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        // A neccessary evil for exchanging information between separate VMs
        public static DeviceDisplay selectedDevice;

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

            // Enable client choice only at first
            IsClientEnabled = true;
            IsSiteEnabled = false;
            IsDeviceEnabled = false;

            // Fill combo box for clients ONLY
            dbModel.Clients.ToList().ForEach(client => Clients.Add(client.NAME));

            // Initialize commands
            EnableSubviewDisplay = new TemplateCommand(EnableSubview, param => this.canExecute);

            AddDocumentCommand = new TemplateCommand(AddDocument, param => this.canExecute);
            AddTemplateCommand = new TemplateCommand(ShowTemplateAdditionForm, param => this.canExecute);
            DisplayDocumentsInGridCommand = new TemplateCommand(ShowDocumentsOfTemplate, param => this.canExecute);

            AutofillComboBoxCommand = new TemplateCommand(FillCBAutomatically, param => this.canExecute);
            ClearComboBoxCommand = new TemplateCommand(ClearCB, param => this.canExecute);
            FillComboBoxCommand = new TemplateCommand(FillCB, param => this.canExecute);

            ShowDocumentPreviewCommand = new TemplateCommand(ShowDocumentPreviewForm, param => this.canExecute);
            PrintDocumentCommand = new TemplateCommand(PrintDocument, param => this.canExecute);
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


        private ICommand _openFileCommand;
        public ICommand OpenDocumentCommand
        {
            get { return _openFileCommand; }
            set { _openFileCommand = value; }
        }

        private ICommand _printDocumentCommand;
        public ICommand PrintDocumentCommand
        {
            get { return _printDocumentCommand; }
            set { _printDocumentCommand = value; }
        }

        private ICommand _displayDocumentsCommand;
        public ICommand DisplayDocumentsInGridCommand
        {
            get { return _displayDocumentsCommand; }
            set { _displayDocumentsCommand = value; }
        }

        private ICommand _loadAutosCommand;
        public ICommand AutofillComboBoxCommand
        {
            get { return _loadAutosCommand; }
            set { _loadAutosCommand = value; }
        }

        private ICommand _clearCBCommand;
        public ICommand ClearComboBoxCommand
        {
            get { return _clearCBCommand; }
            set { _clearCBCommand = value; }
        }

        private ICommand _fillCommand;
        public ICommand FillComboBoxCommand
        {
            get { return _fillCommand; }
            set { _fillCommand = value; }
        }

        private ICommand _viewDocumentCommand;
        public ICommand ShowDocumentPreviewCommand
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
            // Clear old display
            Documents.Clear();

            foreach (Document doc in dbModel.Documents)
            {
                if (SelectedTemplate != null && doc.Template.TYPE.Equals((SelectedTemplate as Template).TYPE))
                {
                    // Display all docs if no client is selected
                    if (SelectedClient == null)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.ID.Equals(doc.Client.ID)).FirstOrDefault();
                        if(site != null)
                        {
                            Device device = dbModel.Devices.Where(dev => dev.Site.ID.Equals(site.ID)).FirstOrDefault();
                            if(device != null)
                            {
                                DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);
                                Documents.Add(docDisplay);
                            }
                        }
                    }
                    // Match specific docs if client is selected
                    else if (doc.Client.NAME == SelectedClient)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.NAME.Equals(SelectedClient)).FirstOrDefault();
                        Device device = dbModel.Devices.Where(dev => dev.Site.ID.Equals(site.ID)).FirstOrDefault();
                        DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);

                        Documents.Add(docDisplay);
                    }
                }
            }

            // Order by expiring documents on the TOP
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

        private void AddDocument(object obj)
        {
            Document doc = new Document();
            doc.Client = dbModel.Clients.Where(client => client.NAME.Equals(SelectedClient)).FirstOrDefault();
            doc.START_DATE = DateTime.Today;
            doc.END_DATE = doc.START_DATE.Value.AddYears(1);
            doc.Template = SelectedTemplate as Template;

            // Generirano ot template-a
            // doc.DOC

            dbModel.Documents.Add(doc);
            dbModel.SaveChanges();
        }

        private void ShowDocumentPreviewForm(object obj)
        {
            if (canOpenSubviewForm)
            {
                LoadDocumentContent();

                PreviewDocumentView previewDocumentView = new PreviewDocumentView();
                previewDocumentView.DataContext = obj;
                previewDocumentView.Show();
                canOpenSubviewForm = false;
            }
        }

        private void LoadDocumentContent()
        {
            Document foundDoc = null;
            string type = (SelectedDocument as DocumentDisplay).TEMPLATE;

            if (SelectedClient != null)
            {
                string clientName = SelectedClient;
                foundDoc = dbModel.Documents.Where(doc => doc.Client.NAME.Equals(clientName) && doc.Template.TYPE.Equals(type)).FirstOrDefault();
            }
            else
            {
                DateTime? start = (SelectedDocument as DocumentDisplay).START_DATE;
                DateTime? end = (SelectedDocument as DocumentDisplay).END_DATE;
                foundDoc = dbModel.Documents.Where(doc => doc.START_DATE.Value.Equals(start.Value) && doc.END_DATE.Value.Equals(end.Value) && doc.Template.TYPE.Equals(type)).FirstOrDefault();
            }

            DocumentContent = foundDoc.DOC;
        }

        private void FillCBAutomatically(object obj)
        {
            if (selectedDevice != null)
            {
                SelectedClient = selectedDevice.CLIENT_NAME;
                SelectedSite = selectedDevice.SITE_NAME;
                Device actualDeviceObject = dbModel.Devices.Find(selectedDevice.ID);
                SelectedDevice = actualDeviceObject.DeviceModel.DEVICE_NUM_PREFIX + selectedDevice.DEVICE_NUM_POSTFIX;

                IsClientEnabled = false;
                IsSiteEnabled = false;
                IsDeviceEnabled = false;
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

        private void PrintDocument(object obj)
        {
            // Implement using DocumentViewer, make another view for this -> simple with button for print
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
        private string _documentContent;
        public string DocumentContent
        {
            get { return _documentContent; }
            set { _documentContent = value; NotifyPropertyChanged(); }
        }

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
