using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoftwareDeploymentProject.Models;
using SoftwareDeploymentProject.Services;

namespace SoftwareDeploymentProject.Pages;

public class IndexModel(ShopStore store, CartIdentity identity) : PageModel
{
    public IReadOnlyList<Product> Products { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [TempData] public string? Notice { get; set; }
    public void OnGet()
    {
        Products = store.Products().Where(p => string.IsNullOrEmpty(Category) || p.Category == Category).ToList();
    }
    public IActionResult OnPostAdd(int productId, int quantity = 1)
    {
        if (!ModelState.IsValid) { Notice = "Please enter a valid product and quantity."; return RedirectToPage(); }
        try { store.ChangeQuantity(identity.Get(forNewItems: true), productId, quantity, add: true); Notice = "Added to your cart."; }
        catch (ArgumentException exception) { Notice = exception.Message; }
        return RedirectToPage(new { Category });
    }
}
