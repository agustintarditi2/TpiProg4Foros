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
    public DateTimeOffset DateCreated {get; private set;}
    public EmailAddress Email {get; private set;}
    public PasswordHash Password {get; private set;}
    private User() { Name = null!; Email = null!; Password = null!;}
    public User(string name, DateOnly dateOfBirth, string email, string hash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        if (dateOfBirth.ToDateTime(TimeOnly.MinValue) > now)
            throw new DomainException("Date of birth cannot be in the future.");
        Role = UserRole.User;
        Name = name.Trim();
        DateOfBirth = dateOfBirth;
        Email = EmailAddress.Create(email);
        Password = PasswordHash.Create(hash);
        DateCreated = now;
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
        Email = EmailAddress.Create(newEmail);
    }
}