namespace PaymentsService.UseCases.ProcessPayment
{
    public record ProcessPaymentResult(bool Success, string? Reason = null);

}
