namespace CashRegisterRepairs.ViewModel.Interfaces
{
    public interface IViewModelUtilityExtentions
    {
        void EnableSubview(object comingFromForm);
        void ShowAdditionCount(string formIdentifier);
        void SwapFocusToCommitButton();
        void ResetFocusToSaveButton();
    }
}
