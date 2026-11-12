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
    public CommentServices(ICommentRepository commentRepository, IPostRepository postRepository,IUserRepository userRepository, IClock clock, ISanitizer sanitizer){
        _sanitizer = sanitizer;
        _repository = commentRepository;
        _postRepository = postRepository;
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
            throw new NotFoundException("Parent comment not found.");
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
        // verifica que el usuario sea el autor
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only edit your own comments.");
        body = _sanitizer.Sanitize(body);
        entity.EditBody(body, _clock.UtcNow);
        await _repository.Update(entity, ct);
    }

    public async Task HideOrUnhide(Guid id, bool value, CancellationToken ct)
    {
        var entity = await _repository.GetById((CommentId)id, ct);
        if (entity is null) throw new NotFoundException("Requested comment not found");
        entity.ToggleHidden(value);
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
        entity.Delete();
        await _repository.Update(entity, ct);
    }

}