using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;

namespace MyApp.Domain.Entities;

// representa un foro dentro del dominio
public sealed class Forum
{
    public ForumId Id { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public bool State { get; private set; }

    // constructor utilizado por entity framework
    private Forum()
    {
        Title = null!;
        Description = null!;
    }

    // crea un nuevo foro
    public Forum(string title, string description)
    {
        Id = ForumId.New();
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        State = true;
    }

    // modifica los datos del foro
    public void Update(string title, string description)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = ValidateDescription(description);

        Title = validatedTitle;
        Description = validatedDescription;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        title = title.Trim();

        if (title.Length > 100)
            throw new DomainException("Title cannot exceed 100 characters.");

        return title;
    }

    // valida la descripcion del foro
    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        description = description.Trim();

        if (description.Length > 500)
            throw new DomainException("Description cannot exceed 500 characters.");

        return description;
    }
    // revisar utiliad del state
    public void Deactivate()
    {
        if (!State)
            throw new DomainException("Forum is already inactive.");

        State = false;
    }

    // activa el foro
    public void Activate()
    {
        if (State)
            throw new DomainException("Forum is already active.");

        State = true;
    }
}

