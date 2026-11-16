using MyApp.Domain.ValueObjects;
using MyApp.Domain.Entities;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Application.Interfaces;
using MyApp.Domain.Exceptions;

namespace MyApp.Application.Services;

public sealed class CommentServices{
    private readonly ICommentRepository _repository;
    private readonly IClock _clock;
    private readonly ISanitizer _sanitizer;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IForumRepository _forumRepository;
    public CommentServices(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository, IForumRepository forumRepository, IClock clock, ISanitizer sanitizer){
        _sanitizer = sanitizer;
        _repository = commentRepository;
        _postRepository = postRepository;
        _forumRepository = forumRepository;
        _userRepository = userRepository;
        _clock = clock;
    }

    public async Task<CommentDTO> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null)
            throw new NotFoundException("Requested comment not found");
        return CommentDTO.Create(entity);
    }

    public async Task<List<CommentDTO>> GetByPost(Guid id, CancellationToken ct)
    {
        var entities = await _repository.GetByPost((PostId)id, ct);
        return entities.Select(CommentDTO.Create).ToList();
    }

    public async Task<List<CommentDTO>> GetByThread(Guid id, CancellationToken ct)
    {
        var entities = await _repository.GetByThread((CommentId)id, ct);
        return entities.Select(CommentDTO.Create).ToList();
    }

    public async Task<List<CommentDTO>> GetByPoster(Guid id, CancellationToken ct)
    {
        var entities = await _repository.GetByPoster((UserId)id, ct);
        return entities.Select(CommentDTO.Create).ToList();
    }

    public async Task<CommentDTO> Create(CreateCommentDTO dto, Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        throw new DomainException("A comment must have an author.");
        // Rescatamos el post padre
        Post? parent = await _postRepository.GetById((PostId)dto.ParentPost, ct);
        if (parent is null)
            throw new NotFoundException("Parent post not found.");
        if (parent.Invisible)
            throw new DomainException("Cannot comment on a hidden or suppressed post.");
        var forum = await _forumRepository.GetById(parent.ForumId, ct);
        if (forum is null)
            throw new NotFoundException("Requested forum not found.");
        if (!forum.State)
            throw new DomainException("Cannot comment in an inactive forum.");
        // Rescatamos el usuario autor
        var user = await _userRepository.GetById((UserId)userId, ct);
        if (user is null)
            throw new NotFoundException("Requested user not found");
        // Rescatamos el comentario al que se contesta, de haberlo
        Comment? repliedComment = null;
        if (dto.RepliedComment is not null)
        {
            repliedComment = await _repository.GetById((CommentId)dto.RepliedComment, ct);
            if (repliedComment is null)
                throw new NotFoundException("Requested comment not found");
            if (repliedComment.Invisible)
                throw new DomainException("Cannot reply to a hidden or suppressed comment.");
        }
        
        var entity = new Comment(
            _sanitizer.Sanitize(dto.Body),
            parent,
            user,
            _clock.UtcNow,
            repliedComment
        );
        var addedEntity = await _repository.Add(entity, ct);

        return CommentDTO.Create(addedEntity);
    }

    public async Task Update(Guid id, string body, Guid userId, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null)
            throw new NotFoundException("Requested comment not found");
        // verifica que el comentario no este eliminado
        if (entity.IsDeleted)
            throw new DomainException("Cannot edit a deleted comment.");
        // verifica que el usuario sea el autor
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only edit your own comments.");
        if (entity.SuppressedByAdmin)
            throw new DomainException("Cannot edit a suppressed comment.");
        var post = await _postRepository.GetById(entity.ParentPost, ct);
        if (post is null)
            throw new NotFoundException("Parent post not found.");
        if (post.Invisible)
            throw new DomainException("Cannot edit comments on a hidden post.");
        var forum = await _forumRepository.GetById(post.ForumId, ct);
        if (forum is null)
            throw new NotFoundException("Requested forum not found.");
        if (!forum.State)
            throw new DomainException("Cannot edit comments in an inactive forum.");
        body = _sanitizer.Sanitize(body);
        entity.EditBody(body, _clock.UtcNow);
        await _repository.Update(entity, ct);
    }

    public async Task HideOrUnhide(Guid id, bool value, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null) throw new NotFoundException("Requested comment not found");
        entity.ToggleSuppressed(value);
        await _repository.Update(entity, ct);
    }

    public async Task HideOrUnhideSelf(Guid id, Guid userId, bool value, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null) throw new NotFoundException("Requested comment not found");
        // verifica que el usuario sea el autor
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only hide your own comments.");
        entity.ToggleHidden(value);
        await _repository.Update(entity, ct);
    }

    public async Task Delete(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null) throw new NotFoundException("Requested comment not found");
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only delete your own comments.");
        entity.MarkAsDeleted();
        await _repository.Update(entity, ct);
    }

}