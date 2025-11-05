using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

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
}
