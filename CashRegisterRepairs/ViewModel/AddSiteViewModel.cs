using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.ViewModel;
using CashRegisterRepairs.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace CashRegisterRepairShop.ViewModel
{
    public class AddSiteViewModel : INotifyPropertyChanged, IViewModel
    {
        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        private bool canExecute = true;
        private List<Site> siteStorage;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public AddSiteViewModel()
        {
            siteStorage = new List<Site>();
            SaveSiteCommand = new TemplateCommand(SaveSite, param => this.canExecute);
            CommitSiteCommand = new TemplateCommand(CommitSite, param => this.canExecute);
            EnableSubviewDisplay = new TemplateCommand(EnableSubvew, param => this.canExecute);
        }

        private void CommitSite(object obj)
        {
            siteStorage.ToList().ForEach(site => dbModel.Sites.Add(site));
            dbModel.SaveChanges();
        }

        private void EnableSubvew(object obj)
        {
            TransitionContext.EnableSubviewOpen();
        }

        private void SaveSite(object obj)
        {
            Site site = new Site();
            site.NAME = SiteName;
            site.ADDRESS = SiteAddress;
            site.PHONE = SitePhone;

            site.Client = dbModel.Clients.Find(TransitionContext.selectedClientIndex);
            //TransitionContext.ConsumeObjectsAfterUse(TransitionContext.selectedClient);

            siteStorage.Add(site);
        }

        private string _siteName = string.Empty;
        public string SiteName
        {
            get { return _siteName; }
            set { _siteName = value; NotifyPropertyChanged(); }
        }

        private string _siteAddress = string.Empty;
        public string SiteAddress
        {
            get { return _siteAddress; }
            set { _siteAddress = value; NotifyPropertyChanged(); }
        }

        private string _sitePhone = string.Empty;
        public string SitePhone
        {
            get { return _sitePhone; }
            set { _sitePhone = value; NotifyPropertyChanged(); }
        }

        private ICommand _saveSiteCommand;
        public ICommand SaveSiteCommand
        {
            get { return _saveSiteCommand; }
            set { _saveSiteCommand = value; }
        }

        private ICommand _enableSubviewDisplay;
        public ICommand EnableSubviewDisplay
        {
            get { return _enableSubviewDisplay; }
            set { _enableSubviewDisplay = value; }
        }

        private ICommand _commitSiteCommand;
        public ICommand CommitSiteCommand
        {
            get { return _commitSiteCommand; }
            set { _commitSiteCommand = value; }
        }
    }
}

