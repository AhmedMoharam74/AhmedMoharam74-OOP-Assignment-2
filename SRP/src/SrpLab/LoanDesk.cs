namespace SrpLab;

/// <summary>
/// Loan desk: eligibility arithmetic, required-document lists, and rejection letter prose.
/// </summary>
public sealed class LoanDesk
{
    public decimal RequestedAmount { get; }
    public int CreditScore { get; }
    public int EmploymentMonths { get; }
    public bool HasCollateral { get; }

    public LoanDesk(decimal requestedAmount, int creditScore, int employmentMonths, bool hasCollateral)
    {
        RequestedAmount = requestedAmount;
        CreditScore = creditScore;
        EmploymentMonths = employmentMonths;
        HasCollateral = hasCollateral;
    }

    public decimal RiskScore()
    {
        // Risk model will change with risk committee — not with letter templates.
        decimal score = 100m;
        score -= Math.Max(0, 700 - CreditScore) * 0.15m;
        if (EmploymentMonths < 6) score -= 20m;
        if (RequestedAmount > 50_000m && !HasCollateral) score -= 25m;
        if (RequestedAmount > 150_000m) score -= 10m;
        return Math.Clamp(score, 0m, 100m);
    }

    public bool IsEligible() => RiskScore() >= 55m && CreditScore >= 580;

    public IReadOnlyList<string> RequiredDocuments() => LoanDocumentPolicy.RequiredFor(this);
    public string DecisionLetter(string applicantName) => LoanDecisionLetterFormatter.Format(this, applicantName);
    public string UnderwriterCsvRow(string applicationId) => UnderwriterCsvExporter.Row(this, applicationId);
}

public static class LoanDocumentPolicy
{
    public static IReadOnlyList<string> RequiredFor(LoanDesk application)
    {
        var docs = new List<string> { "National ID", "Proof of income (3 months)" };
        if (application.RequestedAmount > 40_000m) docs.Add("Bank statements (6 months)");
        if (application.HasCollateral) docs.Add("Collateral ownership deed");
        if (application.EmploymentMonths < 12) docs.Add("Employer letter");
        if (!application.IsEligible()) docs.Add("Manual underwriter referral form");
        return docs;
    }
}

public static class LoanDecisionLetterFormatter
{
    public static string Format(LoanDesk application, string applicantName)
    {
        if (application.IsEligible())
            return $"Dear {applicantName},\nYour request for {application.RequestedAmount:C} is pre-approved (risk {application.RiskScore():0}).\n" +
                   $"Please upload: {string.Join("; ", application.RequiredDocuments())}.\n";
        return $"Dear {applicantName},\nWe are unable to approve {application.RequestedAmount:C} at this time.\n" +
               $"Reference risk={application.RiskScore():0}. You may reapply after improving documentation.\n";
    }
}

public static class UnderwriterCsvExporter
{
    public static string Row(LoanDesk application, string applicationId)
    {
        return $"{applicationId},{application.CreditScore},{application.EmploymentMonths},{(application.HasCollateral ? 1 : 0)},{application.RiskScore():0.00},{(application.IsEligible() ? "Y" : "N")}";
    }
}
