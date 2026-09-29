namespace SrpLab;

/// <summary>Owns the checkout lines and computes cart amounts.</summary>
public sealed class CheckoutBasket
{
    private readonly List<(string Sku, decimal Price, int Qty)> _lines = new();
    private string? _couponRaw;
    private bool _giftWrap;
    internal IReadOnlyList<(string Sku, decimal Price, int Qty)> Lines => _lines;
    internal string? CouponText => _couponRaw;

    public void AddLine(string sku, decimal price, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        _lines.Add((sku, price, qty));
    }
    public void ApplyCouponText(string? couponText) => _couponRaw = couponText;
    public void EnableGiftWrap() => _giftWrap = true;
    public decimal SubTotal() => _lines.Sum(l => l.Price * l.Qty);
    public decimal DiscountAmount() => CouponDiscountCalculator.Calculate(this);
    public decimal GrandTotal() => Math.Max(0m, SubTotal() - DiscountAmount() + (_giftWrap ? GiftWrapPricing.Fee : 0m));
    public string GiftMessageCard(string fromName) => GiftMessageFormatter.Format(this, fromName);
    public string AuthorizePaymentStub(string cardLast4) => PaymentAuthorizationStub.Authorize(this, cardLast4);
}

public static class CouponDiscountCalculator
{
    public static decimal Calculate(CheckoutBasket basket)
    {
        if (string.IsNullOrWhiteSpace(basket.CouponText)) return 0m;
        var text = basket.CouponText.Trim().ToUpperInvariant();
        if (text.StartsWith("SAVE") && int.TryParse(text[4..], out var percent) && percent is > 0 and <= 50)
            return Math.Round(basket.SubTotal() * percent / 100m, 2);
        if (text.Contains("FREESHIP")) return 0m;
        if (text == "WELCOME10") return Math.Min(10m, basket.SubTotal());
        return 0m;
    }
}

public static class GiftWrapPricing
{
    public const decimal Fee = 4.99m;
}

public static class GiftMessageFormatter
{
    public static string Format(CheckoutBasket basket, string fromName)
    {
        var items = string.Join(", ", basket.Lines.Select(l => l.Sku));
        return $"Dear friend,\nA gift from {fromName} awaits ({items}).\nTotal surprise value: {basket.GrandTotal():C}\n";
    }
}

public static class PaymentAuthorizationStub
{
    public static string Authorize(CheckoutBasket basket, string cardLast4)
    {
        var payload = $"{basket.GrandTotal():0.00}|{cardLast4}|{basket.Lines.Count}";
        var hash = payload.GetHashCode();
        return $"AUTH-{Math.Abs(hash):X8}";
    }
}
