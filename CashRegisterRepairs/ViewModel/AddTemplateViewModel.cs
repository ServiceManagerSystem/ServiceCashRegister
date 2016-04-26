using System;
using System.ComponentModel;
using CashRegisterRepairs.ViewModel.Interfaces;

namespace CashRegisterRepairShop.ViewModel
{
    public class AddTemplateViewModel : INotifyPropertyChanged, IAdditionVM
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AddTemplateViewModel()
        {

        }

        public void ClearFields()
        {
            throw new NotImplementedException();
        }

        public void CommitRecords(object obj)
        {
            throw new NotImplementedException();
        }

        public void SaveRecord(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
