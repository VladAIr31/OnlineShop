using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return View(cart);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return NotFound();

            if (product.Stock < quantity)
            {
                TempData["message"] = "Stoc insuficient!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Prevent adding unapproved products
            if (product.Status != ProductStatus.Approved)
            {
                TempData["message"] = "Nu puteti adauga in cos un produs care nu este aprobat!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Confirmation", "Home", new { 
                title = "Produs Adaugat in Cos", 
                message = "Produsul a fost adaugat cu succes in cosul tau de cumparaturi!",
                primaryAction = "Index",
                primaryController = "Cart",
                primaryText = "Vezi Cosul",
                secondaryAction = "Details",
                secondaryController = "Products",
                secondaryText = "Inapoi la Produs" // Or just default "Continua Cumparaturile" which goes to Product Index
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var cartItem = await _context.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.Id == itemId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                     _context.CartItems.Remove(cartItem);
                }
                else if (quantity <= cartItem.Product.Stock)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    TempData["message"] = $"Stoc maxim disponibil: {cartItem.Product.Stock}";
                     TempData["messageType"] = "alert-warning";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
