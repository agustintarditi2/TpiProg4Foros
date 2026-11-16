
using MyApp.Domain.ValueObjects;
using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using MyApp.Application.Interfaces;

namespace MyApp.Application.Services;

public sealed class ReactionServices
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IForumRepository _forumRepository;
    private readonly BanServices _bans;

    public ReactionServices(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IForumRepository forumRepository,
        BanServices bans)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _forumRepository = forumRepository;
        _bans = bans;
    }

    private async Task<User> GetAuthorizedUser(
        Guid userId,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required.");

        // recupera el usuario con sus reacciones
        var user = await _userRepository.GetByIdWithReactions(
            (UserId)userId, ct);

        if (user is null)
            throw new NotFoundException(
                "Requested user not found.");

        // verifica que la cuenta este activa
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "User account is inactive.");

        // verifica que el usuario no este baneado
        await _bans.EnsureNotBanned(userId, ct);

        return user;
    }

    private async Task<Post> GetAvailablePost(
        PostId postId,
        CancellationToken ct)
    {
        if (postId.IsEmpty)
            throw new DomainException("Post ID is required.");

        // verifica que la publicacion exista
        var post = await _postRepository.GetById(postId, ct);

        if (post is null)
            throw new NotFoundException(
                "Requested post not found.");

        // impide reaccionar a publicaciones ocultas
        if (post.Invisible)
            throw new DomainException(
                "Cannot react to a hidden or suppressed post.");

        // verifica que el foro exista
        var forum = await _forumRepository.GetById(
            post.ForumId, ct);

        if (forum is null)
            throw new NotFoundException(
                "Requested forum not found.");

        // verifica que el foro este activo
        if (!forum.State)
            throw new DomainException(
                "Cannot react to posts in an inactive forum.");

        return post;
    }

    // agregar o cambiar reaccion a una publicacion
    
    public async Task SetPostReaction(
        Guid postId,
        Guid userId,
        bool isLike,
        CancellationToken ct)
    {
        var user = await GetAuthorizedUser(userId, ct);

        // verifica que la publicacion permita reacciones
        await GetAvailablePost((PostId)postId, ct);

        // agrega o reemplaza la reaccion
        user.SetPostReaction((PostId)postId, isLike);

        // guarda los cambios
        await _userRepository.Update(user, ct);
    }

    // eliminar reaccion de una publicacion
   
    public async Task RemovePostReaction(
        Guid postId,
        Guid userId,
        CancellationToken ct)
    {
        if (postId == Guid.Empty)
            throw new DomainException("Post ID is required.");

        var user = await GetAuthorizedUser(userId, ct);

        // elimina la reaccion si existe
        user.RemovePostReaction((PostId)postId);

        // guarda los cambios
        await _userRepository.Update(user, ct);
    }

    // agregar o cambiar reaccion a un comentario

    public async Task SetCommentReaction(
        Guid commentId,
        Guid userId,
        bool isLike,
        CancellationToken ct)
    {
        var user = await GetAuthorizedUser(userId, ct);

        if (commentId == Guid.Empty)
            throw new DomainException("Comment ID is required.");

        // verifica que el comentario exista
        var comment = await _commentRepository.GetById(
            (CommentId)commentId, ct);

        if (comment is null)
            throw new NotFoundException(
                "Requested comment not found.");

        // impide reaccionar a comentarios eliminados
        if (comment.IsDeleted)
            throw new DomainException(
                "Cannot react to a deleted comment.");

        // impide reaccionar a comentarios ocultos
        if (comment.Invisible)
            throw new DomainException(
                "Cannot react to a hidden or suppressed comment.");

        // verifica la publicacion y el foro del comentario
        await GetAvailablePost(comment.ParentPost, ct);

        // agrega o reemplaza la reaccion
        user.SetCommentReaction(
            (CommentId)commentId, isLike);

        // guarda los cambios
        await _userRepository.Update(user, ct);
    }

    // eliminar reaccion de un comentario
    
    public async Task RemoveCommentReaction(
        Guid commentId,
        Guid userId,
        CancellationToken ct)
    {
        if (commentId == Guid.Empty)
            throw new DomainException("Comment ID is required.");

        var user = await GetAuthorizedUser(userId, ct);

        // elimina la reaccion si existe
        user.RemoveCommentReaction((CommentId)commentId);

        // guarda los cambios
        await _userRepository.Update(user, ct);
    }
}
