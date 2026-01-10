using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders (Istoric comenzi)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["message"] = "Cosul este gol!";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Index", "Cart");
            }

            // Validare Stoc Finala
            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    TempData["message"] = $"Stoc insuficient pentru {item.Product.Title}. Stoc ramas: {item.Product.Stock}";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Cart");
                }
                
                if (item.Product.Status != OnlineShop.Models.ProductStatus.Approved)
                {
                    TempData["message"] = $"Produsul {item.Product.Title} nu mai este disponibil (neaprobat).";
                    TempData["messageType"] = "alert-danger";
                    // Optional: remove it from cart automatically?
                    // _context.CartItems.Remove(item);
                    // await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Creare Comanda
            var order = new Order
            {
                UserId = user.Id,
                Date = DateTime.UtcNow,
                Status = "Inregistrata",
                TotalAmount = cart.CartItems.Sum(i => i.Quantity * i.Product.Price),
                OrderDetails = new List<OrderDetail>()
            };

            // Procesare iteme
            foreach (var item in cart.CartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });

                // Scadere Stoc
                item.Product.Stock -= item.Quantity;
                _context.Update(item.Product);
            }

            _context.Orders.Add(order);

            // Golire Cos
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["message"] = "Comanda a fost plasata cu succes!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Confirmation", "Home", new { 
                title = "Comanda Plasata", 
                message = "Comanda ta a fost inregistrata cu succes! Iti multumim!",
                primaryAction = "Index",
                primaryController = "Orders",
                primaryText = "Vezi Istoric Comenzi"
            });
        }
    }
}
