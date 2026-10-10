using MyApp.Domain.ValueObjects;
using MyApp.Domain.Entities;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Application.Interfaces;

namespace MyApp.Application.Services;

public sealed class PostServices{
    private readonly IPostRepository _repository;
    private readonly IClock _clock;
    public PostServices(IPostRepository postRepository, IClock clock){
        _repository = postRepository;
        _clock = clock;
    }

    public async Task<PostDTO> GetById(PostId id, CancellationToken ct) 
    {
        var entity = await _repository.GetById(id, ct);
        if (entity is null) throw new Exception("Requested post not found");
        return PostDTO.Create(entity); 
    }

    public async Task<List<PostDTO>> GetByForum(ForumId id, CancellationToken ct) 
    {
        var entities = await _repository.GetByForum(id, ct);
        return entities.Select(PostDTO.Create).ToList();
    }
    
    public async Task<List<PostDTO>> GetByPoster(UserId id, CancellationToken ct) 
    {
        var entities = await _repository.GetByPoster(id, ct);
        return entities.Select(PostDTO.Create).ToList();
    }

    public async Task<PostDTO> Create(PostDTO dto, CancellationToken ct) 
    {
        var entity = new Post(
            dto.Body,
            (UserId)dto.Poster,
            (ForumId)dto.Forum,
            _clock.UtcNow            
        );
        var addedEntity = await _repository.Add(entity, ct);
        return PostDTO.Create(addedEntity);
    }

    public async Task Update(Guid id, string body, CancellationToken ct) 
    {
        var entity = await _repository.GetById((PostId)id, ct);
        if (entity is null) throw new Exception("Requested post not found");
        entity.EditBody(body, _clock.UtcNow);
        await _repository.Update(entity, ct);
    }

    public async Task HideOrUnhide(PostId id, bool value, CancellationToken ct) 
    {
        var entity = await _repository.GetById(id, ct);
        if (entity is null) throw new Exception("Requested post not found");
        entity.HideOrUnhide(value);
        await _repository.Update(entity, ct);
    }

    public async Task Delete(PostId id, CancellationToken ct) 
    {
        var entity = await _repository.GetById(id, ct);
        if (entity is null) throw new Exception("Requested post not found");
        await _repository.Delete(entity, ct);
    }


}