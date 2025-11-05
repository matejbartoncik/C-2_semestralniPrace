namespace semestralniPraceBartoncik.Models;

public class Property
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    
public User? Owner { get; set; }
}
