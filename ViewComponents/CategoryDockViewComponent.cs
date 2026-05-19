using ASPMMA.Data;
using ASPMMA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.ViewComponents
{
    public class CategoryDockViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryDockViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryDockItemViewModel
                {
                    Id = c.Id,
                    Name = c.CategoryName,
                    Description = c.Description,
                    ProductsCount = c.Products.Count
                })
                .ToListAsync();

            return View(categories);
        }
    }
}
