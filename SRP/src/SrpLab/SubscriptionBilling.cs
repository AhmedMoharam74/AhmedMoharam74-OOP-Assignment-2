namespace SrpLab;

/// <summary>Owns subscription billing facts, proration, and failed-payment count.</summary>
public sealed class SubscriptionBilling
{
    public string CustomerId { get; }
    public decimal MonthlyPrice { get; }
    public DateOnly PeriodStart { get; }
    public DateOnly PeriodEnd { get; }
    public int FailedPayments { get; private set; }

    public SubscriptionBilling(string customerId, decimal monthlyPrice, DateOnly periodStart, DateOnly periodEnd)
    {
        CustomerId = customerId; MonthlyPrice = monthlyPrice; PeriodStart = periodStart; PeriodEnd = periodEnd;
    }
    public decimal Prorate(DateOnly activeFrom)
    {
        if (activeFrom <= PeriodStart) return MonthlyPrice;
        if (activeFrom >= PeriodEnd) return 0m;
        var totalDays = PeriodEnd.DayNumber - PeriodStart.DayNumber;
        if (totalDays <= 0) return MonthlyPrice;
        var used = PeriodEnd.DayNumber - activeFrom.DayNumber;
        return Math.Round(MonthlyPrice * used / totalDays, 2);
    }
    public string NextInvoiceNumber() => InvoiceNumberGenerator.Next(PeriodStart);
    public void RegisterFailedPayment() => FailedPayments++;
    public string DunningEmail(string customerName, DateOnly asOf) => DunningEmailFormatter.Format(this, customerName, asOf);
    public string LedgerJournalLine(DateOnly activeFrom) => SubscriptionLedgerExporter.Row(this, activeFrom);
}

public static class InvoiceNumberGenerator
{
    private static int _sequence = 1000;
    public static string Next(DateOnly periodStart)
    {
        var number = ++_sequence;
        return $"INV-{periodStart:yyyyMM}-{number:D5}";
    }
}

public static class DunningPolicy
{
    public static string SeverityFor(int failedPayments)
    {
        if (failedPayments <= 1) return "friendly reminder";
        if (failedPayments == 2) return "second notice";
        return "final notice before suspension";
    }
}

public static class DunningEmailFormatter
{
    public static string Format(SubscriptionBilling billing, string customerName, DateOnly asOf)
    {
        var amount = billing.Prorate(billing.PeriodStart);
        var invoice = billing.NextInvoiceNumber();
        var severity = DunningPolicy.SeverityFor(billing.FailedPayments);
        return $"Subject: {severity} {invoice}\nHi {customerName},\nBalance {amount:C} as of {asOf:o} ({billing.FailedPayments} failures).\n";
    }
}

public static class SubscriptionLedgerExporter
{
    public static string Row(SubscriptionBilling billing, DateOnly activeFrom)
    {
        return $"{billing.CustomerId},{billing.NextInvoiceNumber()},{billing.Prorate(activeFrom):0.00},AR-SUB";
    }
}
