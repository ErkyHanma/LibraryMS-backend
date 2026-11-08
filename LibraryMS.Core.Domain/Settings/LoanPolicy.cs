namespace LibraryMS.Core.Domain.Settings
{
    public class LoanPolicy
    {
        public int MaxActiveBorrows { get; set; } = 5;
        public int LoanDays { get; set; } = 14;
        public int PickupWindowDays { get; set; } = 3;
    }
}
