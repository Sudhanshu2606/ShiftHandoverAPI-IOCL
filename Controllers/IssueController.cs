using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftHandoverAPI.Data;
using ShiftHandoverAPI.Models;

namespace ShiftHandoverAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IssueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IssueController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("handover/{handoverId}")]
        public async Task<IActionResult> GetByHandover(int handoverId)
        {
            var issues = await _context.Issues
                .Where(i => i.HandoverId == handoverId)
                .ToListAsync();
            return Ok(issues);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Issue issue)
        {
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return Ok(issue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Issue updatedIssue)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            issue.Title = updatedIssue.Title;
            issue.Description = updatedIssue.Description;
            issue.Severity = updatedIssue.Severity;
            issue.Status = updatedIssue.Status;

            await _context.SaveChangesAsync();
            return Ok(issue);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }
    }
}