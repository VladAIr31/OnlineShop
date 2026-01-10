using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Services;

namespace OnlineShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductChatController : ControllerBase
    {
        private readonly IAiAssistantService _aiService;
        private readonly ApplicationDbContext _context;

        public ProductChatController(IAiAssistantService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Intrebarea lipseste.");
            }

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return NotFound("Produsul nu a fost gasit.");
            }

            // Call AI Service
            var answer = await _aiService.GetAnswerAsync(request.Question, product);

            // Save to DB (Optional: only if valid answer)
            if (!string.IsNullOrEmpty(answer))
            {
                _context.ProductFaqs.Add(new ProductFaq
                {
                    ProductId = request.ProductId,
                    Question = request.Question,
                    Answer = answer,
                    Date = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { answer });
        }
    }

    public class ChatRequest
    {
        public int ProductId { get; set; }
        public string Question { get; set; }
    }
}
