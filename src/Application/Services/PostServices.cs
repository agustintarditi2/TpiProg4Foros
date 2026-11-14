using MyApp.Domain.ValueObjects;
using MyApp.Domain.Entities;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Application.Interfaces;
using MyApp.Domain.Exceptions;

namespace MyApp.Application.Services;

public sealed class PostServices{
    private readonly IPostRepository _repository;
    private readonly IClock _clock;
    private readonly ISanitizer _sanitizer;
    private readonly IUserRepository _userRepository;
    private readonly IForumRepository _forumRepository;
    private readonly BanServices _bans;
    public PostServices(IPostRepository postRepository, IForumRepository forumRepository,IUserRepository userRepository, IClock clock, ISanitizer sanitizer, BanServices bans){
        _sanitizer = sanitizer;
        _repository = postRepository;
        _forumRepository = forumRepository;
        _userRepository = userRepository;
        _clock = clock;
        _bans = bans;
    }

    public async Task<PostDTO> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null || entity.Invisible)
            throw new NotFoundException("Requested post not found");
        return PostDTO.Create(entity);
    }

    public async Task<List<PostDTO>> GetByForum(Guid id, CancellationToken ct)
    {
        var entities = await _repository.GetByForum((ForumId)id, ct);
        return entities
            // incluye los posts cuyo hidden sea false
            .Where(p => !p.Invisible)
            .Select(PostDTO.Create)
            .ToList();   
    }   

    public async Task<List<PostDTO>> GetByPoster(Guid id, CancellationToken ct)
    {
        var entities = await _repository.GetByPoster((UserId)id, ct);
        return entities
        .Where(p => !p.Invisible)
        .Select(PostDTO.Create)
        .ToList();
    }
    
    public async Task<List<PostDTO>> GetMyPosts(Guid userId, CancellationToken ct)
    {
        var entities = await _repository.GetByPoster((UserId)userId, ct);

        return entities
            .Select(PostDTO.Create)
            .ToList();
    }

    public async Task<PostDTO> Create(CreatePostDTO dto, Guid userId, CancellationToken ct)
    {
        await _bans.EnsureNotBanned(userId, ct);
        if (userId == Guid.Empty)
        throw new DomainException("A post must have an author.");

        var entity = new Post(
            _sanitizer.Sanitize(dto.Body),
            (UserId)userId,
            (ForumId)dto.Forum,
            _clock.UtcNow
        );
         // verifica que el foro exista
        var forum = await _forumRepository.GetById(entity.ForumId, ct);
        if (forum is null)
            throw new NotFoundException("Requested forum not found");

        if (!forum.State)
            throw new DomainException(
            "Cannot create posts in an inactive forum.");

          // verifica que el autor exista
        var user = await _userRepository.GetById(entity.Poster, ct);
        if (user is null)
            throw new NotFoundException("Requested user not found");

        var addedEntity = await _repository.Add(entity, ct);

        return PostDTO.Create(addedEntity);
    }

    public async Task Update(Guid id, string body, Guid userId, CancellationToken ct)
    {
        await _bans.EnsureNotBanned(userId, ct);
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null)
            throw new NotFoundException("Requested post not found");
        // verifica que el usuario sea el autor
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only edit your own posts.");

        var forum = await _forumRepository.GetById(entity.ForumId, ct);
        
        if (forum is null)
            throw new NotFoundException("Requested forum not found");
        if (!forum.State)
            throw new DomainException(
            "Cannot edit posts in an inactive forum.");
        body = _sanitizer.Sanitize(body);
        entity.EditBody(body, _clock.UtcNow);
        await _repository.Update(entity, ct);
    }

    public async Task Suppress(Guid id, bool value, CancellationToken ct)
    {
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null) throw new NotFoundException("Requested post not found");
        entity.ToggleSuppressed(value);
        await _repository.Update(entity, ct);
    }

    public async Task HideOrUnhideSelf(Guid id, Guid userId, bool value, CancellationToken ct)
    {
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null) throw new NotFoundException("Requested post not found");
        // verifica que el usuario sea el autor
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can change the visibility of your own posts.");
        entity.ToggleHidden(value);
        await _repository.Update(entity, ct);
    }

    public async Task Delete(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null) throw new NotFoundException("Requested post not found");
        if (entity.Poster != (UserId)userId)
            throw new UnauthorizedAccessException(
            "You can only delete your own posts.");
        await _repository.Delete(entity, ct);
    }

}