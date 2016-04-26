using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CashRegisterRepairs.Model;
using CashRegisterRepairs.ViewModel;

namespace CashRegisterRepairShop.ViewModel
{
    public class AddSiteViewModel : INotifyPropertyChanged, IViewModel
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

        public AddSiteViewModel()
        {
            SaveSiteCommand = new TemplateCommand(AddSiteToDB, param => this.canExecute);
        }

        private void AddSiteToDB(object obj)
        {
            Site site = new Site();
            site.NAME = SiteName;
            site.ADDRESS = SiteAddress;
            site.PHONE = SitePhone;

            // kak se pravi vruzkata sus Clienta?
            dbModel.Sites.Add(site);
            dbModel.SaveChanges();
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

    }
}

