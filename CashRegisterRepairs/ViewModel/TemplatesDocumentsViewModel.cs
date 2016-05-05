using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Office.Interop.Word;
using System.Xml;

namespace CashRegisterRepairs.ViewModel
{
    public class TemplatesDocumentsViewModel : INotifyPropertyChanged, IViewModel
    {
        // FIELDS
        #region FIELDS
        private readonly CashRegisterServiceContext dbModel;
        public static DeviceDisplay selectedDevice;
        private bool canExecuteCommand = true; // command enable/disable
        #endregion

        // COLLECTIONS
        #region COLLECTIONS
        #region Grid filling collections
        private ObservableCollection<Model.Template> _templates;
        public ObservableCollection<Model.Template> Templates
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

            // Initialize backing datagrid collections
            _templates = new ObservableCollection<Model.Template>(dbModel.Templates);
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

            foreach (Model.Document doc in dbModel.Documents)
            {
                if (SelectedTemplate != null && doc.Template.TYPE.Equals((SelectedTemplate as Model.Template).TYPE))
                {
                    if (SelectedClient == null)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.ID.Equals(doc.Client.ID)).FirstOrDefault();
                        if (site != null)
                        {
                            Device device = dbModel.Devices.Where(dev => dev.Site.ID.Equals(site.ID)).FirstOrDefault();
                            if (device != null)
                            {
                                DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);
                                Documents.Add(docDisplay);
                            }
                        }
                    }
                    else if (doc.Client.NAME == SelectedClient)
                    {
                        Site site = dbModel.Sites.Where(si => si.Client.NAME.Equals(SelectedClient)).FirstOrDefault();
                        Device device = dbModel.Devices.Where(dev => dev.Site.ID.Equals(site.ID)).FirstOrDefault();
                        DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, site, device);

