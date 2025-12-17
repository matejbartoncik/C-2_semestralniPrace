using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using semestralniPraceBartoncik.Data;
using semestralniPraceBartoncik.Models;

namespace semestralniPraceBartoncik.Services;

public class GoogleCalendarService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly string? _credentialsPath;

    public GoogleCalendarService(
        IConfiguration configuration,
ApplicationDbContext db,
        ILogger<GoogleCalendarService> logger,
        IWebHostEnvironment env)
    {
        _configuration = configuration;
        _db = db;
        _logger = logger;
        _credentialsPath = Path.Combine(env.ContentRootPath, "Credentials/secret-key.json");
    }

    public async Task<bool> SyncOrderToCalendarAsync(Order order)
    {
        try
        {
            if (!order.ScheduledFromUtc.HasValue || !order.ScheduledToUtc.HasValue)
            {
                return false;
            }

            if (order.Property == null)
            {
                order = await _db.Orders
           .Include(o => o.Property)
          .ThenInclude(p => p.Owner)
     .Include(o => o.AssignedTechnician)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
            }

            if (order.Property == null)
            {
                return false;
            }

            var calendarEvent = new Event
            {
                Summary = $"Oprava: {order.Property.Title}",
                Description = $"Popis: {order.Description}\n\n" +
                   $"Vlastnik: {order.Property.Owner?.Name ?? "N/A"}\n" +
                   $"Email: {order.Property.Owner?.Email ?? "N/A"}\n" +
                $"Technik: {order.AssignedTechnician?.Name ?? "Neprirazen"}\n" +
               $"Status: {GetStatusCzech(order.Status)}",
                Location = order.Property.Address ?? "",
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = order.ScheduledFromUtc.Value,
                    TimeZone = "Europe/Prague"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = order.ScheduledToUtc.Value,
                    TimeZone = "Europe/Prague"
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
      {
    new EventReminder { Method = "popup", Minutes = 30 }
    }
                },
                ExtendedProperties = new Event.ExtendedPropertiesData
                {
                    Private__ = new Dictionary<string, string>
    {
     { "orderId", order.Id }
                }
                }
            };

            var service = await GetCalendarServiceAsync();
            if (service == null)
            {
                return false;
            }

            var calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
            var existingEvent = await FindExistingEventAsync(service, order.Id, calendarId);

            if (existingEvent != null)
            {
                var updateRequest = service.Events.Update(calendarEvent, calendarId, existingEvent.Id);
                await updateRequest.ExecuteAsync();
            }
            else
            {
                var createRequest = service.Events.Insert(calendarEvent, calendarId);
                await createRequest.ExecuteAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Chyba při synchronizaci s Google Calendar: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveOrderFromCalendarAsync(Order order)
    {
        try
        {
            var service = await GetCalendarServiceAsync();
            if (service == null)
                return false;

            var calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
            var existingEvent = await FindExistingEventAsync(service, order.Id, calendarId);

            if (existingEvent != null)
            {
                var deleteRequest = service.Events.Delete(calendarId, existingEvent.Id);
                await deleteRequest.ExecuteAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Chyba při mazání z Google Calendar: {ex.Message}");
            return false;
        }
    }

    private async Task<Event> FindExistingEventAsync(CalendarService service, string orderId, string calendarId = "primary")
    {
        try
        {
            var listRequest = service.Events.List(calendarId);
            listRequest.PrivateExtendedProperty = new List<string> { $"orderId={orderId}" };
            listRequest.ShowDeleted = false;

            var events = await listRequest.ExecuteAsync();
            return events.Items?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task<CalendarService> GetCalendarServiceAsync()
    {
        try
        {
            if (!File.Exists(_credentialsPath))
            {
                return null;
            }

            var credential = await GoogleCredential.FromFileAsync(_credentialsPath, CancellationToken.None)
                   .ConfigureAwait(false);

            var scopedCredential = credential.CreateScoped(CalendarService.Scope.Calendar);

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = scopedCredential,
                ApplicationName = "spolehlivy-opravar"
            });

            return service;
        }
        catch
        {
            return null;
        }
    }

    private string GetStatusCzech(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.New => "Nova",
            OrderStatus.InProgress => "Probiha",
            OrderStatus.Done => "Dokoncena",
            _ => status.ToString()
        };
    }
}
