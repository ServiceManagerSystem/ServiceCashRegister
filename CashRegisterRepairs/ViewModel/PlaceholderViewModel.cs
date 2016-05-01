using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using CashRegisterRepairShop.ViewModel;

namespace CashRegisterRepairs.ViewModel
{
    public class PlaceholderViewModel : INotifyPropertyChanged,IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private ObservableCollection<IViewModel> _tabViewModels;
        public ObservableCollection<IViewModel> TabViewModels { get { return _tabViewModels; } }

        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; NotifyPropertyChanged(); }
        }

        public PlaceholderViewModel()
        {
            // Initialize VMs of all TABS
            _tabViewModels = new ObservableCollection<IViewModel>();
            _tabViewModels.Add(new ClientsSitesViewModel());
            _tabViewModels.Add(new ModelsDevicesViewModel(this));
            _tabViewModels.Add(new TemplatesDocumentsViewModel());
            _tabViewModels.Add(new ServiceInfoViewModel());
        }

    }
}
