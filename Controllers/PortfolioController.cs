using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.ViewModels; // Added for ViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ApplicationDbContext _context; // Keep for direct DB access in Clone/Chart

        public PortfolioController(IPortfolioService portfolioService, ApplicationDbContext context)
        {
            _portfolioService = portfolioService;
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // It's safer to throw an exception or handle this explicitly
            // if userIdClaim is expected to always be present due to [Authorize]
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("User ID claim not found. User must be authenticated.");
            }
            return Guid.Parse(userIdClaim.Value);
        }

        // GET: /Portfolio
        public async Task<IActionResult> Index()
        {
            Guid userId = GetCurrentUserId();
            var portfolios = await _portfolioService.GetAllPortfoliosAsync(userId);
            return View(portfolios);
        }

        // GET: /Portfolio/Create
        public IActionResult Create()
        {
            // Pass an empty ViewModel to the view for form binding
            return View(new PortfolioViewModel());
        }

        // POST: /Portfolio/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortfolioViewModel model) // Changed to accept PortfolioViewModel
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return the model back to the view
                // This will display validation error messages.
                TempData["Error"] = "Validation failed. Please correct the errors.";
                return View(model);
            }

            // Get current logged-in user's ID
            Guid userId = GetCurrentUserId();

            // Map ViewModel to Model
            var portfolio = new Portfolio
            {
                PortfolioId = Guid.NewGuid(), // Generate new GUID for primary key
                UserId = userId,
                PortfolioName = model.PortfolioName,
                Description = model.Description,
                Type = model.Type,  
                TotalValue = model.TotalValue,
                CreatedDate = DateTime.UtcNow // Set creation date
            };

            await _portfolioService.AddPortfolioAsync(portfolio);
            TempData["Success"] = "Portfolio created successfully!";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Portfolio/Edit/id
        public async Task<IActionResult> Edit(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            // Optional: Map Portfolio model back to a ViewModel for editing if needed
            // For now, directly using Portfolio model for Edit is fine if no extra view-specific logic.
            return View(portfolio);
        }

        // POST: /Portfolio/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken] // Added for consistency with Create
        public async Task<IActionResult> Edit(EditPortfolioRequest editPortfolioRequest)
        {

            var portfolio = new Portfolio
            {
                PortfolioName = editPortfolioRequest.PortfolioName,
                Description = editPortfolioRequest.Description,
                Type = editPortfolioRequest.Type,
                TotalValue = (decimal)editPortfolioRequest.TotalValue,
                UserId = GetCurrentUserId()
            };
            var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(portfolio);
            
             if(updatedPortfolio != null)
            {
                TempData["Success"] = "Portfolio updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(portfolio); 
        }

        // GET: /Portfolio/Details/id
        public async Task<IActionResult> Details(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            // Optional: Authorization check
            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to view this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            return View(portfolio);
        }

        // GET: /Portfolio/Delete/id
        // This action displays the confirmation page for deleting a portfolio.
        public async Task<IActionResult> Delete(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                TempData["Error"] = "Portfolio not found."; // More specific error
                return RedirectToAction(nameof(Index)); // Redirect instead of NotFound() view
            }

            // Authorization check: Ensure the logged-in user owns this portfolio.
            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to delete this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            // If everything is fine, show the delete confirmation view.
            return View(portfolio);
        }


        // POST: /Portfolio/DeleteConfirmed
        // This action handles the actual deletion after user confirmation.
        [HttpPost, ActionName("Delete")] // Maps to "Delete" action but uses POST
        [ValidateAntiForgeryToken]       // Important for security to prevent XSRF attacks
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // First, retrieve the portfolio to perform the authorization check.
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                TempData["Error"] = "Portfolio not found for deletion."; // More specific error
                return RedirectToAction(nameof(Index));
            }

            // Authorization check: Double-check if the logged-in user owns this portfolio.
            // This is crucial, even if checked in the GET, as POST requests can be forged.
            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to delete this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            // If authorized, proceed with deletion.
            await _portfolioService.DeletePortfolioAsync(id);

            // Set success message and redirect to the list of portfolios.
            TempData["Success"] = "Portfolio deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        // 🔁 POST: /Portfolio/Clone
        [HttpPost]
        [ValidateAntiForgeryToken] // Added for security
        public async Task<IActionResult> Clone(Guid id)
        {
            var original = await _context.Portfolios
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (original == null) return NotFound();

            // Authorization check: User can only clone their own portfolios
            if (original.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to clone this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            var clonedPortfolio = new Portfolio
            {
                UserId = original.UserId,
                PortfolioName = original.PortfolioName + " (Copy)",
                Description = original.Description,
                Type = original.Type, // Ensure Type is copied
                TotalValue = original.TotalValue, // Ensure TotalValue is copied
                CreatedDate = DateTime.UtcNow,
                // PortfolioId will be generated by Guid.NewGuid() when added to context
            };

            _context.Portfolios.Add(clonedPortfolio);
            await _context.SaveChangesAsync();

            // Clone assets
            if (original.Assets != null)
            {
                foreach (var asset in original.Assets)
                {
                    var clonedAsset = new Asset
                    {
                        PortfolioId = clonedPortfolio.PortfolioId,
                        AssetName = asset.AssetName,
                        AssetType = asset.AssetType,
                        Quantity = asset.Quantity,
                        PurchasePrice = asset.PurchasePrice,
                        // Add other asset properties if they exist
                    };
                    _context.Assets.Add(clonedAsset);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Portfolio cloned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // 📊 GET: /Portfolio/AllocationChart/id
        public async Task<IActionResult> AllocationChart(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            // Authorization check
            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to view this chart.";
                return RedirectToAction(nameof(Index));
            }

            var data = await _context.Assets
                .Where(a => a.PortfolioId == id)
                .GroupBy(a => a.AssetType)
                .Select(g => new
                {
                    Type = g.Key,
                    TotalAmount = g.Sum(a => a.PurchasePrice * a.Quantity)
                })
                .ToListAsync();

            ViewBag.PortfolioId = id;
            return View(data);
        }
    }
}