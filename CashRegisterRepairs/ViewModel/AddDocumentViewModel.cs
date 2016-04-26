using System;
using System.ComponentModel;
using CashRegisterRepairs.ViewModel.Interfaces;
using CashRegisterRepairs.Model;

namespace CashRegisterRepairShop.ViewModel
{
    public class AddDocumentViewModel : INotifyPropertyChanged, IAdditionVM
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CashRegisterServiceContext dbModel = new CashRegisterServiceContext();

        public AddDocumentViewModel()
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
