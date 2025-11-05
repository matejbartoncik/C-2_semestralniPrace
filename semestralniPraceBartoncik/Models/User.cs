namespace semestralniPraceBartoncik.Models;

public enum UserRole { Admin, Technician, Owner }

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Owner;
}
