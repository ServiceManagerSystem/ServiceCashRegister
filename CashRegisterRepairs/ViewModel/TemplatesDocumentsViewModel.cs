using System;
using System.Xml;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.Utilities.Helpers;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        // FIELDS
        #region FIELDS
        public static Device selectedDevice;
        private bool canExecuteCommand = true; // command enable/disable
        private readonly CashRegisterServiceContext dbModel;
        private readonly MetroWindow placeholder;
        #endregion

        // COLLECTIONS
        #region COLLECTIONS
        #region Grid filling collections
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

        #region ComboBox filling collections
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
        #endregion

        public TemplatesDocumentsViewModel()
        {
            // Initialize prerequisites( DB Context and placeholder reference )
            dbModel = new CashRegisterServiceContext();
            placeholder = App.Current.MainWindow as MetroWindow;

            // Initialize data grid backing collections
            _templates = new ObservableCollection<Template>(dbModel.Templates);
            Documents = new ObservableCollection<Document>();

            // Initialize combo box backing collections
            Sites = new ObservableCollection<string>();
            Clients = new ObservableCollection<string>();
            Devices = new ObservableCollection<string>();

            // Initialize combo box access properties - allow for Client selection ONLY at start
            IsClientEnabled = true;
            IsSiteEnabled = false;
            IsDeviceEnabled = false;

            // Accordingly load ONLY the client combo box content
            dbModel.Clients.ToList().ForEach(client => Clients.Add(client.NAME));

            // Display commands
            DisplayDocumentsInGridCommand = new TemplateCommand(RefreshDocuments, param => this.canExecuteCommand);
            ShowDocumentPreviewCommand = new TemplateCommand(ShowDocumentPreviewForm, param => this.canExecuteCommand);
            ToggleTemplateStatusCommand = new TemplateCommand(ToggleTemplateStatus, param => this.canExecuteCommand);

            // Addition commands
            AddDocumentCommand = new TemplateCommand(AddDocument, param => this.canExecuteCommand);

            // ComboBox manipulation commands
            AutofillComboBoxCommand = new TemplateCommand(FillComboBoxAutomatically, param => this.canExecuteCommand);
            ClearComboBoxCommand = new TemplateCommand(ClearComboBox, param => this.canExecuteCommand);
            FillComboBoxCommand = new TemplateCommand(FillComboBox, param => this.canExecuteCommand);
        }

        // METHODS
        #region METHODS
        #region Grid loading methods
        //TODO: Add refresh button( next to + )
        private void RefreshTemplates(object commandParameter)
        {
            Templates.Clear();

            dbModel.Templates.ToList().ForEach(Templates.Add);
        }

        //TODO: Refactor filtering
        private void RefreshDocuments(object commandParameter)
        {
            List<Document> filteredDocs = new List<Document>();

            string templateType = (SelectedTemplate as Template) != null ? (SelectedTemplate as Template).TYPE : string.Empty;

            if (SelectedTemplate != null)
            {
                filteredDocs = dbModel.Documents.Where(d => d.Template.TYPE.Equals(templateType)).ToList();
                ApplyFilter(filteredDocs);

                if (!string.IsNullOrEmpty(SelectedClient))
                {
                    filteredDocs = filteredDocs.Where(d => d.Device.Site.Client.NAME.Equals(SelectedClient)).ToList();
                    ApplyFilter(filteredDocs);

                    if (!string.IsNullOrEmpty(SelectedSite))
                    {
                        filteredDocs = filteredDocs.Where(d => d.Device.Site.NAME.Equals(SelectedSite)).ToList();
                        ApplyFilter(filteredDocs);

                        if (!string.IsNullOrEmpty(SelectedDevice))
                        {
                            filteredDocs = filteredDocs.Where(d => (d.Device.DeviceModel.DEVICE_NUM_PREFIX + d.Device.DEVICE_NUM_POSTFIX).Equals(SelectedDevice)).ToList();
                            ApplyFilter(filteredDocs);
                        }
                    }
                }
            }

            Documents.OrderByDescending(document => document.END_DATE);
        }

        private void ApplyFilter(List<Document> filteredDocs)
        {
            Documents.Clear();

            filteredDocs.ForEach(Documents.Add);
        }
        #endregion

        #region ComboBox related methods
        private void FillComboBoxAutomatically(object commandParameter)
        {
            if (selectedDevice != null)
            {
                SelectedClient = selectedDevice.Site.Client.NAME;
                SelectedSite = selectedDevice.Site.NAME;
                SelectedDevice = selectedDevice.DeviceModel.DEVICE_NUM_PREFIX + selectedDevice.DEVICE_NUM_POSTFIX;

                IsClientEnabled = false;
                IsSiteEnabled = false;
                IsDeviceEnabled = false;
            }
        }

        private void FillComboBox(object formIdentifier)
        {
            switch (formIdentifier as string)
            {
                case "client":
                    IsSiteEnabled = true;
                    Sites.Clear();
                    dbModel.Sites.ToList().Where(site => site.Client.NAME == SelectedClient).ToList().ForEach(s => Sites.Add(s.NAME));
                    break;
                case "site":
                    IsDeviceEnabled = true;
                    Devices.Clear();
                    dbModel.Devices.ToList().Where(device => device.Site.NAME == SelectedSite).ToList().ForEach(d => Devices.Add(d.DeviceModel.DEVICE_NUM_PREFIX + d.DEVICE_NUM_POSTFIX));
                    break;
                case "device":
                    break;
                default:
                    // Reload clients
                    Clients.Clear();
                    dbModel.Clients.ToList().ForEach(client => Clients.Add(client.NAME));
                    break;
            }

            RefreshDocuments(null);
        }

        private void ClearComboBox(object commandParamater)
        {
            SelectedClient = null;
            SelectedSite = null;
            SelectedDevice = null;

            IsClientEnabled = true;
        }
        #endregion

        #region Document manipulation methods
        private async void AddDocument(object commandParameter)
        {
            if(SelectedTemplate == null)
            {
                await placeholder.ShowMessageAsync("ГРЕШКА","Няма избран шаблон!");
                return;
            }

            Document document = new Document();
            document.Template = SelectedTemplate as Model.Template;
            document.Device = dbModel.Devices.Where(d => (d.DeviceModel.DEVICE_NUM_PREFIX+ d.DEVICE_NUM_POSTFIX).Equals(SelectedDevice)).FirstOrDefault();
            document.Device.Site = dbModel.Sites.Where(s => s.NAME.Equals(SelectedSite)).FirstOrDefault();
            document.Device.Site.Client = dbModel.Clients.Where(client => client.NAME.Equals(SelectedClient)).FirstOrDefault();
            document.START_DATE = DateTime.Today;
            document.END_DATE = document.START_DATE.AddYears(1);

            XmlDocument template = new XmlDocument();
            template.LoadXml(document.Template.TEMPLATE_CONTENT);

            int internalId = dbModel.Documents.Where(x => x.Template.TYPE.Equals(document.Template.TYPE)).Count();
            internalId++;

            string[] serviceProfile = PathFinder.FetchServiceProfile();

            switch (document.Template.TYPE)
            {
                case "Договор":
                    XmlDataFiller.FillContractXml(document, template, internalId, serviceProfile);
                    break;
                case "Свидетелство":
                    XmlDataFiller.FillCertificateXml(document, template, internalId, serviceProfile);
                    break;
                case "Протокол":
                    XmlDataFiller.FillProtocolXml(document, template, internalId, serviceProfile);
                    break;
                default:
                    break;
            }
            document.DOC = template.OuterXml;

            dbModel.Documents.Add(document);
            dbModel.SaveChanges();

            RefreshDocuments(null);
        }

        private void ShowDocumentPreviewForm(object commandParameter)
        {
            Document documentToPreview = dbModel.Documents.Find((SelectedDocument as Document).ID);
            Template templateToBuildFrom = (SelectedTemplate as Template);

            MSWordDocumentGenerator.BuildWordDocumentFromTemplate(documentToPreview, templateToBuildFrom);
        }

        private async void ToggleTemplateStatus(object commandParameter)
        {
            if(SelectedTemplate == null)
            {
               await placeholder.ShowMessageAsync("ГРЕШКА", "Няма избран шаблон!");
                return;
            }

            string templateType = (SelectedTemplate as Template).TYPE;
            string templateStatus = (SelectedTemplate as Template).STATUS.Equals("ЗАДЪЛЖИТЕЛЕН") ? "НЕЗАДЪЛЖИТЕЛЕН" : "ЗАДЪЛЖИТЕЛЕН";

            dbModel.Templates.Where(template => template.TYPE.Equals(templateType)).FirstOrDefault().STATUS = templateStatus;
            try
            {
                dbModel.SaveChanges();
                RefreshTemplates(null);
            }
            catch (Exception e)
            {
                await placeholder.ShowMessageAsync("ПРОБЛЕМ С БД", "Несъответствие с база данни: "+ e.InnerException.InnerException.Message);
            }   
        }
        #endregion
        #endregion

        // COMMANDS
        #region COMMANDS
        #region Template commands
        private ICommand _toggleTemplateStatusCommand;
        public ICommand ToggleTemplateStatusCommand
        {
            get { return _toggleTemplateStatusCommand; }
            set { _toggleTemplateStatusCommand = value; }
        }
        #endregion

        #region Document commands
        private ICommand _addDocumentCommand;
        public ICommand AddDocumentCommand
        {
            get { return _addDocumentCommand; }
            set { _addDocumentCommand = value; }
        }

        private ICommand _showDocumentPreviewCommand;
        public ICommand ShowDocumentPreviewCommand
        {
            get { return _showDocumentPreviewCommand; }
            set { _showDocumentPreviewCommand = value; }
        }

        private ICommand _displayDocumentsInGridCommand;
        public ICommand DisplayDocumentsInGridCommand
        {
            get { return _displayDocumentsInGridCommand; }
            set { _displayDocumentsInGridCommand = value; }
        }
        #endregion

        #region ComboBox commands
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
        #endregion
        #endregion

        // SELECTION PROPERTIES (CURRENT CONTEXT)
        #region SELECTION PROPERTIES (CURRENT CONTEXT)
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

        // PROPERTIES
        #region PROPERTIES 
        #region ComboBox enable/disable properties
        private bool _isClientEnabled;
        public bool IsClientEnabled
        {
            get { return _isClientEnabled; }
            set { _isClientEnabled = value; NotifyPropertyChanged(); }
        }

        private bool _isSiteEnabled;
        public bool IsSiteEnabled
        {
            get { return _isSiteEnabled; }
            set { _isSiteEnabled = value; NotifyPropertyChanged(); }
        }

        private bool _isDeviceEnabled;
        public bool IsDeviceEnabled
        {
            get { return _isDeviceEnabled; }
            set { _isDeviceEnabled = value; NotifyPropertyChanged(); }
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
