using System;
using System.Xml;
using System.Linq;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CashRegisterRepairs.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Utilities.Helpers;
using CashRegisterRepairs.Utilities.GridDisplayObjects;
using System.Collections.Generic;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        // FIELDS
        #region FIELDS
        private readonly MetroWindow placeholder;
        private readonly CashRegisterServiceContext dbModel;
        public static DeviceDisplay selectedDevice;
        private bool canExecuteCommand = true; // command enable/disable
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

        private ObservableCollection<DocumentDisplay> _documents;
        public ObservableCollection<DocumentDisplay> Documents
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
            // Initialize DB context
            dbModel = new CashRegisterServiceContext();
            placeholder = App.Current.MainWindow as MetroWindow;

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
            DisplayDocumentsInGridCommand = new TemplateCommand(ShowDocumentsOfSelectedTemplate, param => this.canExecuteCommand);
            ShowDocumentPreviewCommand = new TemplateCommand(ShowDocumentPreviewForm, param => this.canExecuteCommand);

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
        private void ShowDocumentsOfSelectedTemplate(object commandParameter)
        {
            Documents.Clear();

            List<Document> filteredDocs = new List<Document>();

            foreach (Document document in dbModel.Documents)
            {
                string templateType = (SelectedTemplate as Template) != null ?(SelectedTemplate as Template).TYPE : string.Empty;

                if (SelectedTemplate != null && document.Template.TYPE.Equals(templateType))
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
            }

            Documents.OrderByDescending(document => document.END_DATE);
        }

        private void ApplyFilter(List<Document> filteredDocs)
        {
            Documents.Clear();

            foreach (Document foundDoc in filteredDocs)
            {
                DocumentDisplay docDisplay = new DocumentDisplay(foundDoc, foundDoc.Device.Site.Client, foundDoc.Device.Site, foundDoc.Device);
                Documents.Add(docDisplay);
            }
        }
        #endregion

        #region ComboBox related methods
        private void FillComboBoxAutomatically(object commandParameter)
        {
            if (selectedDevice != null)
            {
                Device actualDeviceObject = dbModel.Devices.Find(selectedDevice.ID);

                SelectedClient = selectedDevice.CLIENT_NAME;
                SelectedSite = selectedDevice.SITE_NAME;
                SelectedDevice = actualDeviceObject.DeviceModel.DEVICE_NUM_PREFIX + selectedDevice.DEVICE_NUM_POSTFIX;

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
                default:
                    break;
            }

            ShowDocumentsOfSelectedTemplate(null);
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
        // TODO: Figure out of async await causes trouble
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
            document.END_DATE = document.START_DATE.AddYears(1); //document.END_DATE = document.START_DATE.AddYears(1); 

            XmlDocument template = new XmlDocument();
            template.LoadXml(document.Template.TEMPLATE_CONTENT);

            int hackedId = dbModel.Documents.Where(x => x.Template.TYPE.Equals(document.Template.TYPE)).Count();
            hackedId++;

            string[] serviceProfileItems = PathFinder.FetchServiceProfile();

            switch (document.Template.TYPE)
            {
                case "Договор":
                    FillContractXml(document, template, hackedId, serviceProfileItems);
                    break;
                case "Свидетелство":
                    FillCertificateXml(document, template, hackedId, serviceProfileItems);
                    break;
                case "Протокол":
                    FillProtocolXml(document, template, hackedId, serviceProfileItems);
                    break;
                default:
                    break;
            }
            document.DOC = template.OuterXml;

            dbModel.Documents.Add(document);
            dbModel.SaveChanges();

            ShowDocumentsOfSelectedTemplate(null);
        }

        // TODO: Extract all these to Helper -> XmlDataFiller
        private void FillProtocolXml(Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            throw new NotImplementedException();
        }

        private void FillCertificateXml(Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            //Client
            template.SelectSingleNode("CertificateTemplate/Title/CurrDate").InnerText = DateTime.Today.ToShortDateString();
            template.SelectSingleNode("CertificateTemplate/Bulstat/Value").InnerText = document.Device.Site.Client.BULSTAT;
            template.SelectSingleNode("CertificateTemplate/EGN/Value").InnerText = document.Device.Site.Client.EGN;
            template.SelectSingleNode("CertificateTemplate/Owner/Client/Value").InnerText = document.Device.Site.Client.NAME;
            template.SelectSingleNode("CertificateTemplate/Owner/Address/Value").InnerText = document.Device.Site.Client.ADDRESS;
            template.SelectSingleNode("CertificateTemplate/Owner/Manager/Value").InnerText = document.Device.Site.Client.Manager.NAME;
            
            //Site
            template.SelectSingleNode("CertificateTemplate/Owner/Site/Value").InnerText = document.Device.Site.NAME;

            //Device
            template.SelectSingleNode("CertificateTemplate/Device/Model/Value").InnerText = document.Device.DeviceModel.MODEL;
            template.SelectSingleNode("CertificateTemplate/Device/Certificate/Value").InnerText = document.Device.DeviceModel.CERTIFICATE;
            template.SelectSingleNode("CertificateTemplate/Device/DeviceNum/Value").InnerText = document.Device.DeviceModel.DEVICE_NUM_PREFIX + document.Device.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("CertificateTemplate/Device/FiscalNum/Value").InnerText = document.Device.DeviceModel.FISCAL_NUM_PREFIX + document.Device.FISCAL_NUM_POSTFIX;

            //Service
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/BulstatAndName/Value").InnerText = serviceProfileItems[0] + serviceProfileItems[1];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/AddressAndPhone/Value").InnerText = serviceProfileItems[2] + serviceProfileItems[4];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/ServiceManager/Value").InnerText = serviceProfileItems[3];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/Value").InnerText = hackedId.ToString();
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/StartDate/Value").InnerText = document.START_DATE.ToShortDateString();

            //NAP info
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPNumber/Value").InnerText = document.Device.NAP_NUMBER;
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPDate/Value").InnerText = document.Device.NAP_DATE.ToShortDateString();
        }

        private void FillContractXml(Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            // Title
            template.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText = hackedId.ToString();
            template.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText = DateTime.Today.Date.ToShortDateString();
            template.SelectSingleNode("ContractTemplate/FreeText/Value").InnerText = DateTime.Today.Date.ToShortDateString();

            // Service
            template.SelectSingleNode("ContractTemplate/Service/ServiceName/Value").InnerText = serviceProfileItems[0];
            template.SelectSingleNode("ContractTemplate/Service/ServiceManager/Value").InnerText = serviceProfileItems[2];

            // Client
            template.SelectSingleNode("ContractTemplate/Client/ClientName/Value").InnerText = document.Device.Site.Client.NAME;
            template.SelectSingleNode("ContractTemplate/Client/ClientBulstat/Value").InnerText = document.Device.Site.Client.BULSTAT;
            template.SelectSingleNode("ContractTemplate/Client/ClientAddress/Value").InnerText = document.Device.Site.Client.ADDRESS;
            template.SelectSingleNode("ContractTemplate/Client/ClientManager/Value").InnerText = document.Device.Site.Client.Manager.NAME;

            // Device
            //Site site = dbModel.Sites.Where(s => s.NAME.Equals(SelectedSite)).FirstOrDefault();
            //Device device = dbModel.Devices.Where(d => (d.DeviceModel.DEVICE_NUM_PREFIX + d.DEVICE_NUM_POSTFIX).Equals(SelectedDevice)).FirstOrDefault();
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceModel/Value").InnerText = document.Device.DeviceModel.MODEL;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceNumber/Value").InnerText = document.Device.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/FiskalNumber/Value").InnerText = document.Device.FISCAL_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/Value").InnerText = "5500 лв.";

            // Contract
            template.SelectSingleNode("ContractTemplate/ContractText/Text/StartDate").InnerText = document.START_DATE.ToShortDateString();
            template.SelectSingleNode("ContractTemplate/ContractText/Text/EndDate").InnerText = document.END_DATE.ToShortDateString();

            // Annex 1
            template.SelectSingleNode("ContractTemplate/ContractText/ServiceAddres/Value").InnerText = serviceProfileItems[3];
            template.SelectSingleNode("ContractTemplate/ContractText/ServicePhone/Value").InnerText = serviceProfileItems[4];
        }

        private void ShowDocumentPreviewForm(object templatesDataContext)
        {
            Document documentToPreview = dbModel.Documents.Find((SelectedDocument as DocumentDisplay).ID);
            Template selectedTemplate = (SelectedTemplate as Template);

            MSWordDocumentGenerator.BuildWordDocumentFromTemplate(documentToPreview, selectedTemplate);
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

        #region Template commands
        private ICommand _addTemplateCommand;
        public ICommand AddTemplateCommand
        {
            get { return _addTemplateCommand; }
            set { _addTemplateCommand = value; }
        }
        #endregion

        #region Document commands
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

        private ICommand _viewDocumentCommand;
        public ICommand ShowDocumentPreviewCommand
        {
            get { return _viewDocumentCommand; }
            set { _viewDocumentCommand = value; }
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
        private string _documentContent;
        public string DocumentContent
        {
            get { return _documentContent; }
            set { _documentContent = value; NotifyPropertyChanged(); }
        }

        #region Focus properties
        private bool _focusSaveButton = true;
        public bool FocusSaveButton
        {
            get { return _focusSaveButton; }
            set { _focusSaveButton = value; NotifyPropertyChanged(); }
        }

        private bool _focusCommitButton = false;
        public bool FocusCommitButton
        {
            get { return _focusCommitButton; }
            set { _focusCommitButton = value; NotifyPropertyChanged(); }
        }
        #endregion
        
        #region ComboBox enable/disable properties
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
