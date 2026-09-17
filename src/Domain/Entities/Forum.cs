namespace Domain.Entities;

public class Forum
{
    public int Id { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public bool State { get; private set; }

    public Forum(string title, string description)
    {
        Title = title;
        Description = description;
        State = true;
    }
}