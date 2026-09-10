namespace EggERP.Application;

public static class PaymentValidation
{
    public static void EnsureValid(string paymentMethod, string? referenceNumber)
    {
        var isCash = string.Equals(paymentMethod?.Trim(), "Cash", StringComparison.OrdinalIgnoreCase);

        if (!isCash && string.IsNullOrWhiteSpace(referenceNumber))
        {
            throw new InvalidOperationException(
                $"A reference number is required for payment method '{paymentMethod}'.");
        }
    }
}