using MyApp.Domain.ValueObjects; 

namespace MyApp.Domain.Entities;

public abstract class TextEntity
{
    public BodyText Body {get; protected set;}
    public DateTimeOffset DateCreated {get; protected set;}
    public DateTimeOffset? DateEdited {get; protected set;}
    public bool Edited {get; protected set;}
    public bool Hidden {get; protected set;}

    protected TextEntity()
    {
        Body = null!;
    }

    public void EditBody(string newBody)
    {
        Body = BodyText.Create(newBody);
    }
}