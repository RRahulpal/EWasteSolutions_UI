using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public DeleteModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryService.GetByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            Category = category;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            await _categoryService.DeleteAsync(id);

            TempData["SuccessMessage"] = "Category deleted successfully.";

            return RedirectToPage("Index");
        }
    }
}