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
    public List<Reaction>? Reactions {get; private set;}
    public bool IsActive { get; private set; } = true;
    private User() { Name = null!; Email = null!; Password = null!;}
    // Constructor
    public User(string name, DateOnly dateOfBirth, string email, string hash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        if (dateOfBirth.ToDateTime(TimeOnly.MinValue) > now)
            throw new DomainException("Date of birth cannot be in the future.");
        DateOnly today = DateOnly.FromDateTime(now.Date);

        if (dateOfBirth.AddYears(16) > today)
            throw new DomainException(
                "You must be older than 16 years to register.");

        Role = UserRole.User;
        Name = name.Trim();
        DateOfBirth = dateOfBirth;
        Email = EmailAddress.Create(email);
        Password = PasswordHash.Create(hash);
        DateCreated = now;
        Reactions = new List<Reaction>();
        Id = UserId.New();
    }
    public void ReId(UserId newId)
    {
        Id = newId;
        return;
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
    public void ChangePassword(string newHash)
    {
        Password = PasswordHash.Create(newHash);
    }

    // registra o cambia una reaccion a una publicacion
    public void SetPostReaction(PostId postId, bool isLike)
    {
        if (postId.IsEmpty)
            throw new DomainException("Post ID is required.");

        Reactions ??= new List<Reaction>();

        var existing = Reactions.FirstOrDefault(r => r.Post == postId);

        if (existing is null)
            Reactions.Add(new Reaction(postId, Id, isLike));
        else
            existing.ChangeType(isLike);
    }

    // registra o cambia una reaccion a un comentario
    public void SetCommentReaction(CommentId commentId, bool isLike)
    {
        if (commentId.IsEmpty)
            throw new DomainException("Comment ID is required.");

        Reactions ??= new List<Reaction>();

        var existing = Reactions.FirstOrDefault(r => r.Comment == commentId);

        if (existing is null)
            Reactions.Add(new Reaction(commentId, Id, isLike));
        else
            existing.ChangeType(isLike);
    }

    public void RemovePostReaction(PostId postId)
    {
        if (postId.IsEmpty)
            throw new DomainException("Post ID is required.");

        if (Reactions is null)
            return;

        // Busca la reacción del usuario a esta publicación
        var existingReaction = Reactions.FirstOrDefault(
            r => r.Post == postId);

        // Si no existe, no hay nada que eliminar
        if (existingReaction is null)
            return;

        // Elimina la reacción
        Reactions.Remove(existingReaction);
    }
    public void RemoveCommentReaction(CommentId commentId)
{
    if (commentId.IsEmpty)
        throw new DomainException("Comment ID is required.");

    if (Reactions is null)
        return;

    // busca la reaccion del usuario a este comentario
    var existingReaction = Reactions.FirstOrDefault(
        r => r.Comment == commentId);

    // si no existe, no hay nada que eliminar
    if (existingReaction is null)
        return;

    // elimina la reaccion
    Reactions.Remove(existingReaction);
}
    public void DeactivateAccount(string disabledPasswordHash)
    {
        if (!IsActive)
            throw new DomainException("Account is already deactivated.");

        // Desactiva la cuenta.
        IsActive = false;

        // Reemplaza los datos personales.
        Name = "Usuario eliminado";
        Email = EmailAddress.Create(
            $"deleted-{Id.Value:N}@example.invalid");

        DateOfBirth = new DateOnly(1900, 1, 1);
        Password = PasswordHash.Create(disabledPasswordHash);
        Role = UserRole.User;
    }

    
}