using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;
using semestralniPraceBartoncik.Helpers;

namespace semestralniPraceBartoncik.Controllers;

public class OrdersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(OrderStatus? status, string? propertyId, string? technicianId, string? search)
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

  // Vyhledávání
        if (!string.IsNullOrWhiteSpace(search))
   {
         search = search.ToLower();
  query = query.Where(o =>
         o.Description.ToLower().Contains(search) ||
    (o.Property != null && o.Property.Title.ToLower().Contains(search)) ||
       (o.Property != null && o.Property.Address.ToLower().Contains(search)));
        }

     var items = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();

 ViewBag.Properties = await db.Properties
     .Include(p => p.Owner)
         .OrderBy(p => p.Title)
    .ToListAsync();

        ViewBag.Technicians = await db.Users
     .Where(u => u.Role == UserRole.Technician)
     .OrderBy(u => u.Name)
         .ToListAsync();
        
  ViewBag.CurrentSearch = search;

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
    
    // ===== EXPORT =====
    
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var orders = await db.Orders
    .Include(o => o.Property)
    .ThenInclude(p => p!.Owner)
        .Include(o => o.AssignedTechnician)
    .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        var bytes = ImportExportHelper.ExportOrdersToCsv(orders);
        return File(bytes, "text/csv; charset=utf-8", $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    // ===== IMPORT =====
  
    public IActionResult Import() => View();

    [HttpPost]
public async Task<IActionResult> Import(IFormFile file)
{
   if (file == null || file.Length == 0)
        {
  TempData["Error"] = "Prosím vyberte soubor.";
      return RedirectToAction(nameof(Import));
  }

        try
     {
  using var reader = new StreamReader(file.OpenReadStream());
var csvContent = await reader.ReadToEndAsync();

     // Create lookups
   var propertyLookup = await db.Properties.ToDictionaryAsync(p => p.Title, p => p.Id);
         var technicianLookup = await db.Users
       .Where(u => u.Role == UserRole.Technician)
    .ToDictionaryAsync(u => u.Name, u => u.Id);

       var orders = ImportExportHelper.ImportOrdersFromCsv(csvContent, propertyLookup, technicianLookup);
     
        db.Orders.AddRange(orders);
    await db.SaveChangesAsync();

      TempData["Success"] = $"Úspìšnì importováno {orders.Count} zakázek.";
    }
   catch (Exception ex)
{
TempData["Error"] = $"Chyba pøi importu: {ex.Message}";
        }

  return RedirectToAction(nameof(Index));
    }
}
