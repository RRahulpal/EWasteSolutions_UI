using EWasteSolutions.Data;
using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Category.CreatedAt = DateTime.UtcNow;
            Category.UpdatedAt = DateTime.UtcNow;

            await _categoryService.CreateAsync(Category);

            TempData["SuccessMessage"] = "Category created successfully.";

            return RedirectToPage("Index");
        }
    }
}