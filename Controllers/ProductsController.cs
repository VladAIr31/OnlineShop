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

        public IActionResult Index()
        {
            // Doar produsele aprobate sunt vizibile public
            var products = db.Products.Include("Category")
                                      .Where(p => p.Status == ProductStatus.Approved)
                                      .ToList();
            return View(products);
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
    }
}

