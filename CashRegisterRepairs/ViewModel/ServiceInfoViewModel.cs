using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CashRegisterRepairs.ViewModel
{
    public class ServiceInfoViewModel : INotifyPropertyChanged, IViewModel
    {
        private readonly string serviceInfoConfigPath;
        string[] configEntries;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public ServiceInfoViewModel()
        {
            serviceInfoConfigPath = ResolveAppPath() + @"\Resources\Service.txt";
            LoadConfigFromFile();
            DisplayInfo();
        }

        private static string ResolveAppPath()
        {
            string assemblyPathNode = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string binPathNode = Directory.GetParent(assemblyPathNode).FullName;
            return Directory.GetParent(binPathNode).FullName;
        }

        private void LoadConfigFromFile()
        {
            string serviceInfoConf = File.ReadAllText(serviceInfoConfigPath);
            configEntries = serviceInfoConf.Split('\n');
        }

        private string _serName;
        public string SerName
        {
            get { return _serName; }
            set { _serName = value; NotifyPropertyChanged(); }
        }

        private string _serBulstat;
        public string SerBulstat
        {
            get { return _serBulstat; }
            set { _serBulstat = value; NotifyPropertyChanged(); }
        }

        private string _serManager;
        public string SerManager
        {
            get { return _serManager; }
            set { _serManager = value; NotifyPropertyChanged(); }
        }

        private string _serAddress;
        public string SerAddress
        {
            get { return _serAddress; }
            set { _serAddress = value; NotifyPropertyChanged(); }
        }

        private string _serPhone;
        public string SerPhone
        {
            get { return _serPhone; }
            set { _serPhone = value; NotifyPropertyChanged(); }
        }

        private void DisplayInfo()
        {
            SerName = configEntries[0];
            SerBulstat = configEntries[1];
            SerManager = configEntries[2];
            SerAddress = configEntries[3];
            SerPhone = configEntries[4];
        }
    }
}
