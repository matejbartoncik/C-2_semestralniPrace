using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Controllers;

public class CalendarController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? technicianId)
    {
      var technicians = await db.Users
            .Where(u => u.Role == UserRole.Technician)
        .OrderBy(u => u.Name)
   .ToListAsync();
     
        ViewBag.Technicians = technicians;
        ViewBag.SelectedTechnicianId = technicianId;
     
    return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetEvents(string? technicianId, DateTime? start, DateTime? end)
    {
        var query = db.Orders
      .Include(o => o.Property)
       .Include(o => o.AssignedTechnician)
      .Where(o => o.ScheduledFromUtc != null && o.ScheduledToUtc != null)
         .AsQueryable();
        
  if (!string.IsNullOrWhiteSpace(technicianId))
     {
            query = query.Where(o => o.AssignedTechnicianId == technicianId);
        }
        
        if (start.HasValue)
        {
          query = query.Where(o => o.ScheduledToUtc >= start.Value);
        }
        
   if (end.HasValue)
        {
            query = query.Where(o => o.ScheduledFromUtc <= end.Value);
        }
      
 var orders = await query.ToListAsync();
        
   // Pøevod na formát FullCalendar
        var events = orders.Select(o => new
        {
 id = o.Id,
      title = $"{o.AssignedTechnician?.Name ?? "Unassigned"} - {o.Property?.Title ?? "N/A"}",
            start = o.ScheduledFromUtc?.ToString("yyyy-MM-ddTHH:mm:ss"),
            end = o.ScheduledToUtc?.ToString("yyyy-MM-ddTHH:mm:ss"),
          description = o.Description,
         backgroundColor = GetTechnicianColor(o.AssignedTechnicianId),
  borderColor = GetTechnicianColor(o.AssignedTechnicianId),
     extendedProps = new
{
              orderId = o.Id,
        propertyTitle = o.Property?.Title,
       propertyAddress = o.Property?.Address,
         technicianName = o.AssignedTechnician?.Name,
                status = o.Status.ToString()
            }
     }).ToList();

        return Json(events);
    }
    
    private string GetTechnicianColor(string? technicianId)
    {
    if (string.IsNullOrEmpty(technicianId)) return "#6c757d"; // gray for unassigned
        
        // Generování konzistentní barvy podle technika
        var hash = technicianId.GetHashCode();
        var colors = new[] { "#3788d8", "#28a745", "#ffc107", "#dc3545", "#17a2b8", "#6610f2", "#e83e8c", "#fd7e14" };
      return colors[Math.Abs(hash) % colors.Length];
    }
}
