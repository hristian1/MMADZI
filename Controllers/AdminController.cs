using ASPMMA.Data;
using ASPMMA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const string DefaultResetPassword = "TestUserPass1";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var revenueOrders = await _context.Orders
                .Include(o => o.Products)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            var revenue = revenueOrders.Sum(o => (o.Products?.Price ?? 0) * o.Quantity);

            var model = new AdminDashboardViewModel
            {
                ProductsCount = await _context.Products.CountAsync(),
                CategoriesCount = await _context.Categories.CountAsync(),
                UsersCount = await _context.Users.CountAsync(),
                OrdersCount = await _context.Orders.CountAsync(),
                PendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Processing),
                Revenue = revenue,
                LowStockProducts = await _context.Products
                    .Include(p => p.Categorys)
                    .Where(p => p.StockQuantity <= 5)
                    .OrderBy(p => p.StockQuantity)
                    .ThenBy(p => p.Name)
                    .Take(6)
                    .ToListAsync(),
                RecentOrders = await _context.Orders
                    .Include(o => o.Products)
                    .Include(o => o.Clients)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(6)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Categorys)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Carts)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var rows = new List<AdminUserRowViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                rows.Add(new AdminUserRowViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    Roles = roles.Any() ? string.Join(", ", roles) : "No role",
                    OrdersCount = user.Orders.Count,
                    CartItemsCount = user.Carts.Sum(x => x.Quantity)
                });
            }

            return View(new AdminUsersViewModel
            {
                DefaultPassword = DefaultResetPassword,
                Users = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, DefaultResetPassword);

            if (!result.Succeeded)
            {
                TempData["AdminError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }

            await _userManager.UpdateSecurityStampAsync(user);
            TempData["AdminSuccess"] = $"Паролата на {user.UserName} беше нулирана до {DefaultResetPassword}.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .ThenInclude(p => p.Categorys)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(new AdminOrdersViewModel
            {
                Orders = orders,
                AvailableStatuses = Enum.GetValues<OrderStatus>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == status)
            {
                TempData["AdminSuccess"] = "Статусът е без промяна.";
                return RedirectToAction(nameof(Orders));
            }

            if (order.Status != OrderStatus.Cancelled && status == OrderStatus.Cancelled)
            {
                order.Products.StockQuantity += order.Quantity;
            }
            else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            {
                if (order.Products.StockQuantity < order.Quantity)
                {
                    TempData["AdminError"] = $"Недостатъчна наличност за повторно активиране на поръчка #{order.Id}.";
                    return RedirectToAction(nameof(Orders));
                }

                order.Products.StockQuantity -= order.Quantity;
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["AdminSuccess"] = $"Поръчка #{order.Id} е със статус {status.GetLabel()}.";
            return RedirectToAction(nameof(Orders));
        }
    }
}
