using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        public string AdminEmail { get; set; } = string.Empty;

        public void OnGet()
        {
            AdminEmail = User.Identity?.Name ?? "Administrator";
        }
    }
}