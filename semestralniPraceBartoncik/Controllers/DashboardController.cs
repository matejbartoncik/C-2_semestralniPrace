using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var stats = new DashboardStats
        {
            TotalOrders = await db.Orders.CountAsync(),
            NewOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.New),
            InProgressOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.InProgress),
            CompletedOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Done),
            TotalProperties = await db.Properties.CountAsync(),
            TotalTechnicians = await db.Users.CountAsync(u => u.Role == UserRole.Technician),
            RecentOrders = await db.Orders
      .Include(o => o.Property)
          .Include(o => o.AssignedTechnician)
      .OrderByDescending(o => o.CreatedAtUtc)
            .Take(5)
        .ToListAsync()
        };

        return View(stats);
    }
}
