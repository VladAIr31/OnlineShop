using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;

        public ProductsController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var products = db.Products.Include("Category").ToList();
            return View(products);
        }
    }
}