                        Documents.Add(docDisplay);
                    }
                }
            }

            Documents.OrderByDescending(document => document.END_DATE);
        }

        private void LoadDocumentInGrid(Model.Document doc)
        {
            Device device = dbModel.Devices.Where(dev => (dev.DeviceModel.DEVICE_NUM_PREFIX + dev.DEVICE_NUM_POSTFIX) == SelectedDevice).FirstOrDefault();
            DocumentDisplay docDisplay = new DocumentDisplay(doc, doc.Client, device.Site, device);

            Documents.Add(docDisplay);
        }
        #endregion

        #region ComboBox related methods
        private void FillComboBoxAutomatically(object commandParameter)
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

        private void ClearComboBox(object commandParamater)
        {
            SelectedClient = null;
            SelectedSite = null;
            SelectedDevice = null;

            IsClientEnabled = true;
        }

        private void FillComboBox(object formIdentifier)
        {
            switch (formIdentifier as string)
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

            ShowDocumentsOfSelectedTemplate(null);
        }
        #endregion

        #region Document manipulation  methods
        private void AddDocument(object commandParameter)
        {
            if(SelectedTemplate == null)
            {
                // TODO: Show error
                return;
            }

            Model.Document document = new Model.Document();
            document.Template = SelectedTemplate as Model.Template;
            document.Client = dbModel.Clients.Where(client => client.NAME.Equals(SelectedClient)).FirstOrDefault();
            document.START_DATE = DateTime.Today;
            document.END_DATE = document.START_DATE.Value.AddYears(1);

            XmlDocument template = new XmlDocument();
            template.LoadXml(document.Template.TEMPLATE_CONTENT);

            int hackedId = dbModel.Documents.Where(x => x.Template.TYPE.Equals(document.Template.TYPE)).Count();
            hackedId++;

            string[] serviceProfileItems = ExtractionHelper.FetchServiceProfile();

            switch (document.Template.TYPE)
            {
                case "Договор":
                    FillContract(document, template, hackedId, serviceProfileItems);
                    break;
                case "Свидетелство":
                    FillCertificate(document, template, hackedId, serviceProfileItems);
                    break;
                case "Протокол":
                    FillProtocol(document, template, hackedId, serviceProfileItems);
                    break;
                default:
                    break;
            }

            document.DOC = template.OuterXml;

            dbModel.Documents.Add(document);
            dbModel.SaveChanges();

            ShowDocumentsOfSelectedTemplate(null);
        }

        private void FillProtocol(Model.Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            throw new NotImplementedException();
        }

        private void FillCertificate(Model.Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            //Client
            template.SelectSingleNode("CertificateTemplate/Title/CurrDate").InnerText = DateTime.Today.ToString();
            template.SelectSingleNode("CertificateTemplate/Bulstat/Value").InnerText = document.Client.BULSTAT;
            template.SelectSingleNode("CertificateTemplate/EGN/Value").InnerText = document.Client.EGN;
            template.SelectSingleNode("CertificateTemplate/Owner/Client/Value").InnerText = document.Client.NAME;
            template.SelectSingleNode("CertificateTemplate/Owner/Address/Value").InnerText = document.Client.ADDRESS;
            template.SelectSingleNode("CertificateTemplate/Owner/Manager/Value").InnerText = document.Client.Manager.NAME;
            //Site
            Site site = dbModel.Sites.Where(s => s.CLIENT_ID == document.Client.ID).FirstOrDefault();
            template.SelectSingleNode("CertificateTemplate/Owner/Site/Value").InnerText = site.NAME;

            //Device
            Device dev = dbModel.Devices.Where(d => d.SITE_ID == site.ID).FirstOrDefault();
            template.SelectSingleNode("CertificateTemplate/Device/Model/Value").InnerText = dev.DeviceModel.MODEL;
            template.SelectSingleNode("CertificateTemplate/Device/Certficate/Value").InnerText = dev.DeviceModel.CERTIFICATE;
            template.SelectSingleNode("CertificateTemplate/Device/DeviceNum/Value").InnerText = dev.DeviceModel.DEVICE_NUM_PREFIX + dev.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("CertificateTemplate/Device/FiscalNum/Value").InnerText = dev.DeviceModel.FISCAL_NUM_PREFIX + dev.FISCAL_NUM_POSTFIX;

            //Service
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/BulstatAndName/Value").InnerText = serviceProfileItems[0] + serviceProfileItems[1];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/AddressAndPhone/Value").InnerText = serviceProfileItems[3] + serviceProfileItems[4];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Manager/Value").InnerText = serviceProfileItems[2];
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/Value").InnerText = hackedId.ToString();
            template.SelectSingleNode("CertificateTemplate/ServiceInfo/Contract/StartDate/Value").InnerText = document.START_DATE.ToString();

            //NAP info
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPNumber/Value").InnerText = dev.NAP_NUMBER;
            template.SelectSingleNode("CertificateTemplate/NAPInfo/NAPDate/Value").InnerText = dev.NAP_DATE.ToString();
        }

        private void FillContract(Model.Document document, XmlDocument template, int hackedId, string[] serviceProfileItems)
        {
            // Title
            template.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText = hackedId.ToString();
            template.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText = DateTime.Today.Date.ToString();
            template.SelectSingleNode("ContractTemplate/FreeText/Value").InnerText = DateTime.Today.Date.ToString();

            // Service
            template.SelectSingleNode("ContractTemplate/Service/ServiceName/Value").InnerText = serviceProfileItems[0];
            template.SelectSingleNode("ContractTemplate/Service/ServiceManager/Value").InnerText = serviceProfileItems[2];

            // Client
            template.SelectSingleNode("ContractTemplate/Client/ClientName/Value").InnerText = document.Client.NAME;
            template.SelectSingleNode("ContractTemplate/Client/ClientBulstat/Value").InnerText = document.Client.BULSTAT;
            template.SelectSingleNode("ContractTemplate/Client/ClientAddress/Value").InnerText = document.Client.ADDRESS;
            template.SelectSingleNode("ContractTemplate/Client/ClientManager/Value").InnerText = document.Client.Manager.NAME;

            // Device
            Site site = dbModel.Sites.Where(s => s.NAME.Equals(SelectedSite)).FirstOrDefault();
            Device device = dbModel.Devices.Where(d => (d.DeviceModel.DEVICE_NUM_PREFIX + d.DEVICE_NUM_POSTFIX).Equals(SelectedDevice)).FirstOrDefault();
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceModel/Value").InnerText = device.DeviceModel.MODEL;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/DeviceNumber/Value").InnerText = device.DEVICE_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/FiskalNumber/Value").InnerText = device.FISCAL_NUM_POSTFIX;
            template.SelectSingleNode("ContractTemplate/DeviceInfo/Value").InnerText = "5500 лв.";

            // Contract
            template.SelectSingleNode("ContractTemplate/ContractText/Text/StartDate").InnerText = document.START_DATE.Value.ToString();
            template.SelectSingleNode("ContractTemplate/ContractText/Text/EndDate").InnerText = document.END_DATE.Value.ToString();

            // Annex 1
            template.SelectSingleNode("ContractTemplate/ContractText/ServiceAddres/Value").InnerText = serviceProfileItems[3];
            template.SelectSingleNode("ContractTemplate/ContractText/ServicePhone/Value").InnerText = serviceProfileItems[4];
        }

        private void ShowDocumentPreviewForm(object templatesDataContext)
        {
            Model.Document previewDoc = dbModel.Documents.Find((SelectedDocument as DocumentDisplay).ID);

            XmlDocument filledDocument = new XmlDocument();
            filledDocument.LoadXml(previewDoc.DOC);

            //OBJECT OF MISSING "NULL VALUE"
            object oMissing = System.Reflection.Missing.Value;
            object oTemplatePath = ExtractionHelper.ResolveAppPath()+@"\Resources\WordTemplates\ContractTemplate.dotx";

            Application wordApp = new Application();
            Microsoft.Office.Interop.Word.Document wordDoc = new Microsoft.Office.Interop.Word.Document();

            wordDoc = wordApp.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, ref oMissing);

            foreach (Field myMergeField in wordDoc.Fields)
            {
                Range rngFieldCode = myMergeField.Code;

                rngFieldCode.Font.Name = "Arial";
                rngFieldCode.Font.Size = 11;
                rngFieldCode.Font.Bold = -1;

                string fieldText = rngFieldCode.Text;

                // ONLY GETTING THE MAILMERGE FIELDS
                if (fieldText.StartsWith(" MERGEFIELD"))
                {
                    // THE TEXT COMES IN THE FORMAT OF
                    // MERGEFIELD  MyFieldName  \\* MERGEFORMAT
                    // THIS HAS TO BE EDITED TO GET ONLY THE FIELDNAME "MyFieldName"
                    int endMerge = fieldText.IndexOf("\\");
                    int fieldNameLength = fieldText.Length - endMerge;
                    string fieldName = fieldText.Substring(11, endMerge - 11);

                    // GIVES THE FIELDNAMES AS THE USER HAD ENTERED IN .dot FILE
                    fieldName = fieldName.Trim();

                    // **** FIELD REPLACEMENT IMPLEMENTATION GOES HERE ****//
                    // THE PROGRAMMER CAN HAVE HIS OWN IMPLEMENTATIONS HERE
                    //if (fieldName == "ContractNumber")
                    //{
                    //    myMergeField.Select();
                    //    wordApp.Selection.TypeText(filledDocument.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText);
                    //}

                    myMergeField.Select();

                    switch (fieldName)
                    {
                        case "ContractNumber":
                            wordApp.Selection.TypeText(filledDocument.SelectSingleNode("ContractTemplate/Title/ContractNumber").InnerText);
                            break;
                        case "Today":
                            wordApp.Selection.TypeText(filledDocument.SelectSingleNode("ContractTemplate/Title/CurrDate").InnerText);
                            break;
                        default:
                            break;
                    }

                }

            }
            wordDoc.SaveAs(ExtractionHelper.ResolveAppPath()+@"\Resources\TemporaryDocuments\myfile2.doc");
            wordApp.Documents.Open(ExtractionHelper.ResolveAppPath()+@"\Resources\TemporaryDocuments\myfile2.doc");
            //wordApp.Application.Quit();
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
