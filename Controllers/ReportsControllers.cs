using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftHandoverAPI.Data;

namespace ShiftHandoverAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetReports(DateTime startDate, DateTime endDate)
        {
            var reports = await _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator)
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            var summary = new
            {
                Total = reports.Count,
                Draft = reports.Count(r => r.Status == "Draft"),
                Submitted = reports.Count(r => r.Status == "Submitted"),
                TotalTasks = reports.Sum(r => r.Tasks?.Count ?? 0),
                CompletedTasks = reports.Sum(r => r.Tasks?.Count(t => t.Status == "Completed") ?? 0),
                TotalIssues = reports.Sum(r => r.Issues?.Count ?? 0),
                OpenIssues = reports.Sum(r => r.Issues?.Count(i => i.Status == "Open") ?? 0)
            };

            return Json(new { reports, summary });
        }
    }
}