using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoftwareDeploymentProject.Models;
using SoftwareDeploymentProject.Services;

namespace SoftwareDeploymentProject.Pages;

public class CartModel(ShopStore store, CartIdentity identity) : PageModel
{
    public Cart Cart { get; private set; } = new([]);
    [TempData] public string? Notice { get; set; }
    public void OnGet() => Cart = store.GetCart(identity.Get());
    public IActionResult OnPostUpdate(int productId, int quantity)
    {
        if (!ModelState.IsValid) { Notice = "Please enter a whole-number quantity."; return RedirectToPage(); }
        try { store.ChangeQuantity(identity.Get(), productId, quantity, add: false); Notice = "Cart updated."; }
        catch (ArgumentException exception) { Notice = exception.Message; }
        return RedirectToPage();
    }
    public IActionResult OnPostRemove(int productId)
    {
        if (!ModelState.IsValid) return BadRequest();
        store.Remove(identity.Get(), productId); Notice = "Item removed.";
        return RedirectToPage();
    }
    public IActionResult OnPostCheckout()
    {
        try { return RedirectToPage("/Confirmation", new { reference = store.Checkout(identity.Get()) }); }
        catch (ArgumentException exception) { Notice = exception.Message; return RedirectToPage(); }
    }
}
