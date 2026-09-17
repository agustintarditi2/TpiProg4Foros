using MyApp.Domain.Enums;
namespace MyApp.Domain.Entities;
public class User
{
    // ID: Esto lo requiere EF, pareciera ser.
    public int Id {get; private set;}
    // El rol es un enum, está tomado del archivo Domain/Enums/UserRoles.cs.
    public UserRole Role {get; private set;}
    public string Name {get; private set;}
    
    public DateOnly DateOfBirth {get; private set;}
    public User() { Name = null!; }
    public User(string name, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Name is required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new Exception("Date of birth cannot be in the future.");
        this.Role = UserRole.USER;
        this.Name = name.Trim();
        this.DateOfBirth = dateOfBirth;
    }
}