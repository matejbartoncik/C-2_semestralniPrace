namespace semestralniPraceBartoncik.Models;

public class DashboardStats
{
    public int TotalOrders { get; set; }
    public int NewOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int TotalProperties { get; set; }
    public int TotalTechnicians { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
}
