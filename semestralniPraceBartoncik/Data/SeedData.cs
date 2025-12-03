using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Data;

public static class SeedData
{
    private static readonly Random _random = new();
    
    public static async Task EnsureSeedAsync(ApplicationDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        Console.WriteLine("🌱 Seeding database with realistic data...");

        // ===== USERS =====
        var users = new List<User>();
        
  // Admin
      users.Add(new User 
  { 
   Id = Guid.NewGuid().ToString(), 
     Name = "Admin Hlavní", 
    Email = "admin@spolehlivyopravar.cz", 
            Role = UserRole.Admin 
        });

        // Technicians (10)
 var technicianNames = new[] 
   {
  "Tomáš Novák", "Pavel Svoboda", "Jan Dvořák", "Petr Černý", "Martin Procházka",
  "Jakub Kučera", "Lukáš Veselý", "Michal Horák", "David Němec", "Ondřej Marek"
        };
        
foreach (var name in technicianNames)
     {
         users.Add(new User
 {
                Id = Guid.NewGuid().ToString(),
     Name = name,
      Email = $"{name.ToLower().Replace(" ", ".")}@spolehlivyopravar.cz",
    Role = UserRole.Technician
         });
        }

    var ownerNames = new[]
        {
            "Marie Nováková", "Jana Svobodová", "Eva Dvořáková", "Petra Černá", "Lucie Procházková",
      "Kateřina Kučerová", "Lenka Veselá", "Hana Horáková", "Věra Němcová", "Zuzana Marková",
            "Josef Malý", "František Pospíšil", "Miroslav Hájek", "Jaroslav Král", "Vladimír Beneš",
            "Stanislav Růžička", "Milan Fiala", "Robert Sedláček", "Radek Doležal", "Jiří Nguyen",
         "Alena Kolářová", "Barbora Čermáková", "Dana Urbanová", "Ivana Vaňková", "Jitka Kratochvílová",
        "Markéta Jelínková", "Monika Bartošová", "Pavla Růžičková", "Simona Kovářová", "Tereza Pokorná"
      };
    
     foreach (var name in ownerNames)
        {
            users.Add(new User
 {
         Id = Guid.NewGuid().ToString(),
      Name = name,
       Email = $"{name.ToLower().Replace(" ", ".")}@email.cz",
                Role = UserRole.Owner
  });
      }

        db.AddRange(users);
    await db.SaveChangesAsync();
 Console.WriteLine($"✅ Created {users.Count} users");

        var owners = users.Where(u => u.Role == UserRole.Owner).ToList();
     var properties = new List<Property>();
        
        var propertyTypes = new[] { "Byt 1+kk", "Byt 2+kk", "Byt 3+1", "Byt 4+1", "Rodinný dům", "Kancelář", "Obchod", "Sklad" };
        var cities = new[] 
        { 
 ("Praha", new[] { "Karlín", "Vinohrady", "Žižkov", "Dejvice", "Smíchov", "Holešovice", "Nusle", "Vršovice" }),
            ("Brno", new[] { "Centrum", "Černá Pole", "Královo Pole", "Líšeň", "Bohunice", "Žabovřesky" }),
       ("Ostrava", new[] { "Centrum", "Poruba", "Moravská Ostrava", "Vítkovice", "Zábřeh" }),
          ("Plzeň", new[] { "Centrum", "Severní Předměstí", "Doubravka", "Lobzy", "Slovany" }),
       ("Liberec", new[] { "Centrum", "Rochlice", "Vratislavice", "Ruprechtice" }),
            ("Olomouc", new[] { "Centrum", "Hodolany", "Nová Ulice", "Bělidla" }),
            ("České Budějovice", new[] { "Centrum", "České Vrbné", "Suché Vrbné" }),
   ("Hradec Králové", new[] { "Centrum", "Nový Hradec", "Slezské Předměstí" })
        };

        int propertyCount = 0;
        foreach (var owner in owners)
      {
            int propsPerOwner = _random.Next(1, 4);
      for (int i = 0; i < propsPerOwner; i++)
       {
           var city = cities[_random.Next(cities.Length)];
  var district = city.Item2[_random.Next(city.Item2.Length)];
                var street = $"{new[] { "Hlavní", "Nová", "Zahradní", "Školní", "Lesní", "Krátká", "Dlouhá", "Průmyslová", "Sokolovská", "Vídeňská" }[_random.Next(10)]} {_random.Next(1, 200)}";
        
    properties.Add(new Property
   {
                 Id = Guid.NewGuid().ToString(),
     Title = $"{propertyTypes[_random.Next(propertyTypes.Length)]} {district}",
                    Address = $"{street}, {city.Item1} - {district}",
OwnerId = owner.Id
    });
                propertyCount++;
      }
        }

        db.AddRange(properties);
   await db.SaveChangesAsync();
   Console.WriteLine($"✅ Created {propertyCount} properties");

  var technicians = users.Where(u => u.Role == UserRole.Technician).ToList();
        var orders = new List<Order>();
        
        var orderDescriptions = new[]
        {
         "Výměna sifonu v kuchyni - ucpaná odpadní trubka",
        "Oprava protékajícího kohoutu v koupelně",
          "Instalace nových radiátorů",
            "Preventivní kontrola topení",
            "Oprava netěsnící pračky - výměna přívodní hadice",
       "Instalace nového WC s bidetovou sprškou",
            "Výměna vodovodního baterie v umyvadle",
       "Oprava protékajícího záchodu - výměna nádržky",
  "Instalace průtokového ohřívače",
 "Výměna vadného ventilu na radiátoru",
    "Čištění kanalizace - ucpání odpadu",
         "Instalace nové kuchyňské linky s připojením vody",
            "Oprava protékající střechy - zatékání do bytu",
   "Výměna poškozených odpadních trubek",
          "Instalace sprchového koutu s odtokem",
            "Oprava prasklé vodovodní trubky v bytě",
            "Výměna starých radiátorů za nové",
            "Instalace termostatických hlavic na radiátory",
  "Oprava kondenzačního kotle - servis",
     "Výměna filtru ve vodním rozvodu",
"Instalace nového bojleru 100L",
       "Oprava netěsnící vaničky sprchového koutu",
      "Výměna poškozených kolen na odpadech",
       "Instalace zpětných klapek proti zápachům",
     "Oprava nefunkční myčky - připojení k vodě",
            "Výměna vadného čerpadla topení",
            "Instalace tlakové stanice v domě",
            "Oprava prasklého odpadu pod umyvadlem",
            "Výměna celého WC kompletu",
            "Instalace nového zásobníku na teplou vodu"
        };

        var startDate = DateTime.UtcNow.AddMonths(-6);
        int orderCount = 0;
        
        foreach (var property in properties)
        {
            int ordersPerProperty = _random.Next(3, 9);
            
   for (int i = 0; i < ordersPerProperty; i++)
            {
            var createdDate = startDate.AddDays(_random.Next(0, 180)).AddHours(_random.Next(7, 19));
                var status = GetRandomStatus(createdDate);
         var technician = technicians[_random.Next(technicians.Count)];
        
       var order = new Order
    {
       Id = Guid.NewGuid().ToString(),
       PropertyId = property.Id,
  Description = orderDescriptions[_random.Next(orderDescriptions.Length)],
             Status = status,
    CreatedAtUtc = createdDate
     };

if (status == OrderStatus.InProgress || status == OrderStatus.Done)
     {
       order.AssignedTechnicianId = technician.Id;
  
          var scheduledDate = createdDate.AddDays(_random.Next(1, 6)).Date.AddHours(_random.Next(8, 16));
        order.ScheduledFromUtc = scheduledDate;
        order.ScheduledToUtc = scheduledDate.AddHours(_random.Next(1, 4));
                }
      else if (status == OrderStatus.New && _random.Next(2) == 0)
                {
        order.AssignedTechnicianId = technician.Id;
    
        var futureDate = DateTime.UtcNow.AddDays(_random.Next(1, 15)).Date.AddHours(_random.Next(8, 16));
              order.ScheduledFromUtc = futureDate;
           order.ScheduledToUtc = futureDate.AddHours(_random.Next(1, 4));
     }

   orders.Add(order);
     orderCount++;
            }
        }

  db.AddRange(orders);
        await db.SaveChangesAsync();
    Console.WriteLine($"✅ Created {orderCount} orders");
        Console.WriteLine($"   - New: {orders.Count(o => o.Status == OrderStatus.New)}");
    Console.WriteLine($"   - InProgress: {orders.Count(o => o.Status == OrderStatus.InProgress)}");
      Console.WriteLine($"   - Done: {orders.Count(o => o.Status == OrderStatus.Done)}");
      Console.WriteLine("🎉 Database seeding completed!");
 }

  private static OrderStatus GetRandomStatus(DateTime createdDate)
    {
        var daysSinceCreation = (DateTime.UtcNow - createdDate).TotalDays;
        
        if (daysSinceCreation < 7)
        {
       var rand = _random.Next(100);
            if (rand < 60) return OrderStatus.New;
    if (rand < 85) return OrderStatus.InProgress;
       return OrderStatus.Done;
        }
        else if (daysSinceCreation < 30)
        {
   var rand = _random.Next(100);
            if (rand < 20) return OrderStatus.New;
  if (rand < 50) return OrderStatus.InProgress;
            return OrderStatus.Done;
   }
      else
      {
var rand = _random.Next(100);
            if (rand < 5) return OrderStatus.New;
    if (rand < 20) return OrderStatus.InProgress;
            return OrderStatus.Done;
        }
    }
}
