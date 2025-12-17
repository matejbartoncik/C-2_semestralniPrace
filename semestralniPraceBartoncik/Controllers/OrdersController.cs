using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;
using semestralniPraceBartoncik.Helpers;
using semestralniPraceBartoncik.Services;

namespace semestralniPraceBartoncik.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly GoogleCalendarService _calendarService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        ApplicationDbContext db,
        GoogleCalendarService calendarService,
        ILogger<OrdersController> logger)
    {
        _db = db;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(OrderStatus? status, string? propertyId, string? technicianId, string? search)
    {
        var query = _db.Orders
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

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
           o.Description.ToLower().Contains(search) ||
           (o.Property != null && o.Property.Title.ToLower().Contains(search)) ||
           (o.Property != null && o.Property.Address.ToLower().Contains(search)));
        }

        var items = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();

        ViewBag.Properties = await _db.Properties
       .Include(p => p.Owner)
        .OrderBy(p => p.Title)
     .ToListAsync();

        ViewBag.Technicians = await _db.Users
         .Where(u => u.Role == UserRole.Technician)
            .OrderBy(u => u.Name)
        .ToListAsync();

        ViewBag.CurrentSearch = search;

        return View(items);
    }

    public async Task<IActionResult> Details(string id)
    {
        var order = await _db.Orders
   .Include(o => o.Property)
          .ThenInclude(p => p!.Owner)
       .Include(o => o.AssignedTechnician)
.FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Properties = await _db.Properties
     .Include(p => p.Owner)
         .OrderBy(p => p.Title)
         .ToListAsync();

        ViewBag.Technicians = await _db.Users
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
            ViewBag.Properties = await _db.Properties
     .Include(p => p.Owner)
   .OrderBy(p => p.Title)
              .ToListAsync();

            ViewBag.Technicians = await _db.Users
                   .Where(u => u.Role == UserRole.Technician)
                       .OrderBy(u => u.Name)
                   .ToListAsync();

            return View(model);
        }

        model.CreatedAtUtc = DateTime.UtcNow;
        _db.Add(model);
        await _db.SaveChangesAsync();


        if (model.ScheduledFromUtc.HasValue && model.ScheduledToUtc.HasValue)
        {

            var orderToSync = await _db.Orders
               .Include(o => o.Property)
    .ThenInclude(p => p.Owner)
        .Include(o => o.AssignedTechnician)
               .FirstOrDefaultAsync(o => o.Id == model.Id);

            var syncSuccess = await _calendarService.SyncOrderToCalendarAsync(orderToSync);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        ViewBag.Properties = await _db.Properties
               .Include(p => p.Owner)
               .OrderBy(p => p.Title)
     .ToListAsync();

        ViewBag.Technicians = await _db.Users
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
            ViewBag.Properties = await _db.Properties
            .Include(p => p.Owner)
              .OrderBy(p => p.Title)
      .ToListAsync();

            ViewBag.Technicians = await _db.Users
                    .Where(u => u.Role == UserRole.Technician)
          .OrderBy(u => u.Name)
               .ToListAsync();

            return View(model);
        }

        _db.Update(model);
        await _db.SaveChangesAsync();

        var orderToSync = await _db.Orders
         .Include(o => o.Property)
              .ThenInclude(p => p.Owner)
     .Include(o => o.AssignedTechnician)
            .FirstOrDefaultAsync(o => o.Id == model.Id);

        var syncSuccess = await _calendarService.SyncOrderToCalendarAsync(orderToSync);


        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        await _calendarService.RemoveOrderFromCalendarAsync(order);

        _db.Remove(order);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> SyncCalendar(string id)
    {
        var order = await _db.Orders
           .Include(o => o.Property)
        .ThenInclude(p => p.Owner)
       .Include(o => o.AssignedTechnician)
              .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            TempData["Error"] = "Zakázka nebyla nalezena.";
            return RedirectToAction(nameof(Index));
        }

        if (!order.ScheduledFromUtc.HasValue || !order.ScheduledToUtc.HasValue)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var success = await _calendarService.SyncOrderToCalendarAsync(order);


        return RedirectToAction(nameof(Details), new { id });
    }
    // ===== EXPORT =====

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var orders = await _db.Orders
            .Include(o => o.Property)
            .ThenInclude(p => p!.Owner)
       .Include(o => o.AssignedTechnician)
      .OrderByDescending(o => o.CreatedAtUtc)
         .ToListAsync();

        var bytes = ImportExportHelper.ExportOrdersToCsv(orders);

        return File(bytes, "text/csv", $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

            var propertyLookup = await _db.Properties.ToDictionaryAsync(p => p.Title, p => p.Id);
            var technicianLookup = await _db.Users
            .Where(u => u.Role == UserRole.Technician)
            .ToDictionaryAsync(u => u.Name, u => u.Id);

            var orders = ImportExportHelper.ImportOrdersFromCsv(csvContent, propertyLookup, technicianLookup);

            _db.Orders.AddRange(orders);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Úspìšnì importováno {orders.Count} zakázek.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Chyba pøi importu: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
