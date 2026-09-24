namespace MyApp.Domain.ValueObjects;

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid value) => new(value);
    public bool IsEmpty => Value == Guid.Empty;    
}

public readonly record struct PostId(Guid Value)
{
    public static PostId New() => new(Guid.NewGuid());
    public static implicit operator Guid(PostId id) => id.Value;
    public static explicit operator PostId(Guid value) => new(value);
    public bool IsEmpty => Value == Guid.Empty;
}
public readonly record struct CommentId(Guid Value)
{
    public static CommentId New() => new(Guid.NewGuid());
    public static implicit operator Guid(CommentId id) => id.Value;
    public static explicit operator CommentId(Guid value) => new(value);
    public bool IsEmpty => Value == Guid.Empty;    
}