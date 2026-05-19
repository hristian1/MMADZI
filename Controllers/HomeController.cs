using System.Diagnostics;
using ASPMMA.Data;
using ASPMMA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Categorys)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.Products.Count)
                .ThenBy(c => c.CategoryName)
                .Take(8)
                .ToListAsync();

            var model = new HomeViewModel
            {
                FeaturedProducts = products
                    .Where(p => !IsPlaceholderText(p.Name)
                        && !IsPlaceholderText(p.Description)
                        && !IsPlaceholderText(p.Categorys?.CategoryName))
                    .Take(6)
                    .ToList(),
                Categories = categories
                    .Where(c => !IsPlaceholderText(c.CategoryName) && !IsPlaceholderText(c.Description))
                    .Take(4)
                    .ToList()
            };

            return View(model);
        }

        private static bool IsPlaceholderText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim().ToLowerInvariant();
            return normalized is "test" or "тест" or "tect";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
