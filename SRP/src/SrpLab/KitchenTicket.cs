namespace SrpLab;

/// <summary>Owns the order items attached to a kitchen ticket.</summary>
public sealed class KitchenTicket
{
    private readonly List<(string Item, List<string> Ingredients, int PrepMinutes)> _items = new();
    internal IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> Items => _items;

    public void AddItem(string item, IEnumerable<string> ingredients, int prepMinutes)
    {
        _items.Add((item, ingredients.Select(i => i.Trim().ToLowerInvariant()).ToList(), prepMinutes));
    }

    public IReadOnlyList<string> DetectAllergens() => AllergenDetector.Detect(Items);
    public int EstimatedReadyMinutes(int openStations) => KitchenEtaEstimator.Estimate(Items, DetectAllergens(), openStations);
    public string RenderThermalTicket(int orderNumber) => ThermalTicketFormatter.Render(this, orderNumber);
    public string ExpoLaneHint() => ExpoLanePolicy.For(DetectAllergens(), EstimatedReadyMinutes(2));
}

public static class AllergenDetector
{
    public static IReadOnlyList<string> Detect(IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> items)
    {
        var hits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in items)
        foreach (var ingredient in row.Ingredients)
        {
            if (ingredient.Contains("milk") || ingredient.Contains("cheese") || ingredient.Contains("butter")) hits.Add("dairy");
            if (ingredient.Contains("wheat") || ingredient.Contains("flour") || ingredient.Contains("bread")) hits.Add("gluten");
            if (ingredient.Contains("peanut") || ingredient.Contains("almond") || ingredient.Contains("cashew")) hits.Add("nuts");
            if (ingredient.Contains("shrimp") || ingredient.Contains("prawn") || ingredient.Contains("crab")) hits.Add("shellfish");
        }
        return hits.OrderBy(x => x).ToList();
    }
}

public static class KitchenEtaEstimator
{
    public static int Estimate(IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> items, IReadOnlyList<string> allergens, int openStations)
    {
        if (openStations <= 0) openStations = 1;
        var sequential = items.Sum(i => i.PrepMinutes);
        var parallel = (int)Math.Ceiling(sequential / (double)openStations);
        if (allergens.Count > 0) parallel += 3;
        var longest = items.Count == 0 ? 0 : items.Max(i => i.PrepMinutes);
        return Math.Max(parallel, longest);
    }
}

public static class ThermalTicketFormatter
{
    public static string Render(KitchenTicket ticket, int orderNumber)
    {
        const int width = 32;
        var line = new string('=', width);
        var body = string.Join('\n', ticket.Items.Select(i => $"* {i.Item.ToUpperInvariant()} ({i.PrepMinutes}m)"));
        var allergens = ticket.DetectAllergens();
        var allergyLine = allergens.Count == 0 ? "ALLERGENS: none" : "ALLERGENS: " + string.Join(",", allergens);
        return $"{line}\nORDER #{orderNumber}\nETA {ticket.EstimatedReadyMinutes(2)} MIN\n{body}\n{allergyLine}\n{line}\n";
    }
}

public static class ExpoLanePolicy
{
    public static string For(IReadOnlyList<string> allergens, int etaMinutes)
    {
        if (allergens.Count > 0) return "LANE-ALLERGY";
        return etaMinutes > 20 ? "LANE-SLOW" : "LANE-FAST";
    }
}
