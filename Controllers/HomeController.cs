using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models;
using OnlineShop.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromServices] ApplicationDbContext _context, [FromServices] UserManager<ApplicationUser> _userManager)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var request = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                        _context.CollaboratorRequests.Where(r => r.UserId == user.Id && !r.Seen && r.Status != RequestStatus.Pending)
                        .OrderByDescending(r => r.RequestDate));

                    if (request != null)
                    {
                        if (request.Status == RequestStatus.Approved)
                        {
                            TempData["message"] = "Felicitari! Cererea ta de a deveni colaborator a fost APROBATA! Acum poti adauga produse.";
                            TempData["messageType"] = "alert-success";
                        }
                        else if (request.Status == RequestStatus.Rejected)
                        {
                            TempData["message"] = "Din pacate, cererea ta de a deveni colaborator a fost RESPINSA. Poti incerca din nou mai tarziu.";
                            TempData["messageType"] = "alert-danger";
                        }
                        else if (request.Status == RequestStatus.Revoked)
                        {
                            TempData["message"] = "Drepturile tale de colaborator au fost REVOCATE de un administrator.";
                            TempData["messageType"] = "alert-warning";
                        }

                        request.Seen = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Confirmation(string title, string message, string primaryAction, string primaryController, string primaryText, string secondaryAction, string secondaryController, string secondaryText)
        {
            dynamic model = new System.Dynamic.ExpandoObject();
            model.Title = title;
            model.Message = message;
            model.PrimaryAction = primaryAction;
            model.PrimaryController = primaryController;
            model.PrimaryText = primaryText;
            model.SecondaryAction = secondaryAction;
            model.SecondaryController = secondaryController;
            model.SecondaryText = secondaryText;

            return View(model);
        }
    }
}
