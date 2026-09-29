namespace SrpLab;

/// <summary>Owns the bed assignment and latest clinical score for each bed.</summary>
public sealed class WardBoard
{
    private readonly Dictionary<int, string> _bedPatient = new();
    private readonly Dictionary<int, int> _vitalsScore = new();
    private readonly WardPager _pager = new();
    internal IReadOnlyDictionary<int, string> BedPatients => _bedPatient;
    internal IReadOnlyDictionary<int, int> VitalsScores => _vitalsScore;

    public void AssignBed(int bed, string patientId, int heartRate, int spo2)
    {
        if (bed <= 0) throw new ArgumentOutOfRangeException(nameof(bed));
        if (string.IsNullOrWhiteSpace(patientId)) throw new ArgumentException("patient required");
        _bedPatient[bed] = patientId.Trim().ToUpperInvariant();
        _vitalsScore[bed] = AcuityScorer.Score(heartRate, spo2);
        _pager.RecordIfCritical(bed, _vitalsScore[bed]);
    }

    public int ScoreAcuity(int heartRate, int spo2) => AcuityScorer.Score(heartRate, spo2);
    public string BuildHandoffNote(int bed) => WardHandoffFormatter.Format(this, bed);
    public IReadOnlyList<string> DrainPagerLog() => _pager.Drain();
    public string ExportCensusCsv() => WardCensusCsvExporter.Export(this);
}

public static class AcuityScorer
{
    public static int Score(int heartRate, int spo2)
    {
        var score = 0;
        if (heartRate > 120 || heartRate < 45) score += 4;
        else if (heartRate > 100) score += 2;
        if (spo2 < 90) score += 5;
        else if (spo2 < 94) score += 2;
        return Math.Min(score, 10);
    }
}

public sealed class WardPager
{
    private readonly List<string> _log = new();
    public void RecordIfCritical(int bed, int acuity)
    {
        if (acuity >= 8) _log.Add($"CODE-YELLOW bed={bed} at {DateTime.UtcNow:HH:mm}");
    }
    public IReadOnlyList<string> Drain()
    {
        var copy = _log.ToList(); _log.Clear(); return copy;
    }
}

public static class WardHandoffFormatter
{
    public static string Format(WardBoard board, int bed)
    {
        if (!board.BedPatients.TryGetValue(bed, out var patient)) return $"Bed {bed}: empty";
        var acuity = board.VitalsScores[bed];
        var tone = acuity >= 8 ? "ESCALATE" : acuity >= 4 ? "WATCH" : "STABLE";
        return $"[HANDOFF {DateTime.UtcNow:yyyy-MM-dd}] Bed {bed} · {patient} · acuity={acuity} · {tone}";
    }
}

public static class WardCensusCsvExporter
{
    public static string Export(WardBoard board)
    {
        var lines = new List<string> { "bed,patient,acuity" };
        foreach (var bed in board.BedPatients.Keys.OrderBy(x => x))
            lines.Add($"{bed},{board.BedPatients[bed]},{board.VitalsScores[bed]}");
        return string.Join('\n', lines);
    }
}
