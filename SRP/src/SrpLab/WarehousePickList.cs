namespace SrpLab;

/// <summary>Owns the requested stock lines in one pick batch.</summary>
public sealed class WarehousePickList
{
    private readonly List<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> _lines = new();
    internal IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> Lines => _lines;
    public void AddNeed(string sku, string aisle, int bin, int qtyNeeded, int qtyOnHand) => _lines.Add((sku, aisle, bin, qtyNeeded, qtyOnHand));
    public IReadOnlyList<(string Sku, int Allocated)> Allocate() => StockAllocationPolicy.Allocate(Lines);
    public IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> WalkingOrder() => WarehouseRoutePlanner.WalkingOrder(Lines);
    public string PickerScript() => PickerScriptFormatter.Format(this);
    public string WmsXmlBatch(string batchId) => WmsXmlBatchSerializer.Serialize(this, batchId);
}

public static class StockAllocationPolicy
{
    public static IReadOnlyList<(string Sku, int Allocated)> Allocate(IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> lines)
    {
        var result = new List<(string, int)>();
        foreach (var line in lines) result.Add((line.Sku, Math.Min(line.QtyNeeded, line.QtyOnHand)));
        return result;
    }
}

public static class WarehouseRoutePlanner
{
    public static IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> WalkingOrder(IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> lines)
    {
        return lines.OrderBy(l => l.Aisle).ThenBy(l => l.Bin)
            .Select(l => (l.Aisle, l.Bin, l.Sku, Math.Min(l.QtyNeeded, l.QtyOnHand)))
            .Where(x => x.Item4 > 0).ToList();
    }
}

public static class PickerScriptFormatter
{
    public static string Format(WarehousePickList pickList)
    {
        var steps = pickList.WalkingOrder().Select((step, index) => $"{index + 1}. Go aisle {step.Aisle} bin {step.Bin}: pick {step.Qty} × {step.Sku}");
        var allocations = pickList.Allocate();
        var shortfalls = allocations.Where(allocation =>
        {
            var need = pickList.Lines.First(line => line.Sku == allocation.Sku).QtyNeeded;
            return allocation.Allocated < need;
        });
        var warning = shortfalls.Any() ? "SHORTAGES: " + string.Join(", ", shortfalls.Select(x => x.Sku)) : "SHORTAGES: none";
        return string.Join('\n', steps) + "\n" + warning;
    }
}

public static class WmsXmlBatchSerializer
{
    public static string Serialize(WarehousePickList pickList, string batchId)
    {
        var parts = pickList.Allocate().Select(a => $"<line sku=\"{a.Sku}\" qty=\"{a.Allocated}\" />");
        return $"<batch id=\"{batchId}\">{string.Join("", parts)}</batch>";
    }
}
