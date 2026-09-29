namespace SrpLab;

/// <summary>
/// Course enrollment: capacity, waitlist math, welcome-packet markdown, and invoice lines.
/// </summary>
public sealed class CourseEnrollmentDesk
{
    private readonly HashSet<string> _seated = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _waitlist = new();
    public int Capacity { get; }
    public decimal Tuition { get; }
    public string CourseCode { get; }

    public CourseEnrollmentDesk(string courseCode, int capacity, decimal tuition)
    {
        CourseCode = courseCode;
        Capacity = capacity;
        Tuition = tuition;
    }

    public string Register(string studentEmail)
    {
        if (string.IsNullOrWhiteSpace(studentEmail)) throw new ArgumentException("email");
        var email = studentEmail.Trim();

        if (_seated.Contains(email) || _waitlist.Contains(email))
            return "ALREADY_REGISTERED";

        if (_seated.Count < Capacity)
        {
            _seated.Add(email);
            return "SEATED";
        }

        _waitlist.Add(email);
        return $"WAITLIST:{_waitlist.Count}";
    }

    public int WaitlistPosition(string studentEmail)
    {
        var idx = _waitlist.FindIndex(x => x.Equals(studentEmail, StringComparison.OrdinalIgnoreCase));
        return idx < 0 ? -1 : idx + 1;
    }

    internal bool IsSeated(string studentEmail) => _seated.Contains(studentEmail);
    public string WelcomePacketMarkdown(string studentEmail, string studentName) => WelcomePacketFormatter.Format(this, studentEmail, studentName);
    public string TuitionInvoiceLine(string studentEmail) => TuitionInvoiceFormatter.Format(this, studentEmail);

    public void PromoteFromWaitlist(int seats)
    {
        // Operational promotion policy mixed with messaging responsibilities above.
        while (seats > 0 && _waitlist.Count > 0 && _seated.Count < Capacity)
        {
            var next = _waitlist[0];
            _waitlist.RemoveAt(0);
            _seated.Add(next);
            seats--;
        }
    }
}

public static class WelcomePacketFormatter
{
    public static string Format(CourseEnrollmentDesk course, string studentEmail, string studentName)
    {
        var status = course.IsSeated(studentEmail) ? "confirmed seat" : $"waitlist #{course.WaitlistPosition(studentEmail)}";
        return $"# Welcome to {course.CourseCode}\nHi {studentName},\nYour status: **{status}**.\n" +
               $"Bring a laptop. Discord onboarding link: https://example.invalid/{course.CourseCode.ToLowerInvariant()}\n";
    }
}

public static class TuitionInvoiceFormatter
{
    public static string Format(CourseEnrollmentDesk course, string studentEmail)
    {
        if (!course.IsSeated(studentEmail)) return $"{course.CourseCode},WAITLIST,0.00";
        var vat = Math.Round(course.Tuition * 0.14m, 2);
        return $"{course.CourseCode},TUITION,{course.Tuition:0.00},VAT,{vat:0.00},TOTAL,{(course.Tuition + vat):0.00}";
    }
}
