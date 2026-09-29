namespace SrpLab;

/// <summary>
/// Grade book: aggregation, letter-band policy, and transcript export.
/// </summary>
public sealed class GradeBook
{
    private readonly Dictionary<string, List<decimal>> _scores = new(StringComparer.OrdinalIgnoreCase);

    public void Record(string studentId, decimal score)
    {
        if (score is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(score));
        if (!_scores.TryGetValue(studentId, out var list))
        {
            list = new List<decimal>();
            _scores[studentId] = list;
        }
        list.Add(score);
    }

    public decimal Average(string studentId)
    {
        if (!_scores.TryGetValue(studentId, out var list) || list.Count == 0) return 0m;
        return Math.Round(list.Average(), 2);
    }

    public IReadOnlyList<string> StudentIds() => _scores.Keys.OrderBy(x => x).ToList();

    public string Letter(string studentId) => LetterGradePolicy.For(Average(studentId));

    public bool MeetsHonorRoll(string studentId) => HonorRollPolicy.Qualifies(Average(studentId), Letter(studentId));

    public string TranscriptPlain(string studentId, string fullName) => TranscriptFormatter.Plain(this, studentId, fullName);

    public string ExportCsv() => GradeCsvExporter.Export(this);
}

public static class LetterGradePolicy
{
    public static string For(decimal average)
    {
        if (average >= 90) return "A";
        if (average >= 80) return "B";
        if (average >= 70) return "C";
        if (average >= 60) return "D";
        return "F";
    }
}

public static class HonorRollPolicy
{
    public static bool Qualifies(decimal average, string letter)
    {
        return average >= 85 && (letter == "A" || letter == "B");
    }
}

public static class TranscriptFormatter
{
    public static string Plain(GradeBook grades, string studentId, string fullName)
    {
        return $"TRANSCRIPT\nStudent: {fullName} ({studentId})\nAverage: {grades.Average(studentId)}\nLetter: {grades.Letter(studentId)}\nHonor: {grades.MeetsHonorRoll(studentId)}\n";
    }
}

public static class GradeCsvExporter
{
    public static string Export(GradeBook grades)
    {
        var rows = new List<string> { "studentId,average,letter,honor" };
        foreach (var id in grades.StudentIds())
            rows.Add($"{id},{grades.Average(id)},{grades.Letter(id)},{(grades.MeetsHonorRoll(id) ? 1 : 0)}");
        return string.Join('\n', rows);
    }
}
