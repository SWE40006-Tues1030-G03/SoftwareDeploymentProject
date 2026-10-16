using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
namespace SoftwareDeploymentProject.Tests;

public class ShopFactory : WebApplicationFactory<Program>
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "campus-web-" + Guid.NewGuid().ToString("N"));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Shop:DatabasePath"] = Path.Combine(directory, "shop.db") }));
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); SqliteConnection.ClearAllPools();
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
public class WebTests
{
    private static HttpClient Client(ShopFactory factory) => factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });
    private static async Task<HttpResponseMessage> Post(HttpClient client, string page, string handler, params (string Key, string Value)[] values)
    {
        var html = await client.GetStringAsync("/"); // Tokens are valid across this browser's forms, including an empty cart.
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        return await client.PostAsync(page + "?handler=" + handler, new FormUrlEncodedContent(values.Append(("__RequestVerificationToken", WebUtility.HtmlDecode(token))).Select(v => new KeyValuePair<string, string>(v.Item1, v.Item2))));
    }
    [Fact]
    public async Task CatalogueAndHealthAreAvailable()
    {
        using var factory = new ShopFactory(); using var client = Client(factory);
        Assert.Contains("Campus hoodie", await client.GetStringAsync("/"));
        Assert.Contains("healthy", await client.GetStringAsync("/health"));
        Assert.Contains("ready", await client.GetStringAsync("/health/ready"));
    }
    [Fact]
    public async Task FilteringShowsOnlySelectedCategory()
    {
        using var factory = new ShopFactory(); using var client = Client(factory);
        var html = await client.GetStringAsync("/?category=Stationery");
        Assert.Contains("Study notebook", html); Assert.DoesNotContain("data-testid=\"product-1\"", html);
    }
    [Fact]
    public async Task CartMutationRequiresAntiforgeryToken()
    {
        using var factory = new ShopFactory(); using var client = Client(factory);
        using var response = await client.PostAsync("/?handler=Add", new FormUrlEncodedContent(new Dictionary<string, string> { { "productId", "1" } }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task FullHttpJourneyIsIsolatedAndUsesServerPrices()
    {
        using var factory = new ShopFactory(); using var alice = Client(factory); using var bob = Client(factory);
        using var added = await Post(alice, "/", "Add", ("productId", "1"), ("quantity", "2"), ("priceCents", "1"));
        Assert.Equal(HttpStatusCode.Redirect, added.StatusCode);
        Assert.Contains("$130.00", await alice.GetStringAsync("/Cart"));
        Assert.Contains("Your cart is empty", await bob.GetStringAsync("/Cart"));
        using var updated = await Post(alice, "/Cart", "Update", ("productId", "1"), ("quantity", "3"));
        Assert.Contains("$195.00", await alice.GetStringAsync("/Cart"));
        using var checkout = await Post(alice, "/Cart", "Checkout");
        Assert.Equal(HttpStatusCode.Redirect, checkout.StatusCode);
        var receipt = checkout.Headers.Location!.ToString();
        Assert.Contains("Your demo order is confirmed", await alice.GetStringAsync(receipt));
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(receipt)).StatusCode);
        Assert.Contains("Your cart is empty", await alice.GetStringAsync("/Cart"));
        using var repeat = await Post(alice, "/Cart", "Checkout"); Assert.Equal(receipt, repeat.Headers.Location!.ToString());
        using var newCart = await Post(alice, "/", "Add", ("productId", "3"));
        Assert.Contains("$12.00", await alice.GetStringAsync("/Cart"));
    }
    [Theory]
    [InlineData("0")]
    [InlineData("21")]
    [InlineData("abc")]
    public async Task InvalidQuantityDoesNotChangeCart(string quantity)
    {
        using var factory = new ShopFactory(); using var client = Client(factory);
        await Post(client, "/", "Add", ("productId", "1"), ("quantity", quantity));
        Assert.Contains("Your cart is empty", await client.GetStringAsync("/Cart"));
    }
    [Fact]
    public async Task RemoveAndEmptyCheckoutAreHandled()
    {
        using var factory = new ShopFactory(); using var client = Client(factory);
        await Post(client, "/", "Add", ("productId", "1"));
        await Post(client, "/Cart", "Remove", ("productId", "1"));
        Assert.Contains("Your cart is empty", await client.GetStringAsync("/Cart"));
        var response = await Post(client, "/Cart", "Checkout");
        Assert.Equal("/Cart", response.Headers.Location!.ToString());
    }
}
