using EWasteSolutions.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EWasteSolutions.Pages.Admin
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalPickupRequests { get; set; }
        public int TotalEnquiries { get; set; }

        public async Task OnGetAsync()
        {
            TotalProducts = await _context.Products.CountAsync();
            TotalCategories = await _context.Categories.CountAsync();

            // Keep these as 0 until their tables/modules are created.
            TotalPickupRequests = 0;
            TotalEnquiries = 0;
        }
    }
}