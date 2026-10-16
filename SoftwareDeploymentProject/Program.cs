using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.DataProtection;
using SoftwareDeploymentProject.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
var databasePath = builder.Configuration["Shop:DatabasePath"] ?? "App_Data/shop.db";
if (!Path.IsPathRooted(databasePath)) databasePath = Path.Combine(builder.Environment.ContentRootPath, databasePath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(databasePath)!, "keys")));
var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, ForeignKeys = true }.ToString();
builder.Services.AddSingleton(new ShopStore(connectionString));
builder.Services.AddScoped<CartIdentity>();
var app = builder.Build();
app.Services.GetRequiredService<ShopStore>().Initialize();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
// TLS is terminated by the production reverse proxy. Local/container HTTP stays usable.
// Serve wwwroot directly in both source runs and published deployments.
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/ready", (ShopStore store, ILogger<Program> logger) =>
{
    try { return store.IsReady() ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503); }
    catch (SqliteException exception)
    {
        logger.LogError(exception, "Database readiness check failed");
        return Results.StatusCode(503);
    }
});
app.Run();
public partial class Program { }
