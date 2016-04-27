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
        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        private bool canExecute = true;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

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

        public ClientsSitesViewModel()
        {
            // Initializing datagrid backing collections
            _sites = new ObservableCollection<Site>();
            Clients = new ObservableCollection<ClientDisplay>();
            LoadClientsToGrid();

            // Initialize commands
            DisplaySitesCommand = new TemplateCommand(ShowSitesForCLient, param => this.canExecute);
            AddClientCommand = new TemplateCommand(ShowClientAdditionForm, param => this.canExecute);
            AddSiteCommand = new TemplateCommand(ShowSiteAdditionForm, param => this.canExecute);
            AddDeviceCommand = new TemplateCommand(ShowDevicesAdditionForm, param => this.canExecute);
        }

        private void LoadClientsToGrid()
        {
            foreach (Client client in dbModel.Clients.ToList())
            {
                Manager manager = client.Manager;
                ClientDisplay clientDisplay = new ClientDisplay(client, manager);
                Clients.Add(clientDisplay);
            }
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

        private void ShowSitesForCLient(object obj)
        {
            // Get rid of previous displays
            Sites.Clear();

            // Avoid selection of empty row
            if (SelectedClient != null && (SelectedClient is ClientDisplay))
            {
                ClientDisplay selectedClientFromGrid = SelectedClient as ClientDisplay;
                List<Site> sitesFromDB = dbModel.Sites.ToList();

                // Add to Sites grid
                sitesFromDB.Where(site => site.CLIENT_ID == selectedClientFromGrid.ID).ToList().ForEach(Sites.Add);

                // Extract to helper may be
                if (Sites.Count == 0)
                {
                    MessageBox.Show("Няма обекти към този клиент!", "НОВ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ShowClientAdditionForm(object obj)
        {
            // Move on to view ( allow one instance at a time )
            if (TransitionContext.CanOpenSubview())
            {
                AddClientView addClientsView = new AddClientView();
                addClientsView.Show();
                TransitionContext.DisableSubviewOpen();
            }
        }

        private void ShowSiteAdditionForm(object obj)
        {
            // Move on to view ( allow one instance at a time )
            if (TransitionContext.CanOpenSubview())
            {
                TransitionContext.selectedClientIndex = (SelectedClient as ClientDisplay).ID;

                AddSiteView addSitesView = new AddSiteView();
                addSitesView.Show();
                TransitionContext.DisableSubviewOpen();
            }
        }

        private void ShowDevicesAdditionForm(object obj)
        {
            // Hook currently selected site to a transition context object
            TransitionContext.selectedSite = (SelectedSite as Site);

            // Move on to view ( allow one instance at a time )
            if (TransitionContext.CanOpenSubview())
            {
                AddDeviceView addDevicesView = new AddDeviceView();
                addDevicesView.Show();
                TransitionContext.DisableSubviewOpen();
            }

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

    }

}
