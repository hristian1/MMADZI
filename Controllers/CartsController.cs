using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var applicationDbContext = _context.Carts
                   .Include(o => o.Clients)
                   .Include(o => o.Products);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var applicationDbContext = _context.Carts

                    .Include(o => o.Clients)
                    .Include(o => o.Products)
                    .Where(x => x.ClientId == _userManager.GetUserId(User));
                return View(await applicationDbContext.ToListAsync());
            }

        }
        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            //ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity")] Cart cart)
        {

           cart.CreatedAt = DateTime.Now;
           cart.ClientId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id", order.ClientId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cart.ProductId);
            return View(cart);
        }

        // GET: Orders/Edit/5
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
            //ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id", order.ClientId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            return View(cart);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(cart.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id", order.ClientId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            return View(cart);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}

