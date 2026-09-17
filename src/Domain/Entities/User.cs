using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;
namespace MyApp.Domain.Entities;
public sealed class User
{
    // El rol es un enum, está tomado del archivo Domain/Enums/UserRoles.cs.
    public UserRole Role {get; private set;}
    public string Name {get; private set;}
    
    public DateOnly DateOfBirth {get; private set;}
    public EmailAddress Email {get; private set;}
    public User() { Name = null!; }
    public User(string name, DateOnly dateOfBirth, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Name is required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new Exception("Date of birth cannot be in the future.");
        this.Role = UserRole.USER;
        this.Name = name.Trim();
        this.DateOfBirth = dateOfBirth;
        this.Email = EmailAddress.Create(email);
    }
    public void PromoteOrDemote(UserRole userRole)
    {
        this.Role = userRole;
    }
    public void Rename(string newName)
    {
        this.Name = newName.Trim();
    }
}