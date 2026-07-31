using EWasteSolutions.Data;
using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace EWasteSolutions.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public EditModel(ICategoryService categoryService)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingCategory = await _categoryService.GetByIdAsync(Category.Id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = Category.Name;
            existingCategory.Description = Category.Description;
            existingCategory.DisplayOrder = Category.DisplayOrder;
            existingCategory.IsActive = Category.IsActive;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _categoryService.UpdateAsync(existingCategory);

            TempData["SuccessMessage"] = "Category updated successfully.";

            return RedirectToPage("Index");
        }
    }
}