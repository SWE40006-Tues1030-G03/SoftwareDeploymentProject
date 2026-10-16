namespace SoftwareDeploymentProject.Services;

// The random cookie identifies an anonymous cart. It contains no personal information.
public sealed class CartIdentity(IHttpContextAccessor accessor, ShopStore store, IConfiguration configuration)
{
    public const string CookieName = "CampusShop.Cart";
    private string? current;
    public string Get(bool forNewItems = false)
    {
        if (current is not null) return current;
        var context = accessor.HttpContext ?? throw new InvalidOperationException("A request is required.");
        var cookie = context.Request.Cookies[CookieName];
        if (!Guid.TryParseExact(cookie, "N", out _) || (forNewItems && store.HasOrder(cookie)))
        {
            cookie = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(CookieName, cookie, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps || configuration.GetValue<bool>("Shop:SecureCookies"),
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(7),
                Path = "/"
            });
        }
        return current = cookie!;
    }
}
