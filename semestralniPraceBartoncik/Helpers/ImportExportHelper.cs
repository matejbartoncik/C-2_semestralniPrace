using System.Text;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Helpers;

public static class ImportExportHelper
{
    // ===== CSV EXPORT (s støedníkem) =====
 
  public static byte[] ExportOrdersToCsv(IEnumerable<Order> orders)
  {
  var csv = new StringBuilder();
      csv.AppendLine("ID;Created;PropertyTitle;PropertyAddress;Description;Status;TechnicianName;TechnicianEmail;ScheduledFrom;ScheduledTo");

        foreach (var order in orders)
   {
            csv.AppendLine($"\"{order.Id}\";" +
      $"\"{order.CreatedAtUtc:yyyy-MM-dd HH:mm}\";" +
   $"\"{order.Property?.Title ?? ""}\";" +
     $"\"{order.Property?.Address ?? ""}\";" +
    $"\"{order.Description}\";" +
         $"\"{order.Status}\";" +
     $"\"{order.AssignedTechnician?.Name ?? ""}\";" +
 $"\"{order.AssignedTechnician?.Email ?? ""}\";" +
$"\"{order.ScheduledFromUtc?.ToString("yyyy-MM-dd HH:mm") ?? ""}\";" +
         $"\"{order.ScheduledToUtc?.ToString("yyyy-MM-dd HH:mm") ?? ""}\"");
        }

   return Encoding.UTF8.GetBytes(csv.ToString());
}

    public static byte[] ExportPropertiesToCsv(IEnumerable<Property> properties)
  {
        var csv = new StringBuilder();
        csv.AppendLine("ID;Title;Address;OwnerName;OwnerEmail");

 foreach (var prop in properties)
     {
            csv.AppendLine($"\"{prop.Id}\";" +
     $"\"{prop.Title}\";" +
  $"\"{prop.Address}\";" +
    $"\"{prop.Owner?.Name ?? ""}\";" +
    $"\"{prop.Owner?.Email ?? ""}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public static byte[] ExportUsersToCsv(IEnumerable<User> users)
    {
        var csv = new StringBuilder();
     csv.AppendLine("ID;Name;Email;Role");

  foreach (var user in users)
   {
    csv.AppendLine($"\"{user.Id}\";" +
       $"\"{user.Name}\";" +
    $"\"{user.Email}\";" +
        $"\"{user.Role}\"");
    }

        return Encoding.UTF8.GetBytes(csv.ToString());
 }

    // ===== CSV IMPORT (s støedníkem) =====
    
    public static List<Order> ImportOrdersFromCsv(string csvContent, Dictionary<string, string> propertyLookup, Dictionary<string, string> technicianLookup)
    {
  var orders = new List<Order>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // Skip header
  for (int i = 1; i < lines.Length; i++)
     {
      var parts = ParseCsvLine(lines[i], ';');
   if (parts.Length < 6) continue;

    var order = new Order
     {
      Id = Guid.NewGuid().ToString(),
          PropertyId = propertyLookup.ContainsKey(parts[2]) ? propertyLookup[parts[2]] : "",
        Description = parts[4],
 Status = Enum.TryParse<OrderStatus>(parts[5], out var status) ? status : OrderStatus.New,
   CreatedAtUtc = DateTime.TryParse(parts[1], out var created) ? created : DateTime.UtcNow
  };

     if (parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]) && technicianLookup.ContainsKey(parts[6]))
   {
order.AssignedTechnicianId = technicianLookup[parts[6]];
 }

            if (parts.Length > 8 && DateTime.TryParse(parts[8], out var schedFrom))
{
   order.ScheduledFromUtc = schedFrom;
  }

if (parts.Length > 9 && DateTime.TryParse(parts[9], out var schedTo))
  {
      order.ScheduledToUtc = schedTo;
    }

     orders.Add(order);
        }

        return orders;
    }

    public static List<Property> ImportPropertiesFromCsv(string csvContent, Dictionary<string, string> ownerLookup)
    {
  var properties = new List<Property>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
  // Skip header
     for (int i = 1; i < lines.Length; i++)
   {
     var parts = ParseCsvLine(lines[i], ';');
  if (parts.Length < 3) continue;

            var property = new Property
{
   Id = Guid.NewGuid().ToString(),
Title = parts[1],
   Address = parts[2],
         OwnerId = parts.Length > 3 && ownerLookup.ContainsKey(parts[3]) ? ownerLookup[parts[3]] : ""
            };

         if (string.IsNullOrWhiteSpace(property.OwnerId) && ownerLookup.Any())
   {
  property.OwnerId = ownerLookup.First().Value;
            }

         properties.Add(property);
    }

        return properties;
    }

    public static List<User> ImportUsersFromCsv(string csvContent)
    {
var users = new List<User>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
    // Skip header
        for (int i = 1; i < lines.Length; i++)
 {
     var parts = ParseCsvLine(lines[i], ';');
            if (parts.Length < 4) continue;

    var user = new User
        {
    Id = Guid.NewGuid().ToString(),
 Name = parts[1],
   Email = parts[2],
   Role = Enum.TryParse<UserRole>(parts[3], out var role) ? role : UserRole.Owner
  };

users.Add(user);
        }

        return users;
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
var current = new StringBuilder();
    bool inQuotes = false;

    foreach (char c in line)
      {
if (c == '"')
            {
    inQuotes = !inQuotes;
         }
      else if (c == delimiter && !inQuotes)
            {
            result.Add(current.ToString());
       current.Clear();
          }
         else
       {
         current.Append(c);
        }
    }

        result.Add(current.ToString());
        return result.ToArray();
    }
}
