using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public CartsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IQueryable<Cart> query = _context.Carts
                .Include(c => c.Clients)
                .Include(c => c.Products)
                .ThenInclude(p => p.Categorys);

            if (User.IsInRole("Admin"))
            {
                return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUserId = _userManager.GetUserId(User);
                return View(await query
                    .Where(c => c.ClientId == currentUserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync());
            }

            return View(new List<Cart>());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Clients)
                .Include(c => c.Products)
                .ThenInclude(p => p.Categorys)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            if (!CanAccessClientRecord(cart.ClientId))
            {
                return Forbid();
            }

            return View(cart);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity")] Cart cart)
        {
            cart.CreatedAt = DateTime.Now;
            cart.ClientId = _userManager.GetUserId(User) ?? string.Empty;

            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name", cart.ProductId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Challenge();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity <= 0)
            {
                TempData["CartError"] = "Продуктът не е наличен в момента.";
                return Redirect(returnUrl ?? Url.Action("Details", "Products", new { id = productId })!);
            }

            quantity = Math.Max(1, quantity);
            var userId = _userManager.GetUserId(User) ?? string.Empty;

            var existingCart = await _context.Carts
                .FirstOrDefaultAsync(c => c.ClientId == userId && c.ProductId == productId);

            var targetQuantity = quantity + (existingCart?.Quantity ?? 0);
            if (targetQuantity > product.StockQuantity)
            {
                TempData["CartError"] = "Нямате достатъчна наличност за избраното количество.";
                return Redirect(returnUrl ?? Url.Action("Details", "Products", new { id = productId })!);
            }

            if (existingCart == null)
            {
                existingCart = new Cart
                {
                    ClientId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now
                };
                _context.Carts.Add(existingCart);
            }
            else
            {
                existingCart.Quantity = targetQuantity;
                existingCart.CreatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["CartSuccess"] = "Продуктът беше добавен в количката.";

            return Redirect(returnUrl ?? Url.Action(nameof(Index))!);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            if (!CanAccessClientRecord(cart.ClientId))
            {
                return Forbid();
            }

            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name", cart.ProductId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id);
            if (existingCart == null)
            {
                return NotFound();
            }

            if (!CanAccessClientRecord(existingCart.ClientId))
            {
                return Forbid();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cart.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Изберете валиден продукт.");
            }

            if (cart.Quantity < 1)
            {
                ModelState.AddModelError("Quantity", "Количеството трябва да е поне 1.");
            }

            if (product != null && cart.Quantity > product.StockQuantity)
            {
                ModelState.AddModelError("Quantity", "Количеството надвишава наличността.");
            }

            if (ModelState.IsValid)
            {
                existingCart.ProductId = cart.ProductId;
                existingCart.Quantity = cart.Quantity;
                existingCart.CreatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name", cart.ProductId);
            return View(cart);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Clients)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            if (!CanAccessClientRecord(cart.ClientId))
            {
                return Forbid();
            }

            return View(cart);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!CanAccessClientRecord(cart.ClientId))
            {
                return Forbid();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var carts = await _context.Carts
                .Include(c => c.Products)
                .Where(c => c.ClientId == userId)
                .ToListAsync();

            if (!carts.Any())
            {
                TempData["CartError"] = "Количката е празна.";
                return RedirectToAction(nameof(Index));
            }

            var insufficientProduct = carts.FirstOrDefault(c => c.Products.StockQuantity < c.Quantity);
            if (insufficientProduct != null)
            {
                TempData["CartError"] = $"Недостатъчна наличност за {insufficientProduct.Products.Name}.";
                return RedirectToAction(nameof(Index));
            }

            var createdAt = DateTime.Now;

            foreach (var cart in carts)
            {
                _context.Orders.Add(new Order
                {
                    ClientId = cart.ClientId,
                    ProductId = cart.ProductId,
                    Quantity = cart.Quantity,
                    CreatedAt = createdAt,
                    Status = OrderStatus.New
                });

                cart.Products.StockQuantity -= cart.Quantity;
            }

            _context.Carts.RemoveRange(carts);
            await _context.SaveChangesAsync();

            TempData["OrderSuccess"] = "Поръчката беше създадена успешно.";
            return RedirectToAction("Index", "Orders");
        }

        private bool CanAccessClientRecord(string clientId)
        {
            return User.IsInRole("Admin") || clientId == _userManager.GetUserId(User);
        }
    }
}
