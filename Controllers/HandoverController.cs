using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftHandoverAPI.Data;
using ShiftHandoverAPI.Models;
using System.Security.Claims;

namespace ShiftHandoverAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HandoverController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HandoverController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userShift = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.ShiftAssigned)
                .FirstOrDefaultAsync();

            IQueryable<HandoverReport> query = _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator);

            if (userRole != "Admin")
            {
                query = query.Where(h => h.ShiftName == userShift || h.CreatedBy == userId);
            }

            var reports = await query.OrderByDescending(h => h.Date).ToListAsync();
            return Ok(reports);
        }

        // GET: api/handover/reports - Admin only
        [HttpGet("reports")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            return Ok(reports);
        }

        // GET: api/handover/reports/date/{date} - Admin only
        [HttpGet("reports/date/{date}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportsByDate(string date)
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return BadRequest("Invalid date format");

            var reports = await _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator)
                .Where(h => h.Date.Date == targetDate.Date)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            return Ok(reports);
        }

        // GET: api/handover/reports/range?from=2024-01-01&to=2024-01-31 - Admin only
        [HttpGet("reports/range")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportsByDateRange([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
                return BadRequest("Invalid date format");

            var reports = await _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator)
                .Where(h => h.Date.Date >= fromDate.Date && h.Date.Date <= toDate.Date)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _context.HandoverReports
                .Include(h => h.Tasks)
                .Include(h => h.Issues)
                .Include(h => h.Creator)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (report == null)
                return NotFound(new { message = "Handover not found" });

            return Ok(report);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Create([FromBody] HandoverReport report)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            report.CreatedBy = userId;
            report.Date = DateTime.Now;

            _context.HandoverReports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(report);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] HandoverReport updatedReport)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var report = await _context.HandoverReports.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = "Handover not found" });

                if (userRole != "Admin" && report.CreatedBy != userId)
                    return Forbid("You can only edit your own handover reports");

                report.ShiftName = updatedReport.ShiftName;
                report.Summary = updatedReport.Summary;
                report.Status = updatedReport.Status;
                report.Unit = updatedReport.Unit ?? report.Unit;
                report.UnitName = updatedReport.UnitName ?? report.UnitName;

                await _context.SaveChangesAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.HandoverReports.FindAsync(id);
            if (report == null)
                return NotFound(new { message = "Handover not found" });

            _context.HandoverReports.Remove(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}