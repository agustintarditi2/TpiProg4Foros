using MyApp.Domain.Enums;
namespace Domain.Entities;
public class User
{
    // ID: Esto lo requiere EF, pareciera ser.
    public int Id {get; private set;}
    // El rol es un enum, está tomado del archivo Domain/Enums/UserRoles.cs.
    public UserRole Role {get; private set;}
    public string Name {get; private set;}
    
    public long dob {get; private set;}
    public User() { null! }
    public User(string userName, long dob)
    {
        this.role = UserRole.USER;
        this.name = userName;
        this.dob = dob;
    }

}