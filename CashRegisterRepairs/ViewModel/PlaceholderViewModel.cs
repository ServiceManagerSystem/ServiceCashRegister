using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CashRegisterRepairs.Utilities.Helpers;

namespace CashRegisterRepairs.ViewModel
{
    public class PlaceholderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<IViewModel> _tabViewModels;
        public ObservableCollection<IViewModel> TabViewModels { get { return _tabViewModels; } }

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

        private void RemoveTempDocs(object commandParameter)
        {
            string[] tempDocs = Directory.GetFiles(PathFinder.temporaryDocumentsPath);

            tempDocs.ToList().ForEach(temp => File.Delete(temp));

            MSWordDocumentGenerator.CloseMSWord();
        }

        private ICommand _removeTempDocsCommand;
        public ICommand RemoveTempDocsCommand
        {
            get { return _removeTempDocsCommand; }
            set { _removeTempDocsCommand = value; }
        }

        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; NotifyPropertyChanged(); }
        }

        private bool canExecuteCommand = true;
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
