using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class CollaboratorRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CollaboratorRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin View (List of requests)
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.CollaboratorRequests
                .Include(r => r.User)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        // GET: User Apply View
        public async Task<IActionResult> Apply()
        {
            // If user is already a collaborator or admin, redirect
            if (User.IsInRole("Colaborator") || User.IsInRole("Administrator"))
            {
                TempData["message"] = "Esti deja colaborator sau administrator!";
                TempData["messageType"] = "alert-info";
                return RedirectToAction("Index", "Home");
            }

            // Check if user has a pending request
            var userId = _userManager.GetUserId(User);
            var existingRequest = await _context.CollaboratorRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);

            if (existingRequest != null)
            {
                 return RedirectToAction("RequestSent");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(CollaboratorRequest request)
        {
            // Remove UserId/User from validation because we set them manually
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                 // Double check existing
                var existingRequest = await _context.CollaboratorRequests
                    .FirstOrDefaultAsync(r => r.UserId == user.Id && r.Status == RequestStatus.Pending);

                if (existingRequest != null) return RedirectToAction("RequestSent");

                request.UserId = user.Id;
                request.RequestDate = DateTime.UtcNow;
                request.Status = RequestStatus.Pending;

                _context.CollaboratorRequests.Add(request);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("RequestSent");
            }
            return View(request);
        }

        public IActionResult RequestSent()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.CollaboratorRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Approved;
                request.Seen = false; // Trigger notification
                
                // Assign Role
                if (!await _userManager.IsInRoleAsync(request.User, "Colaborator"))
                {
                    await _userManager.AddToRoleAsync(request.User, "Colaborator");
                }

                await _context.SaveChangesAsync();
                TempData["message"] = $"Cererea utilizatorului {request.User.Email} a fost aprobata!";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.CollaboratorRequests.FindAsync(id);
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Rejected;
                request.Seen = false; // Trigger notification
                await _context.SaveChangesAsync();
                TempData["message"] = "Cererea a fost respinsa.";
                TempData["messageType"] = "alert-warning";
            }
            return RedirectToAction("Index");
        }
        

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ManageCollaborators()
        {
            var collaborators = await _userManager.GetUsersInRoleAsync("Colaborator");
            return View(collaborators);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Revoke(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Remove Role
                await _userManager.RemoveFromRoleAsync(user, "Colaborator");

                // Update Request Status
                var request = await _context.CollaboratorRequests
                    .Where(r => r.UserId == userId && r.Status == RequestStatus.Approved)
                    .OrderByDescending(r => r.RequestDate)
                    .FirstOrDefaultAsync();

                if (request != null)
                {
                    request.Status = RequestStatus.Revoked;
                    request.Seen = false;
                    await _context.SaveChangesAsync();
                }

                TempData["message"] = $"Drepturile de colaborator pentru {user.Email} au fost revocate.";
                TempData["messageType"] = "alert-warning";
            }
            return RedirectToAction("ManageCollaborators");
        }
    }
}
