using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;
using semestralniPraceBartoncik.Helpers;

namespace semestralniPraceBartoncik.Controllers;

public class UsersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
=> View(await db.Users.OrderBy(u => u.Name).ToListAsync());

public IActionResult Create() => View(new User());

    [HttpPost]
    public async Task<IActionResult> Create(User model)
    {
  if (!ModelState.IsValid) return View(model);
      
   db.Add(model);
   await db.SaveChangesAsync();
  return RedirectToAction(nameof(Index));
  }

    public async Task<IActionResult> Edit(string id)
  {
   var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
   return View(user);
    }

  [HttpPost]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid) return View(model);

  db.Update(model);
     await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
  var user = await db.Users.FindAsync(id);
     if (user == null) return NotFound();
     
    db.Remove(user);
   await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
  }

    // ===== EXPORT =====
    
    public IActionResult Export() => View();

    [HttpPost]
    public async Task<IActionResult> ExportCsv()
    {
     var users = await db.Users.OrderBy(u => u.Name).ToListAsync();
        var bytes = ImportExportHelper.ExportUsersToCsv(users);
   return File(bytes, "text/csv", $"users_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

  // ===== IMPORT =====
    
    public IActionResult Import() => View();

[HttpPost]
  public async Task<IActionResult> ImportCsv(IFormFile file)
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

   var users = ImportExportHelper.ImportUsersFromCsv(csvContent);

  db.Users.AddRange(users);
  await db.SaveChangesAsync();

  TempData["Success"] = $"Úspìšnì importováno {users.Count} uživatelù.";
        }
  catch (Exception ex)
{
TempData["Error"] = $"Chyba pøi importu: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
