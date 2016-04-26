using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.ViewModel.Interfaces;

namespace CashRegisterRepairs.ViewModel
{
    public class AddModelViewModel : INotifyPropertyChanged, IAdditionVM
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

        public AddModelViewModel()
        {
            SaveModelCommand = new TemplateCommand(SaveRecord, param => this.canExecute);
            EnableSubviewDisplay = new TemplateCommand(EnableSubviews, param => this.canExecute);
        }

        public void ClearFields()
        {
            throw new NotImplementedException();
        }

        public void SaveRecord(object obj)
        {
            DeviceModel devModel = new DeviceModel();
            devModel.MANUFACTURER = ModelsManufacturer;
            devModel.MODEL = ModelsModel;
            devModel.CERTIFICATE = ModelsCertificate;
            devModel.DEVICE_NUM_PREFIX = ModelsNumber;
            devModel.FISCAL_NUM_PREFIX = ModelsFiscalNumber;

            // save to temp

        }

        public void CommitRecords(object obj)
        {
            //dbModel.DeviceModels.Add( .. );
            dbModel.SaveChanges();
        }

        private void EnableSubviews(object obj)
        {
            TransitionContext.EnableSubviewOpen();
        }

        private string _modelsManufacturer = string.Empty;
        public string ModelsManufacturer
        {
            get { return _modelsManufacturer; }
            set { _modelsManufacturer = value; NotifyPropertyChanged(); }
        }

        private string _modelsModel = string.Empty;
        public string ModelsModel
        {
            get { return _modelsModel; }
            set { _modelsModel = value; NotifyPropertyChanged(); }
        }

        private string _modelsCertificate = string.Empty;
        public string ModelsCertificate
        {
            get { return _modelsCertificate; }
            set { _modelsCertificate = value; NotifyPropertyChanged(); }
        }

        private string _modelsNumber = string.Empty;
        public string ModelsNumber
        {
            get { return _modelsNumber; }
            set { _modelsNumber = value; NotifyPropertyChanged(); }
        }

        private string _modelsFiscalNumber = string.Empty;
        public string ModelsFiscalNumber
        {
            get { return _modelsFiscalNumber; }
            set { _modelsFiscalNumber = value; NotifyPropertyChanged(); }
        }

        private ICommand _saveModelCommand;
        public ICommand SaveModelCommand
        {
            get { return _saveModelCommand; }
            set { _saveModelCommand = value; }
        }

        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }
    }
}
