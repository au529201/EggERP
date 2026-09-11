namespace EggERP.Application;

public static class PaymentValidation
{
    public static void EnsureValid(string paymentMethod, string? referenceNumber, string? paymentSource)
    {
        var isCash = string.Equals(paymentMethod?.Trim(), "Cash", StringComparison.OrdinalIgnoreCase);

        if (isCash)
        {
            return;
        }

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            missing.Add("reference number");
        }

        if (string.IsNullOrWhiteSpace(paymentSource))
        {
            missing.Add("payment source (e.g. GCash, Maya, BDO, BPI)");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Payment method '{paymentMethod}' requires: {string.Join(" and ", missing)}.");
        }
    }
}