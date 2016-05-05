using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using CashRegisterRepairs.Utilities;

namespace CashRegisterRepairs.ViewModel
{
    public class PlaceholderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<IViewModel> _tabViewModels;
        public ObservableCollection<IViewModel> TabViewModels { get { return _tabViewModels; } }

        private bool canExecuteCommand = true;

        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; NotifyPropertyChanged(); }
        }

        private ICommand _removeTempDocsCommand;
        public ICommand RemoveTempDocsCommand
        {
            get { return _removeTempDocsCommand; }
            set { _removeTempDocsCommand = value; }
        }

        private void RemoveTempDocs(object commandParameter)
        {
            string[] tempDocs = Directory.GetFiles(ExtractionHelper.ResolveAppPath() + @"\Resources\TemporaryDocuments\");

            foreach (string path in tempDocs)
            {
                File.Delete(path);
            }
        }

        public PlaceholderViewModel()
        {
            // Initialize VMs of all TABS
            _tabViewModels = new ObservableCollection<IViewModel>();
            _tabViewModels.Add(new ClientsSitesViewModel());
            _tabViewModels.Add(new ModelsDevicesViewModel(this));
            _tabViewModels.Add(new TemplatesDocumentsViewModel());
            _tabViewModels.Add(new ServiceInfoViewModel());

            RemoveTempDocsCommand = new TemplateCommand(RemoveTempDocs,param => this.canExecuteCommand);
        }

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
