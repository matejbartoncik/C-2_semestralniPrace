using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Controllers;

public class OrdersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(OrderStatus? status, string? propertyId, string? technicianId)
    {
        var query = db.Orders
            .Include(o => o.Property)
       .ThenInclude(p => p!.Owner)
            .Include(o => o.AssignedTechnician)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(propertyId))
            query = query.Where(o => o.PropertyId == propertyId);

        if (!string.IsNullOrWhiteSpace(technicianId))
            query = query.Where(o => o.AssignedTechnicianId == technicianId);

        var items = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();

        ViewBag.Properties = await db.Properties
              .Include(p => p.Owner)
         .OrderBy(p => p.Title)
    .ToListAsync();

        ViewBag.Technicians = await db.Users
            .Where(u => u.Role == UserRole.Technician)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Details(string id)
    {
        var order = await db.Orders
       .Include(o => o.Property)
            .ThenInclude(p => p!.Owner)
       .Include(o => o.AssignedTechnician)
  .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Properties = await db.Properties
   .Include(p => p.Owner)
            .OrderBy(p => p.Title)
        .ToListAsync();

        ViewBag.Technicians = await db.Users
                  .Where(u => u.Role == UserRole.Technician)
                  .OrderBy(u => u.Name)
          .ToListAsync();

        return View(new Order());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Properties = await db.Properties
        .Include(p => p.Owner)
                   .OrderBy(p => p.Title)
           .ToListAsync();

            ViewBag.Technicians = await db.Users
      .Where(u => u.Role == UserRole.Technician)
    .OrderBy(u => u.Name)
      .ToListAsync();

            return View(model);
        }

        model.CreatedAtUtc = DateTime.UtcNow;
        db.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        ViewBag.Properties = await db.Properties
   .Include(p => p.Owner)
  .OrderBy(p => p.Title)
            .ToListAsync();

        ViewBag.Technicians = await db.Users
         .Where(u => u.Role == UserRole.Technician)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Order model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Properties = await db.Properties
         .Include(p => p.Owner)
              .OrderBy(p => p.Title)
               .ToListAsync();

            ViewBag.Technicians = await db.Users
                .Where(user => user.Role == UserRole.Technician)
            .OrderBy(u => u.Name)
               .ToListAsync();

            return View(model);
        }

        db.Update(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        db.Remove(order);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
