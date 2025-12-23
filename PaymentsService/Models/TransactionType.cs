namespace PaymentsService.Models
{
    /// <summary>
    /// Тип финансовой операции.
    /// </summary>
    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        Hold = 2,
        Charge = 3,
        Refund = 4
    }
}
