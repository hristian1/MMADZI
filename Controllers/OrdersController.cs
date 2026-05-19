using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .ThenInclude(p => p.Categorys);

            if (User.IsInRole("Admin"))
            {
                return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                return View(await query
                    .Where(o => o.ClientId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync());
            }

            return View(new List<Order>());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .ThenInclude(p => p.Categorys)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!CanAccessClientRecord(order.ClientId))
            {
                return Forbid();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            ViewData["ClientId"] = new SelectList(_context.Users.OrderBy(u => u.UserName), "Id", "UserName");
            return View(new Order { Status = OrderStatus.New, Quantity = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ClientId,ProductId,Quantity,Status")] Order order)
        {
            order.CreatedAt = DateTime.Now;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == order.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Изберете валиден продукт.");
            }
            else if (order.Status != OrderStatus.Cancelled && order.Quantity > product.StockQuantity)
            {
                ModelState.AddModelError("Quantity", "Количеството надвишава наличността.");
            }

            if (string.IsNullOrWhiteSpace(order.ClientId) || !await _context.Users.AnyAsync(u => u.Id == order.ClientId))
            {
                ModelState.AddModelError("ClientId", "Изберете валиден клиент.");
            }

            if (order.Quantity < 1)
            {
                ModelState.AddModelError("Quantity", "Количеството трябва да е поне 1.");
            }

            if (ModelState.IsValid)
            {
                if (order.Status != OrderStatus.Cancelled)
                {
                    product!.StockQuantity -= order.Quantity;
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateCreateViewData(order.ProductId, order.ClientId);
            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            var existingOrder = await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!await TryApplyStatusChangeAsync(existingOrder, order.Status))
                    {
                        return View(existingOrder);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(existingOrder);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                if (order.Status != OrderStatus.Cancelled)
                {
                    order.Products.StockQuantity += order.Quantity;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateCreateViewData(int? productId = null, string? clientId = null)
        {
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name", productId);
            ViewData["ClientId"] = new SelectList(_context.Users.OrderBy(u => u.UserName), "Id", "UserName", clientId);
        }

        private async Task<bool> TryApplyStatusChangeAsync(Order existingOrder, OrderStatus nextStatus)
        {
            if (existingOrder.Status == nextStatus)
            {
                return true;
            }

            if (existingOrder.Status != OrderStatus.Cancelled && nextStatus == OrderStatus.Cancelled)
            {
                existingOrder.Products.StockQuantity += existingOrder.Quantity;
            }
            else if (existingOrder.Status == OrderStatus.Cancelled && nextStatus != OrderStatus.Cancelled)
            {
                await _context.Entry(existingOrder.Products).ReloadAsync();

                if (existingOrder.Products.StockQuantity < existingOrder.Quantity)
                {
                    ModelState.AddModelError("Status", "Недостатъчна наличност за повторно активиране на поръчката.");
                    return false;
                }

                existingOrder.Products.StockQuantity -= existingOrder.Quantity;
            }

            existingOrder.Status = nextStatus;
            return true;
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private bool CanAccessClientRecord(string clientId)
        {
            return User.IsInRole("Admin") || clientId == _userManager.GetUserId(User);
        }
    }
}
