using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(ApplicationDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var users = new List<User>();

        users.Add(new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin Hlavní",
            Email = "admin@spolehlivyopravar.cz",
            Role = UserRole.Admin
        });

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
    }
}
