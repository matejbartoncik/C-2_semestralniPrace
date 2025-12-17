using System.Text;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Helpers;

public static class ImportExportHelper
{
    public static byte[] ExportOrdersToCsv(IEnumerable<Order> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Created;PropertyTitle;PropertyAddress;Description;Status;TechnicianName;TechnicianEmail;ScheduledFrom;ScheduledTo");

        foreach (var o in orders)
        {
            sb.AppendLine(BuildCsvLine(
    o.Id,
       o.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm"),
                o.Property?.Title,
           o.Property?.Address,
         o.Description,
           o.Status.ToString(),
                o.AssignedTechnician?.Name,
       o.AssignedTechnician?.Email,
        o.ScheduledFromUtc?.ToString("yyyy-MM-dd HH:mm"),
o.ScheduledToUtc?.ToString("yyyy-MM-dd HH:mm")
            ));
        }

        return GetBytes(sb.ToString());
    }

    public static byte[] ExportPropertiesToCsv(IEnumerable<Property> properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Title;Address;OwnerName;OwnerEmail");

        foreach (var p in properties)
        {
            sb.AppendLine(BuildCsvLine(p.Id, p.Title, p.Address, p.Owner?.Name, p.Owner?.Email));
        }

        return GetBytes(sb.ToString());
    }

    public static byte[] ExportUsersToCsv(IEnumerable<User> users)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Name;Email;Role");

        foreach (var u in users)
        {
            sb.AppendLine(BuildCsvLine(u.Id, u.Name, u.Email, u.Role.ToString()));
        }

        return GetBytes(sb.ToString());
    }

    public static List<Order> ImportOrdersFromCsv(string content, Dictionary<string, string> propertyLookup, Dictionary<string, string> technicianLookup)
    {
        var result = new List<Order>();
        var lines = ReadCsvLines(content);

        foreach (var cols in lines)
        {
            if (cols.Length < 6) continue;

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                PropertyId = propertyLookup.GetValueOrDefault(cols[2], ""),
                Description = cols[4],
                Status = Enum.TryParse<OrderStatus>(cols[5], out var s) ? s : OrderStatus.New,
                CreatedAtUtc = TryParseDate(cols[1]) ?? DateTime.UtcNow
            };

            if (cols.Length > 6 && technicianLookup.TryGetValue(cols[6], out var techId))
                order.AssignedTechnicianId = techId;

            if (cols.Length > 8)
                order.ScheduledFromUtc = TryParseDate(cols[8]);

            if (cols.Length > 9)
                order.ScheduledToUtc = TryParseDate(cols[9]);

            result.Add(order);
        }

        return result;
    }

    public static List<Property> ImportPropertiesFromCsv(string content, Dictionary<string, string> ownerLookup)
    {
        var result = new List<Property>();
        var lines = ReadCsvLines(content);

        foreach (var cols in lines)
        {
            if (cols.Length < 3) continue;

            var ownerId = cols.Length > 3 ? ownerLookup.GetValueOrDefault(cols[3], "") : "";
            if (string.IsNullOrWhiteSpace(ownerId) && ownerLookup.Any())
                ownerId = ownerLookup.First().Value;

            result.Add(new Property
            {
                Id = Guid.NewGuid().ToString(),
                Title = cols[1],
                Address = cols[2],
                OwnerId = ownerId
            });
        }

        return result;
    }

    public static List<User> ImportUsersFromCsv(string content)
    {
        var result = new List<User>();
        var lines = ReadCsvLines(content);

        foreach (var cols in lines)
        {
            if (cols.Length < 4) continue;

            result.Add(new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = cols[1],
                Email = cols[2],
                Role = Enum.TryParse<UserRole>(cols[3], out var r) ? r : UserRole.Owner
            });
        }

        return result;
    }

    private static string BuildCsvLine(params string?[] values)
    {
        return string.Join(';', values.Select(v => $"\"{v ?? ""}\""));
    }

    private static byte[] GetBytes(string text)
    {
        return new UTF8Encoding(true).GetBytes(text);
    }

    private static IEnumerable<string[]> ReadCsvLines(string content)
    {
        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
            .Select(line => SplitCsvLine(line));
    }

    private static DateTime? TryParseDate(string value)
    {
        return DateTime.TryParse(value, out var d) ? d : null;
    }

    private static string[] SplitCsvLine(string line)
    {
        var items = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char ch in line)
        {
            if (ch == '"')
                inQuotes = !inQuotes;
            else if (ch == ';' && !inQuotes)
            {
                items.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(ch);
        }

        items.Add(current.ToString());
        return items.ToArray();
    }
}
