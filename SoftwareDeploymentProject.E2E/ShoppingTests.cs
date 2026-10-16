using Microsoft.Playwright;
namespace SoftwareDeploymentProject.E2E;

public class ShoppingTests
{
    [Fact]
    public async Task ShopperCanManageCartAndPlaceOrder()
    {
        var url = Environment.GetEnvironmentVariable("SHOP_E2E_URL")
            ?? throw new InvalidOperationException("Set SHOP_E2E_URL to an isolated running test server. See docs/TESTING.md.");
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        await using var context = await browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } });
        var page = await context.NewPageAsync();
        var errors = new List<string>(); page.PageError += (_, error) => errors.Add(error);
        var evidence = Path.GetFullPath(Environment.GetEnvironmentVariable("SHOP_EVIDENCE_DIR") ?? "TestResults/browser");
        Directory.CreateDirectory(evidence);
        await page.GotoAsync(url);
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Shop the essentials" })).ToBeVisibleAsync();
        await page.ScreenshotAsync(new() { Path = Path.Combine(evidence, "01-catalogue-desktop.png"), FullPage = true });
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Campus hoodie to cart", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("cart-count")).ToHaveTextAsync("1");
        await page.GetByRole(AriaRole.Link, new() { Name = "Your cart", Exact = false }).First.ClickAsync();
        await page.GetByLabel("Quantity", new() { Exact = true }).FillAsync("2");
        await page.GetByRole(AriaRole.Button, new() { Name = "Update", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("cart-total")).ToHaveTextAsync("$130.00");
        await page.ScreenshotAsync(new() { Path = Path.Combine(evidence, "02-cart.png"), FullPage = true });
        await page.GetByRole(AriaRole.Button, new() { Name = "Place demo order", Exact = false }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Your demo order is confirmed." })).ToBeVisibleAsync();
        await page.ReloadAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Your demo order is confirmed." })).ToBeVisibleAsync();
        await page.ScreenshotAsync(new() { Path = Path.Combine(evidence, "03-confirmation.png"), FullPage = true });
        await page.GotoAsync(url);
        await page.GetByRole(AriaRole.Link, new() { Name = "Stationery", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("product-1")).ToHaveCountAsync(0);
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Study notebook to cart", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Your cart", Exact = false }).First.ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Remove Study notebook", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByText("Your cart is empty.", new() { Exact = false })).ToBeVisibleAsync();
        await page.SetViewportSizeAsync(390, 844); await page.GotoAsync(url);
        await Assertions.Expect(page.GetByTestId("product-1")).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > window.innerWidth"));
        await page.ScreenshotAsync(new() { Path = Path.Combine(evidence, "04-catalogue-mobile.png"), FullPage = true });
        Assert.Empty(errors);
    }
}
