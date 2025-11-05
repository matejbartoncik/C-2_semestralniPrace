namespace semestralniPraceBartoncik.Models;

public enum OrderStatus { New, InProgress, Done }

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PropertyId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public string? AssignedTechnicianId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
    public DateTime? ScheduledFromUtc { get; set; }
    public DateTime? ScheduledToUtc { get; set; }
    
    public Property? Property { get; set; }
    public User? AssignedTechnician { get; set; }
}
