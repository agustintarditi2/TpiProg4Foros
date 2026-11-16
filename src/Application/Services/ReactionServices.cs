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

    // valida al usuario autenticado y sus permisos
    private async Task<User> GetAuthorizedUser(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required.");

        var user = await _userRepository.GetByIdWithReactions((UserId)userId, ct);
        if (user is null)
            throw new NotFoundException("Requested user not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive.");

        await _bans.EnsureNotBanned(userId, ct);
        return user;
    }

    // verifica la publicacion y su foro
    private async Task<Post> GetAvailablePost(PostId postId, CancellationToken ct)
    {
        if (postId.IsEmpty)
            throw new DomainException("Post ID is required.");

        var post = await _postRepository.GetById(postId, ct);
        if (post is null)
            throw new NotFoundException("Requested post not found.");

        if (post.Invisible)
            throw new DomainException("Cannot react to a hidden or suppressed post.");

        var forum = await _forumRepository.GetById(post.ForumId, ct);
        if (forum is null)
            throw new NotFoundException("Requested forum not found.");

        if (!forum.State)
            throw new DomainException("Cannot react to posts in an inactive forum.");

        return post;
    }

    public async Task SetPostReaction(Guid postId, Guid userId, bool isLike, CancellationToken ct)
    {
        var user = await GetAuthorizedUser(userId, ct);
        await GetAvailablePost((PostId)postId, ct);
        user.SetPostReaction((PostId)postId, isLike);
        await _userRepository.Update(user, ct);
    }

    public async Task RemovePostReaction(Guid postId, Guid userId, CancellationToken ct)
    {
        if (postId == Guid.Empty)
            throw new DomainException("Post ID is required.");

        var user = await GetAuthorizedUser(userId, ct);
        // permite retirar una reaccion aunque el contenido este oculto
        user.RemovePostReaction((PostId)postId);
        await _userRepository.Update(user, ct);
    }

    public async Task SetCommentReaction(Guid commentId, Guid userId, bool isLike, CancellationToken ct)
    {
        var user = await GetAuthorizedUser(userId, ct);
        if (commentId == Guid.Empty)
            throw new DomainException("Comment ID is required.");

        var comment = await _commentRepository.GetById((CommentId)commentId, ct);
        if (comment is null)
            throw new NotFoundException("Requested comment not found.");

        if (comment.IsDeleted)
            throw new DomainException("Cannot react to a deleted comment.");

        if (comment.Invisible)
            throw new DomainException("Cannot react to a hidden or suppressed comment.");

        await GetAvailablePost(comment.ParentPost, ct);
        user.SetCommentReaction((CommentId)commentId, isLike);
        await _userRepository.Update(user, ct);
    }

    public async Task RemoveCommentReaction(Guid commentId, Guid userId, CancellationToken ct)
    {
        if (commentId == Guid.Empty)
            throw new DomainException("Comment ID is required.");

        var user = await GetAuthorizedUser(userId, ct);
        // permite retirar la reaccion aunque el comentario este oculto o eliminado
        user.RemoveCommentReaction((CommentId)commentId);
        await _userRepository.Update(user, ct);
    }
}
