using EWasteSolutions.Data;
using EWasteSolutions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EWasteSolutions.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "displayOrder";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "asc";

        public async Task OnGetAsync()
        {
            if (PageNumber < 1)
            {
                PageNumber = 1;
            }

            if (PageSize != 10 && PageSize != 25 && PageSize != 50)
            {
                PageSize = 10;
            }

            IQueryable<Category> query = _context.Categories
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var search = SearchTerm.Trim();

                query = query.Where(c =>
                    EF.Functions.ILike(c.Name, $"%{search}%") ||
                    (c.Description != null &&
                     EF.Functions.ILike(c.Description, $"%{search}%")));
            }

            if (Status == "active")
            {
                query = query.Where(c => c.IsActive);
            }
            else if (Status == "inactive")
            {
                query = query.Where(c => !c.IsActive);
            }

            bool descending = SortDirection == "desc";

            query = SortBy switch
            {
                "name" => descending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),

                "createdAt" => descending
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),

                _ => descending
                    ? query.OrderByDescending(c => c.DisplayOrder)
                        .ThenByDescending(c => c.Name)
                    : query.OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.Name)
            };

            TotalRecords = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(
                TotalRecords / (double)PageSize
            );

            if (TotalPages > 0 && PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
            }

            Categories = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}