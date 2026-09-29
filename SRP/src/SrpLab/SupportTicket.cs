namespace SrpLab;

/// <summary>Owns the ticket identity and the current customer conversation.</summary>
public sealed class SupportTicket
{
    public string Id { get; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public DateTimeOffset OpenedAt { get; }
    public string Priority { get; private set; } = "P3";

    public SupportTicket(string id, string subject, string body, DateTimeOffset openedAt)
    {
        Id = id; Subject = subject; Body = body; OpenedAt = openedAt; RecalculatePriorityFromText();
    }
    public void AppendCustomerMessage(string text)
    {
        Body += "\n---\n" + text;
        RecalculatePriorityFromText();
    }
    public void RecalculatePriorityFromText() => Priority = TicketPriorityClassifier.Classify(Subject, Body);
    public DateTimeOffset SlaDeadline() => SupportSlaPolicy.Deadline(this);
    public bool IsBreached(DateTimeOffset now) => now > SlaDeadline();
    public string DraftPublicReply(string agentName) => PublicReplyFormatter.Format(this, agentName);
    public string InternalEscalationBlurb() => EscalationBlurbFormatter.Format(this);
}

public static class TicketPriorityClassifier
{
    public static string Classify(string subject, string body)
    {
        var blob = (subject + " " + body).ToLowerInvariant();
        if (blob.Contains("down") || blob.Contains("outage") || blob.Contains("cannot login")) return "P1";
        if (blob.Contains("urgent") || blob.Contains("asap") || blob.Contains("blocked")) return "P2";
        return "P3";
    }
}

public static class SupportSlaPolicy
{
    public static DateTimeOffset Deadline(SupportTicket ticket)
    {
        var hours = ticket.Priority switch { "P1" => 4, "P2" => 24, _ => 72 };
        return ticket.OpenedAt.AddHours(hours);
    }
}

public static class PublicReplyFormatter
{
    public static string Format(SupportTicket ticket, string agentName)
    {
        var apology = ticket.Priority == "P1" ? "We are treating this as a critical incident." : "Thanks for reaching out.";
        return $"Hi,\n{apology}\nTicket {ticket.Id} is with {agentName}. Next update before {ticket.SlaDeadline():u}.\n";
    }
}

public static class EscalationBlurbFormatter
{
    public static string Format(SupportTicket ticket)
    {
        return $"ESCALATE {ticket.Id} priority={ticket.Priority} breachAt={ticket.SlaDeadline():u} keywords-scanned=yes";
    }
}
