namespace CashRegisterRepairs.ViewModel.Interfaces
{
    interface IAdditionVM:IViewModel
    {
        void ClearFields();
        void SaveRecord(object obj);
        void CommitRecords(object obj);
    }
}
