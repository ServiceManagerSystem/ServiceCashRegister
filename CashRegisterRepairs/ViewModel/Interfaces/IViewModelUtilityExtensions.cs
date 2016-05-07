namespace CashRegisterRepairs.ViewModel.Interfaces
{
    public interface IViewModelUtilityExtentions
    {
        void ClearCaches();
        void EnableSubview(object comingFromForm);
        void ShowAdditionCount(string formIdentifier);
    }
}
