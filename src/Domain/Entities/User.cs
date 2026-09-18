using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;
namespace MyApp.Domain.Entities;
public sealed class User
{
    // Este ID es para Entity Framework, que lo va a hacer su PK.
    public UserId Id {get; private set;}
    // El rol es un enum, está tomado del archivo Domain/Enums/UserRoles.cs.
    public UserRole Role {get; private set;}
    public string Name {get; private set;}
    
    public DateOnly DateOfBirth {get; private set;}
    public EmailAddress Email {get; private set;} //Esto después hay que ponerle un constraint Unique en el archivo del ORM
    private User() { Name = null!; Email = null!;}
    public User(string name, DateOnly dateOfBirth, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Date of birth cannot be in the future.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-Mail address is required.");
        Role = UserRole.USER;
        Name = name.Trim();
        DateOfBirth = dateOfBirth;
        Email = EmailAddress.Create(email);
    }
    public void SetRole(UserRole userRole)
    {

        Role = userRole;
    }
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name is required.");
        Name = newName.Trim();
    }
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new Exception("E-Mail address is required.");
        Email = EmailAddress.Create(newEmail);
    }
}