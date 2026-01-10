using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _env = env;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index(string? searchString, int? categoryId, string? sortOrder)
        {
            // ViewBag pentru a retine selectiile in view
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;

            // Preluam categoriile pentru dropdown
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", categoryId);

            // Query de baza - doar produse aprobate
            var products = db.Products.Include("Category")
                                      .Include("Reviews")
                                      .Where(p => p.Status == ProductStatus.Approved);

            // 1. Cautare dupa titlu sau descriere (Case Insensitive)
            if (!string.IsNullOrEmpty(searchString))
            {
                var search = searchString.ToLower();
                products = products.Where(p => p.Title.ToLower().Contains(search) || 
                                               p.Description.ToLower().Contains(search));
            }

            // 2. Filtrare dupa categorie
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // 3. Sortare
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_desc":
                    // Pentru rating e mai complex deoarece e calculat in memorie (not mapped)
                    // Daca vrem sortare in DB, ar trebui sa executam query-ul si apoi sa sortam in memorie
                    // SAU sa persistam rating-ul. 
                    // Pentru simplitate si performanta pe seturi mici, aducem in memorie apoi sortam.
                    // Totusi, pentru query eficient, vom lasa default sortarea implicita din baza, 
                    // iar daca userul cere rating, o facem in-memory la final (ToList).
                    break;
                default:
                    // Default: cele mai recente sau dupa ID
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            var productList = products.ToList();

            // Sortare in-memory pentru Rating (fiindca e calculat si NotMapped)
            if (sortOrder == "rating_desc")
            {
                productList = productList.OrderByDescending(p => p.Rating).ToList();
            }

            return View(productList);
        }

        [AllowAnonymous]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)!
                .ThenInclude(r => r.User)
                .FirstOrDefault(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET param pentru a lista produse in asteptare
        [Authorize(Roles = "Administrator")]
        public IActionResult PendingIndex()
        {
            var products = db.Products.Include("Category")
                                      .Where(p => p.Status == ProductStatus.Pending)
                                      .ToList();
            return View(products);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                product.Status = ProductStatus.Approved;
                db.SaveChanges();
                TempData["message"] = "Produsul a fost aprobat!";
            }
            return RedirectToAction("PendingIndex");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                product.Status = ProductStatus.Rejected;
                db.SaveChanges();
                TempData["message"] = "Produsul a fost respins!";
            }
            return RedirectToAction("PendingIndex");
        }

        // ISTORIC PRODUSE PROPRII (Colaboratori + Admini)
        [Authorize(Roles = "Colaborator,Administrator")]
        public IActionResult MyProducts()
        {
            var userId = _userManager.GetUserId(User);
            var products = db.Products.Include("Category")
                                      .Where(p => p.UserId == userId)
                                      .OrderByDescending(p => p.Id) // Cele mai recente primele
                                      .ToList();
            return View(products);
        }

        // GET: Products/Create
        [Authorize(Roles = "Colaborator,Administrator")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [Authorize(Roles = "Colaborator,Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? ProductImage)
        {
            if (ProductImage != null)
            {
                // Validare tip fisier (trebuie sa fie imagine)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(ProductImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "Fisierul trebuie sa fie o imagine (.jpg, .jpeg, .png, .gif, .bmp, .webp).");
                }

                // Validare dimensiune fisier (max 5MB)
                if (ProductImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProductImage", "Dimensiunea imaginii nu poate depasi 5MB.");
                }
            }
            else
            {
                ModelState.AddModelError("ProductImage", "Imaginea este obligatorie/necesara!");
            }

            if (ModelState.IsValid)
            {
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    // Generam cale stocare
                    var storagePath = Path.Combine(_env.WebRootPath, "images", ProductImage.FileName);
                    var databasePath = "/images/" + ProductImage.FileName;

                    // Salvam fisierul
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await ProductImage.CopyToAsync(fileStream);
                    }
                    product.ImagePath = databasePath;
                }

                // Setam user-ul curent si statusul
                product.UserId = _userManager.GetUserId(User);
                
                if (User.IsInRole("Administrator"))
                {
                    product.Status = ProductStatus.Approved;
                    TempData["message"] = "Produsul a fost adaugat!";
                }
                else
                {
                    product.Status = ProductStatus.Pending;
                    TempData["message"] = "Produsul a fost trimis spre aprobare";
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Colaborator,Administrator")]
        public IActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            
            // Optional: Check if user is owner or admin
            // if (User.IsInRole("Colaborator") && product.UserId != _userManager.GetUserId(User)) { return Forbid(); }

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [Authorize(Roles = "Colaborator,Administrator")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ProductImage)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ProductImage != null)
            {
                // Validare tip fisier
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(ProductImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "Fisierul trebuie sa fie o imagine (.jpg, .jpeg, .png, .gif, .bmp, .webp).");
                }

                // Validare dimensiune fisier (max 5MB)
                if (ProductImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProductImage", "Dimensiunea imaginii nu poate depasi 5MB.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ProductImage != null && ProductImage.Length > 0)
                    {
                        var storagePath = Path.Combine(_env.WebRootPath, "images", ProductImage.FileName);
                        var databasePath = "/images/" + ProductImage.FileName;

                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await ProductImage.CopyToAsync(fileStream);
                        }
                        product.ImagePath = databasePath;
                    }
                    else
                    {
                        var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
                        if (existingProduct != null)
                        {
                            product.ImagePath = existingProduct.ImagePath;
                            // Ensure we don't lose status or userId if hidden fields weren't enough, 
                            // though hidden fields in View should handle this. Added safety:
                            if (string.IsNullOrEmpty(product.UserId)) product.UserId = existingProduct.UserId;
                            // Keep existing status or reset to Pending on edit? 
                            // Usually edits require re-approval. Let's set it to Pending on Edit if User is Colaborator.
                            if (User.IsInRole("Colaborator")) 
                            { 
                                product.Status = ProductStatus.Pending; 
                            }
                            else 
                            {
                                // Admin keeps the status (or whatever was passed)
                                if (product.Status == 0) product.Status = existingProduct.Status; // if 0/default
                            }
                        }
                    }

                    db.Update(product);
                    await db.SaveChangesAsync();
                    TempData["message"] = "Produsul a fost modificat";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!db.Products.Any(p => p.Id == product.Id))
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

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = db.Products.Find(id);
            if(product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
            }
            return RedirectToAction("Index");
        }
        // POST: Products/AddReview
        [Authorize]
        [HttpPost]
        public IActionResult AddReview([FromForm] Review review)
        {
            if (ModelState.IsValid)
            {
                // Setam valorile implicite
                review.Date = DateTime.UtcNow;
                review.UserId = _userManager.GetUserId(User);

                // Nu putem avea review fara continut si fara rating
                if (string.IsNullOrWhiteSpace(review.Content) && review.Rating == null)
                {
                    TempData["message"] = "Trebuie sa adaugi un text sau un rating!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Details", new { id = review.ProductId });
                }

                db.Reviews.Add(review);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost adaugat!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Nu am putut adauga review-ul. Verifica datele.";
                TempData["messageType"] = "alert-danger";
            }
            return RedirectToAction("Details", new { id = review.ProductId });
        }

        // POST: Products/DeleteReview
        [Authorize]
        [HttpPost]
        public IActionResult DeleteReview(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            // Verificam drepturile: Admin sau Proprietarul review-ului
            if (User.IsInRole("Administrator") || review.UserId == _userManager.GetUserId(User))
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost sters!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Nu ai dreptul sa stergi acest review!";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Details", new { id = review.ProductId });
        }
        // GET: Products/FaqHistory
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> FaqHistory()
        {
            var faqs = await db.ProductFaqs
                .Include(f => f.Product)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
            
            return View(faqs);
        }
    }
}

