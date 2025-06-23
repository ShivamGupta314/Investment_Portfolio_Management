using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // Eagerly load Investments and their associated Assets for each portfolio
            // This ensures all necessary data for report generation is available on the client-side
            // without additional database calls when the download button is clicked.
            var portfolios = await _context.Portfolios
                                            .Where(p => p.UserId == userId)
                                            .Include(p => p.Investments)
                                                .ThenInclude(i => i.Asset) // Include Asset details for each investment
                                            .ToListAsync();
            return View(portfolios);
        }


        [HttpGet]
        public async Task<IActionResult> GetLivePortfolioTotalValues()
        {
            Guid userId = GetCurrentUserId(); // Get the current user's ID

            var portfolios = await _context.Portfolios
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    portfolioId = p.PortfolioId,
                    totalValue = p.TotalValue
                })
                .ToListAsync();

            return Json(portfolios);
        }
        [HttpGet]
        public async Task<IActionResult> GetPortfolioLiveUpdates(Guid id)
        {
            // Ensure the portfolio and its investments/assets are loaded
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)
                    .ThenInclude(i => i.Asset) // Crucial for getting Asset.Name and Asset.CurrentPrice
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (portfolio == null || portfolio.UserId != GetCurrentUserId())
            {
                // Return an error or empty data if portfolio not found or unauthorized
                return NotFound(); // Or Json(new { success = false, message = "Portfolio not found or unauthorized." });
            }

            // Prepare investment data for JSON response
            var investmentData = portfolio.Investments.Select(inv => new
            {
                investmentId = inv.InvestmentId, // Include ID if you ever need to reference it
                assetId = inv.Asset.AssetId, // ADDED: Pass AssetId for chart functionality
                assetName = inv.Asset?.Name, // Use null-conditional operator for safety
                quantity = inv.Quantity,
                // **IMPORTANT:** Use Asset.CurrentPrice for the most up-to-date value
                // If Investment.CurrentPrice is just a cached copy.
                currentPrice = inv.Asset?.CurrentPrice ?? 0.0M, // Use decimal from Asset, default to 0
                purchasePrice = inv.PurchasePrice,
                // Calculate total value based on the Asset's current price for accuracy
                totalInvestmentValue = (inv.Asset?.CurrentPrice ?? 0.0M) * inv.Quantity,
                investmentDate = inv.InvestmentDate.ToShortDateString(), // Format date for display
                // P/L and Risk Level will be calculated client-side for simplicity,
                // using the already fetched data.
                // Add AssetType for risk level determination on client side
                assetType = inv.Asset?.AssetType.ToString()
            }).ToList();

            // Return consolidated data including portfolio's TotalValue
            return Json(new
            {
                totalValue = portfolio.TotalValue, // This is already updated by background service/AssetService
                investments = investmentData
            });
        }

        // --- NEW ACTION FOR CHART DATA ---
        [HttpGet]
        public async Task<IActionResult> GetAssetCurrentPrice(string id) // 'id' will be assetId (string Guid)
        {
            // Ensure the passed 'id' is a valid GUID
            if (!Guid.TryParse(id, out Guid assetGuid))
            {
                // Log the error or return a specific error message
                return BadRequest("Invalid asset ID format.");
            }

            // Fetch only the necessary data (CurrentPrice and Name) for the given AssetId
            var asset = await _context.Assets
                                        .Where(a => a.AssetId == assetGuid)
                                        .Select(a => new { a.CurrentPrice, a.Name })
                                        .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound(new { message = "Asset not found." });
            }

            // Return the current price and name as JSON
            return Json(new { currentPrice = asset.CurrentPrice, assetName = asset.Name });
        }

        // GET: /Portfolio/Create
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

        // POST: /Portfolio/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortfolioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Validation failed. Please correct the errors.";
                return View(model);
            }

            Guid userId = GetCurrentUserId();

            var portfolio = new Portfolio
            {
                PortfolioId = Guid.NewGuid(),
                UserId = userId,
                Name = model.PortfolioName,
                Description = model.Description,
                //Type = model.Type,
                TotalValue = 0.0M, // Initialize TotalValue to 0.0 when creating
                CreatedDate = DateTime.UtcNow
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

            // When editing, you might want to map to an EditViewModel
            // if the fields differ significantly from the Portfolio model.
            // For now, if the Portfolio model directly maps to your edit form,
            // you can pass it directly.
            // You might want to create an EditPortfolioViewModel that matches the form fields.
            var editViewModel = new EditPortfolioRequest // Assuming EditPortfolioRequest is your existing ViewModel for editing
            {
                PortfolioId = portfolio.PortfolioId,
                PortfolioName = portfolio.Name,
                Description = portfolio.Description,
                //Type = portfolio.Type,
                TotalValue = (decimal?)(double)portfolio.TotalValue // Still pass the current value for display, but it won't be used for update
            };
            return View(editViewModel); // Pass the ViewModel to the view
        }

        // POST: /Portfolio/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPortfolioRequest editPortfolioRequest)
        {
            // You might want to add ModelState.IsValid check here too for Edit
            // if (!ModelState.IsValid)
            // {
            //    TempData["Error"] = "Validation failed. Please correct the errors.";
            //    return View(editPortfolioRequest);
            // }

            var portfolio = new Portfolio
            {
                PortfolioId = editPortfolioRequest.PortfolioId,
                Name = editPortfolioRequest.PortfolioName,
                Description = editPortfolioRequest.Description,
                //Type = editPortfolioRequest.Type,
                // TotalValue is NOT updated from the request; it's calculated dynamically
                // TotalValue = (decimal)editPortfolioRequest.TotalValue, // REMOVED
                UserId = GetCurrentUserId()
            };

            var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(portfolio);

            if (updatedPortfolio != null)
            {
                // After updating basic portfolio details, recalculate its total value
                await _portfolioService.CalculateAndSetPortfolioTotalValueAsync(updatedPortfolio.PortfolioId);
                TempData["Success"] = "Portfolio updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Failed to update portfolio."; // Add error message for null update
            return View(editPortfolioRequest); // Return the request model to keep form data
        }

        // GET: /Portfolio/Details/id
        public async Task<IActionResult> Details(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to view this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Investments = portfolio.Investments ?? new List<Investment>();
            return View(portfolio);
        }

        // GET: /Portfolio/Delete/id
        public async Task<IActionResult> Delete(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                TempData["Error"] = "Portfolio not found.";
                return RedirectToAction(nameof(Index));
            }

            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to delete this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            return View(portfolio);
        }

        // POST: /Portfolio/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                TempData["Error"] = "Portfolio not found for deletion.";
                return RedirectToAction(nameof(Index));
            }

            if (portfolio.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to delete this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            await _portfolioService.DeletePortfolioAsync(id);

            TempData["Success"] = "Portfolio deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // 🔁 POST: /Portfolio/Clone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(Guid id)
        {
            var original = await _context.Portfolios
                .Include(p => p.Assets)
                .Include(p => p.Investments) // Include Investments
                    .ThenInclude(i => i.Asset) // Include Asset for each Investment
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (original == null) return NotFound();

            if (original.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "You are not authorized to clone this portfolio.";
                return RedirectToAction(nameof(Index));
            }

            var clonedPortfolio = new Portfolio
            {
                UserId = original.UserId,
                Name = original.Name + " (Copy)",
                Description = original.Description,
                //Type = original.Type,
                TotalValue = 0.0M, // Cloned portfolio starts with 0 value, will be recalculated
                CreatedDate = DateTime.UtcNow,
                Assets = new List<Asset>(),      // Initialize the collection
                Investments = new List<Investment>() // Initialize the collection
            };

            _context.Portfolios.Add(clonedPortfolio);

            if (original.Assets != null)
            {
                foreach (var asset in original.Assets)
                {
                    var clonedAsset = new Asset
                    {
                        PortfolioId = clonedPortfolio.PortfolioId,
                        Name = asset.Name,
                        Symbol = asset.Symbol,
                        AssetType = asset.AssetType,
                        BasePrice = asset.BasePrice,
                        CurrentPrice = asset.CurrentPrice,
                        Quantity = asset.Quantity,
                        TotalValue = asset.TotalValue,
                        PurchasedDate = asset.PurchasedDate
                    };
                    _context.Assets.Add(clonedAsset);
                    clonedPortfolio.Assets.Add(clonedAsset);
                }
            }

            if (original.Investments != null)
            {
                foreach (var investment in original.Investments)
                {
                    var clonedInvestment = new Investment
                    {
                        UserId = investment.UserId,
                        AssetId = investment.AssetId,
                        PortfolioId = clonedPortfolio.PortfolioId,
                        Quantity = investment.Quantity,
                        AssetName = investment.AssetName,
                        PurchasePrice = investment.PurchasePrice,
                        CurrentPrice = investment.CurrentPrice,
                        InvestmentDate = investment.InvestmentDate
                    };
                    _context.Investments.Add(clonedInvestment);
                    clonedPortfolio.Investments.Add(clonedInvestment);
                }
            }

            await _context.SaveChangesAsync(); // Save all cloned entities

            // After cloning, calculate and set the total value for the new cloned portfolio
            await _portfolioService.CalculateAndSetPortfolioTotalValueAsync(clonedPortfolio.PortfolioId);

            TempData["Success"] = "Portfolio cloned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // 📊 GET: /Portfolio/AllocationChart/id
        [HttpGet]
        public async Task<IActionResult> AllocationChart(Guid id)
        {
            ViewBag.PortfolioId = id; // pass for JS use
            return View(); // view will use AJAX to fetch data
        }
        [HttpGet]

        public async Task<IActionResult> GetAllocationData(Guid id)

        {

            var portfolio = await _context.Portfolios

                .Include(p => p.Investments)

                    .ThenInclude(i => i.Asset)

                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (portfolio == null || portfolio.Investments == null || !portfolio.Investments.Any())

                return Json(new { success = false, message = "No investments found." });

            var data = portfolio.Investments

                .Where(i => i.Asset != null)

                .GroupBy(i => i.Asset.AssetType)

                .Select(g => new

                {

                    type = g.Key,

                    totalAmount = g.Sum(i => i.CurrentPrice * i.Quantity)

                })

                .ToList();

            return Json(data);

        }

        // GET: /Portfolio/AddInvestments/{portfolioId}
        [HttpGet]
        public async Task<IActionResult> AddInvestments(Guid portfolioId)
        {
            var userId = GetCurrentUserId();
            var unassignedInvestments = await _portfolioService.GetUnassignedInvestmentsAsync(userId);

            var viewModel = new AddInvestmentsToPortfolioViewModel
            {
                PortfolioId = portfolioId,
                UnassignedInvestments = unassignedInvestments
            };

            return View(viewModel);
        }

        // POST: /Portfolio/AddInvestments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInvestments(AddInvestmentsToPortfolioViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedInvestmentIds == null || model.SelectedInvestmentIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one investment.";
                model.UnassignedInvestments = await _portfolioService.GetUnassignedInvestmentsAsync(GetCurrentUserId());
                return View(model);
            }

            try
            {
                await _portfolioService.AssignInvestmentsToPortfolioAsync(model.PortfolioId, model.SelectedInvestmentIds);
                TempData["Success"] = "Investments successfully added to portfolio. Portfolio value updated.";
                return RedirectToAction(nameof(Details), new { id = model.PortfolioId }); // Redirect to details page
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
