using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = user.Id, WishlistItems = new List<WishlistItem>() };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return View(wishlist);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Status != ProductStatus.Approved)
            {
                TempData["message"] = "Produsul nu exista sau nu este aprobat!";
                return RedirectToAction("Index");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = user.Id };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            if (!wishlist.WishlistItems.Any(wi => wi.ProductId == productId))
            {
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };
                _context.WishlistItems.Add(item);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Confirmation", "Home", new { 
                    title = "Produs Adaugat", 
                    message = "Produsul a fost adaugat cu succes in Wishlist-ul tau!",
                    primaryAction = "Index",
                    primaryController = "Wishlist",
                    primaryText = "Vezi Wishlist"
                });
            }
            else 
            {
                 TempData["message"] = "Produsul este deja in Wishlist!";
                 return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var item = await _context.WishlistItems.FindAsync(itemId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int itemId)
        {
             var item = await _context.WishlistItems.Include(wi => wi.Product).FirstOrDefaultAsync(wi => wi.Id == itemId);
             
             if (item != null)
             {
                 var user = await _userManager.GetUserAsync(User);

                 // 1. Verificare Stoc
                 if (item.Product.Stock < 1)
                 {
                     TempData["message"] = "Produsul nu mai este in stoc!";
                     TempData["messageType"] = "alert-danger";
                     return RedirectToAction("Index");
                 }

                 // 2. Gasire/Creare Cos
                 var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                 if (cart == null)
                 {
                     cart = new Cart { UserId = user.Id };
                     _context.Carts.Add(cart);
                     await _context.SaveChangesAsync();
                 }

                 // 3. Adaugare in Cos
                 var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);
                 if (cartItem != null)
                 {
                     cartItem.Quantity += 1;
                 }
                 else
                 {
                     cartItem = new CartItem
                     {
                         CartId = cart.Id,
                         ProductId = item.ProductId,
                         Quantity = 1,
                         DateAdded = DateTime.UtcNow
                     };
                     _context.CartItems.Add(cartItem);
                 }

                 // 4. Stergere din Wishlist
                 _context.WishlistItems.Remove(item);

                 await _context.SaveChangesAsync();

                 return RedirectToAction("Confirmation", "Home", new { 
                    title = "Produs Mutat", 
                    message = "Produsul a fost mutat cu succes in cosul de cumparaturi!",
                    primaryAction = "Index",
                    primaryController = "Cart",
                    primaryText = "Vezi Cosul",
                    secondaryAction = "Index",
                    secondaryController = "Wishlist",
                    secondaryText = "Inapoi la Wishlist"
                });
             }
             return RedirectToAction("Index");
        }
    }
}
