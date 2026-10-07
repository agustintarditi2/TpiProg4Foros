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
        var entity = await_repository.GetById(id);
        return PostDTO.Create(entity); 
    }

}