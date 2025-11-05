using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(ApplicationDbContext db)
  {
     if (await db.Users.AnyAsync()) return;

      // Create users
        var admin = new User 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = "Admin", 
     Email = "admin@demo.cz", 
            Role = UserRole.Admin 
        };
      
        var tech1 = new User 
        { 
  Id = Guid.NewGuid().ToString(), 
       Name = "Technik Tomáš", 
            Email = "tomas@demo.cz", 
          Role = UserRole.Technician 
        };
 
        var tech2 = new User 
        { 
            Id = Guid.NewGuid().ToString(), 
          Name = "Technik Pavel", 
     Email = "pavel@demo.cz", 
     Role = UserRole.Technician 
        };
      
        var owner1 = new User 
        { 
            Id = Guid.NewGuid().ToString(), 
 Name = "Majitelka Marie", 
        Email = "marie@demo.cz", 
       Role = UserRole.Owner 
        };
      
      var owner2 = new User 
        { 
      Id = Guid.NewGuid().ToString(), 
     Name = "Majitel Jan", 
  Email = "jan@demo.cz", 
            Role = UserRole.Owner 
  };

        // Create properties
        var prop1 = new Property
        {
  Id = Guid.NewGuid().ToString(),
     Title = "Byt 2+kk Praha",
 Address = "Hlavní 123, Praha 1",
         OwnerId = owner1.Id
        };
   
        var prop2 = new Property
  {
       Id = Guid.NewGuid().ToString(),
            Title = "Rodinný dùm Brno",
            Address = "Zahradní 45, Brno",
            OwnerId = owner2.Id
        };

      var prop3 = new Property
      {
 Id = Guid.NewGuid().ToString(),
        Title = "Kanceláø Ostrava",
   Address = "Prùmyslová 78, Ostrava",
    OwnerId = owner1.Id
        };

        // Create orders
        var order1 = new Order
  {
      Id = Guid.NewGuid().ToString(),
     PropertyId = prop1.Id,
            Description = "Výmìna sifonu v kuchyni - ucpaná odpadní trubka",
            Status = OrderStatus.New,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        };
        
        var order2 = new Order
     {
            Id = Guid.NewGuid().ToString(),
   PropertyId = prop2.Id,
       Description = "Oprava protékajícího kohoutu v koupelnì",
         Status = OrderStatus.InProgress,
            AssignedTechnicianId = tech1.Id,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
            ScheduledFromUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(9),
     ScheduledToUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(11)
        };
        
      var order3 = new Order
        {
 Id = Guid.NewGuid().ToString(),
            PropertyId = prop3.Id,
       Description = "Instalace nových radiátorù - 3 kusy",
            Status = OrderStatus.New,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-3)
      };
        
        var order4 = new Order
 {
            Id = Guid.NewGuid().ToString(),
      PropertyId = prop1.Id,
     Description = "Preventivní kontrola topení pøed zimou",
      Status = OrderStatus.Done,
            AssignedTechnicianId = tech2.Id,
        CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
       ScheduledFromUtc = DateTime.UtcNow.AddDays(-8).Date.AddHours(14),
            ScheduledToUtc = DateTime.UtcNow.AddDays(-8).Date.AddHours(16)
 };

        // Add all entities
        db.AddRange(admin, tech1, tech2, owner1, owner2);
        db.AddRange(prop1, prop2, prop3);
        db.AddRange(order1, order2, order3, order4);
        
        await db.SaveChangesAsync();
    }
}
