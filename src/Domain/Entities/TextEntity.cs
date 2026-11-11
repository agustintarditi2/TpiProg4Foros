using MyApp.Domain.ValueObjects; 

namespace MyApp.Domain.Entities;

public abstract class TextEntity
{
    public BodyText Body {get; protected set;}
    public DateTimeOffset DateCreated {get; protected set;}
    public DateTimeOffset? DateEdited {get; protected set;}
    public bool Edited {get; protected set;}
    public bool Hidden {get; protected set;}
    public UserId Poster {get; protected set;}

    protected TextEntity()
    {
        Body = null!;
    }

    public void EditBody(string newBody, DateTimeOffset dateEdited)
    {
        Body = BodyText.Create(newBody);
        DateEdited = dateEdited;
        Edited = true;
    }

    public void HideOrUnhide(bool hidden)
    {
        Hidden = hidden;
    }
    public void Redact()
    {
        Body = BodyText.Create("*Este comentario está oculto.*");
    }

    public void Delete()
    {
        Body = BodyText.Deleted();
        //Poster = User.NoUser()
        // Dejo esto comentado por si eventualmente implementamos una cuenta placeholder para usuarios eliminados y ausentes.
    }
}