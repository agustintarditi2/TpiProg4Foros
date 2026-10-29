using MyApp.Domain.ValueObjects;

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
        Title = title;
        Description = description;
        State = true;
    }

    // modifica los datos del foro
    public void Update(string title, string description)
    {
        Title = title;
        Description = description;
    }
}