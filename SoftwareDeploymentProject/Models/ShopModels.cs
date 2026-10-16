using System.Globalization;

namespace SoftwareDeploymentProject.Models;

public record Product(int Id, string Name, string Category, string Description, int PriceCents, string Icon);
public record CartLine(Product Product, int Quantity)
{
    public int TotalCents => checked(Product.PriceCents * Quantity);
}
public record Cart(IReadOnlyList<CartLine> Lines)
{
    public int ItemCount => Lines.Sum(line => line.Quantity);
    public int TotalCents => Lines.Sum(line => line.TotalCents);
}
public record OrderLine(string Name, int PriceCents, int Quantity);
public record Order(string Reference, string CreatedUtc, int TotalCents, IReadOnlyList<OrderLine> Lines);

public static class ShopRules
{
    public const int MaximumQuantity = 20;
    public static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > MaximumQuantity)
            throw new ArgumentException($"Choose a quantity between 1 and {MaximumQuantity}.");
    }
    public static string Money(int cents) => (cents / 100m).ToString("C", CultureInfo.GetCultureInfo("en-AU"));
}
