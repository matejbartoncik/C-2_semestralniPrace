using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Controllers;

public class PropertiesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {

        var properties = await db.Properties
            .Include(p => p.Owner)
            .OrderBy(p => p.Title)
                  .ToListAsync();

        return View(properties);
    }

    public async Task<IActionResult> Details(string id)
    {
        var property = await db.Properties
  .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return NotFound();
        return View(property);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Owners = await db.Users
            .Where(u => u.Role == UserRole.Owner)
            .OrderBy(u => u.Name)
            .ToListAsync();
        return View(new Property());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Property model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Owners = await db.Users
       .Where(u => u.Role == UserRole.Owner)
            .OrderBy(u => u.Name)
    .ToListAsync();
            return View(model);
        }

        db.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var property = await db.Properties.FindAsync(id);
        if (property == null) return NotFound();

        ViewBag.Owners = await db.Users
   .Where(u => u.Role == UserRole.Owner)
          .OrderBy(u => u.Name)
       .ToListAsync();
        return View(property);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Property model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Owners = await db.Users
                    .Where(u => u.Role == UserRole.Owner)
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
        var property = await db.Properties.FindAsync(id);
        if (property == null) return NotFound();

        db.Remove(property);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
