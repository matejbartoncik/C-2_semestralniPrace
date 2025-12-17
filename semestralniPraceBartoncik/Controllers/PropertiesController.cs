using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;
using semestralniPraceBartoncik.Helpers;

namespace semestralniPraceBartoncik.Controllers;

[Authorize]
public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public PropertiesController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var properties = await _db.Properties
            .Include(p => p.Owner)
            .OrderBy(p => p.Title)
            .ToListAsync();

        return View(properties);
    }

    public async Task<IActionResult> Details(string id)
    {
        var property = await _db.Properties
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return NotFound();
        return View(property);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Owners = await _db.Users
            .Where(u => u.Role == UserRole.Owner)
            .OrderBy(u => u.Name)
            .ToListAsync();
        
        ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
        
        return View(new Property());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Property model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Owners = await _db.Users
                .Where(u => u.Role == UserRole.Owner)
                .OrderBy(u => u.Name)
                .ToListAsync();
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
            return View(model);
        }

        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var property = await _db.Properties.FindAsync(id);
        if (property == null) return NotFound();

        ViewBag.Owners = await _db.Users
            .Where(u => u.Role == UserRole.Owner)
            .OrderBy(u => u.Name)
            .ToListAsync();
        
        ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
        
        return View(property);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Property model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Owners = await _db.Users
                .Where(u => u.Role == UserRole.Owner)
                .OrderBy(u => u.Name)
                .ToListAsync();
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
            return View(model);
        }

        _db.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var property = await _db.Properties.FindAsync(id);
        if (property == null) return NotFound();

        _db.Remove(property);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===== EXPORT =====
    
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var properties = await _db.Properties
            .Include(p => p.Owner)
            .OrderBy(p => p.Title)
            .ToListAsync();

        var bytes = ImportExportHelper.ExportPropertiesToCsv(properties);
        return File(bytes, "text/csv; charset=utf-8", $"properties_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

            var ownerLookup = await _db.Users
                .Where(u => u.Role == UserRole.Owner)
                .ToDictionaryAsync(u => u.Name, u => u.Id);

            var properties = ImportExportHelper.ImportPropertiesFromCsv(csvContent, ownerLookup);
            
            _db.Properties.AddRange(properties);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Úspìšnì importováno {properties.Count} nemovitostí.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Chyba pøi importu: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
