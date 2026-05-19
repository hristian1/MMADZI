using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<Client> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public Client CurrentUser { get; set; } = null!;
        public IReadOnlyList<Order> RecentOrders { get; set; } = [];
        public int OrdersCount { get; set; }
        public int ActiveOrdersCount { get; set; }
        public decimal TotalSpent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            CurrentUser = user;

            var orders = await _context.Orders
                .Include(o => o.Products)
                .ThenInclude(p => p.Categorys)
                .Where(o => o.ClientId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            OrdersCount = orders.Count;
            ActiveOrdersCount = orders.Count(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped);
            TotalSpent = orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .Sum(o => (o.Products?.Price ?? 0) * o.Quantity);
            RecentOrders = orders.Take(6).ToList();

            return Page();
        }
    }
}
