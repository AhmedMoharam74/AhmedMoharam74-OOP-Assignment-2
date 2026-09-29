namespace PatternsLab.Problems.Builder;

public sealed class CourseRegistration
{
    public string StudentEmail { get; }
    public string CourseCode { get; }
    public string AccessMode { get; }
    public string? GroupCode { get; }
    public string? DiscountCode { get; }
    public bool SendWhatsApp { get; }
    public bool SendEmailWelcome { get; }
    public string? MentorNote { get; }
    public DateOnly? PreferredStart { get; }

    internal CourseRegistration(string studentEmail, string courseCode, string accessMode, string? groupCode, string? discountCode, bool sendWhatsApp, bool sendEmailWelcome, string? mentorNote, DateOnly? preferredStart)
    {
        StudentEmail = studentEmail; CourseCode = courseCode; AccessMode = accessMode; GroupCode = groupCode;
        DiscountCode = discountCode; SendWhatsApp = sendWhatsApp; SendEmailWelcome = sendEmailWelcome;
        MentorNote = mentorNote; PreferredStart = preferredStart;
    }

    public override string ToString() => $"{StudentEmail} → {CourseCode} [{AccessMode}] group={GroupCode ?? "-"} discount={DiscountCode ?? "-"} wa={SendWhatsApp} mail={SendEmailWelcome}";
}

public sealed class CourseRegistrationBuilder
{
    private string? _studentEmail, _courseCode, _accessMode, _groupCode, _discountCode, _mentorNote;
    private bool _sendWhatsApp, _sendEmailWelcome;
    private DateOnly? _preferredStart;
    public CourseRegistrationBuilder ForStudent(string email) { _studentEmail = email; return this; }
    public CourseRegistrationBuilder ForCourse(string code) { _courseCode = code; return this; }
    public CourseRegistrationBuilder WithAccessMode(string mode) { _accessMode = mode; return this; }
    public CourseRegistrationBuilder InGroup(string group) { _groupCode = group; return this; }
    public CourseRegistrationBuilder WithDiscount(string? code) { _discountCode = code; return this; }
    public CourseRegistrationBuilder SendWhatsApp(bool enabled = true) { _sendWhatsApp = enabled; return this; }
    public CourseRegistrationBuilder SendEmailWelcome(bool enabled = true) { _sendEmailWelcome = enabled; return this; }
    public CourseRegistrationBuilder WithMentorNote(string? note) { _mentorNote = note; return this; }
    public CourseRegistrationBuilder StartingOn(DateOnly? date) { _preferredStart = date; return this; }

    public CourseRegistration Build()
    {
        if (string.IsNullOrWhiteSpace(_studentEmail)) throw new InvalidOperationException("Student email is required.");
        if (string.IsNullOrWhiteSpace(_courseCode)) throw new InvalidOperationException("Course code is required.");
        if (_accessMode is not ("LiveGroup" or "VideosOnly")) throw new InvalidOperationException("Access mode must be LiveGroup or VideosOnly.");
        if (_accessMode == "LiveGroup" && string.IsNullOrWhiteSpace(_groupCode)) throw new InvalidOperationException("LiveGroup requires a GroupCode.");
        if (_accessMode == "VideosOnly" && !string.IsNullOrWhiteSpace(_groupCode)) throw new InvalidOperationException("VideosOnly cannot have a GroupCode.");
        return new CourseRegistration(_studentEmail, _courseCode, _accessMode, _groupCode, _discountCode, _sendWhatsApp, _sendEmailWelcome, _mentorNote, _preferredStart);
    }
}

public static class RegistrationCallSites
{
    public static CourseRegistration CreateLiveStudentUgly() => new CourseRegistrationBuilder()
        .ForStudent("sara@mail.com").ForCourse("SEF-101").WithAccessMode("LiveGroup").InGroup("G1")
        .WithDiscount("EARLY10").SendWhatsApp().SendEmailWelcome().WithMentorNote("Needs evening slot")
        .StartingOn(new DateOnly(2026, 10, 1)).Build();

    public static CourseRegistration CreateVideosOnlyUgly() => new CourseRegistrationBuilder()
        .ForStudent("ali@mail.com").ForCourse("SEF-101").WithAccessMode("VideosOnly").SendEmailWelcome().Build();
}
