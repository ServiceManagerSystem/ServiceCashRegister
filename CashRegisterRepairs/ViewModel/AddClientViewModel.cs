using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.Utilities;
using CashRegisterRepairs.ViewModel.Interfaces;

namespace CashRegisterRepairs.ViewModel
{
    public class AddClientViewModel : INotifyPropertyChanged, IAdditionVM
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
        private List<Manager> managerStorage;
        private List<Client> clientStorage;

        public AddClientViewModel()
        {
            // Initialize local storage
            managerStorage = new List<Manager>();
            clientStorage = new List<Client>();

            // Initialize commands
            SaveClientAndManagerCommand = new TemplateCommand(SaveRecord, param => this.canExecute);
            CommitClientsAndManagersCommand = new TemplateCommand(CommitRecords, param => this.canExecute);
            EnableSubviewDisplay = new TemplateCommand(EnableSubviews, param => this.canExecute);
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

        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        public void ClearFields()
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

        public void SaveRecord(object obj)
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
        }

        public void CommitRecords(object obj)
        {
            managerStorage.ToList().ForEach(manager => dbModel.Managers.Add(manager));
            clientStorage.ToList().ForEach(client => dbModel.Clients.Add(client));
            dbModel.SaveChanges();
        }

        private void EnableSubviews(object obj)
        {
            TransitionContext.EnableSubviewOpen();
        }

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

    }

}
