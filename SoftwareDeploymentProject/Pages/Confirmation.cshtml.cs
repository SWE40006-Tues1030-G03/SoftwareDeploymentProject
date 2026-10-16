using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoftwareDeploymentProject.Models;
using SoftwareDeploymentProject.Services;

namespace SoftwareDeploymentProject.Pages;

public class ConfirmationModel(ShopStore store, CartIdentity identity) : PageModel
{
    public Order Order { get; private set; } = null!;
    public IActionResult OnGet(string reference)
    {
        var order = store.GetOrder(reference ?? "", identity.Get());
        if (order is null) return NotFound();
        Order = order;
        return Page();
    }
}
