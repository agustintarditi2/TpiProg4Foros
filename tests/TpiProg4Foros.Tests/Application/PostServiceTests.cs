using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class PostServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakePosts _posts = new();
    private readonly FakeForums _forums = new();
    private readonly FakeUsers _users = new();
    private readonly FakeClock _clock = new();

    private PostServices Service => BanTestSupport.CreatePost(
        _posts, _forums, _users, _clock, new FakeSanitizer(),
        BanTestSupport.CreateRepository().Repository);

    [Fact]
    public async Task Create_requires_authenticated_author()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreatePostDTO(Guid.NewGuid(), "Content"), Guid.Empty, Ct));
        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Create_requires_existing_forum()
    {
        var author = Sample.User();
        _users.Items[author.Id.Value] = author;
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.Create(new CreatePostDTO(Guid.NewGuid(), "Content"), author.Id.Value, Ct));
        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Create_rejects_inactive_forum()
    {
        var author = Sample.User();
        var forum = Sample.Forum();
        forum.Deactivate();
        _users.Items[author.Id.Value] = author;
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreatePostDTO(forum.Id.Value, "Content"), author.Id.Value, Ct));
        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Create_requires_existing_author()
    {
        var forum = Sample.Forum();
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.Create(new CreatePostDTO(forum.Id.Value, "Content"), Guid.NewGuid(), Ct));
        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Create_saves_valid_post_with_authenticated_author()
    {
        var author = Sample.User();
        var forum = Sample.Forum();
        _users.Items[author.Id.Value] = author;
        _forums.Items[forum.Id.Value] = forum;

        var result = await Service.Create(
            new CreatePostDTO(forum.Id.Value, "  Hello  "), author.Id.Value, Ct);

        Assert.Single(_posts.Items);
        Assert.Equal(author.Id.Value, result.Poster);
        Assert.Equal(forum.Id.Value, result.Forum);
        Assert.Equal("Hello", result.Body);
    }

    [Fact]
    public async Task Create_rejects_invalid_body_without_saving()
    {
        var author = Sample.User();
        var forum = Sample.Forum();
        _users.Items[author.Id.Value] = author;
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreatePostDTO(forum.Id.Value, " "), author.Id.Value, Ct));
        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Only_author_can_edit()
    {
        var owner = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(owner, forum);
        _posts.Items[post.Id.Value] = post;
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Update(post.Id.Value, "Changed", Guid.NewGuid(), Ct));
        Assert.Equal("Hello", post.Body.Value);
        Assert.Equal(0, _posts.Updates);

        await Service.Update(post.Id.Value, "Changed", owner.Id.Value, Ct);
        Assert.Equal("Changed", post.Body.Value);
        Assert.True(post.Edited);
        Assert.Equal(1, _posts.Updates);
    }

    [Fact]
    public async Task Cannot_edit_in_inactive_forum()
    {
        var owner = Sample.User();
        var forum = Sample.Forum();
        forum.Deactivate();
        var post = Sample.Post(owner, forum);
        _posts.Items[post.Id.Value] = post;
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Update(post.Id.Value, "Changed", owner.Id.Value, Ct));
        Assert.Equal(0, _posts.Updates);
    }

    [Fact]
    public async Task Invalid_edit_does_not_save()
    {
        var owner = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(owner, forum);
        _posts.Items[post.Id.Value] = post;
        _forums.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Update(post.Id.Value, " ", owner.Id.Value, Ct));
        Assert.Equal("Hello", post.Body.Value);
        Assert.Equal(0, _posts.Updates);
    }

    [Fact]
    public async Task Only_author_can_change_visibility()
    {
        var owner = Sample.User();
        var post = Sample.Post(owner, Sample.Forum());
        _posts.Items[post.Id.Value] = post;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.HideOrUnhideSelf(post.Id.Value, Guid.NewGuid(), true, Ct));
        Assert.False(post.Invisible);

        await Service.HideOrUnhideSelf(post.Id.Value, owner.Id.Value, true, Ct);
        Assert.True(post.Invisible);
        Assert.Equal(1, _posts.Updates);
    }

    [Fact]
    public async Task Only_author_can_delete()
    {
        var owner = Sample.User();
        var post = Sample.Post(owner, Sample.Forum());
        _posts.Items[post.Id.Value] = post;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Delete(post.Id.Value, Guid.NewGuid(), Ct));
        Assert.Single(_posts.Items);

        await Service.Delete(post.Id.Value, owner.Id.Value, Ct);
        Assert.Empty(_posts.Items);
        Assert.Equal(1, _posts.Deletes);
    }

    [Fact]
    public async Task Public_queries_exclude_hidden_posts_but_mine_includes_them()
    {
        var author = Sample.User();
        var forum = Sample.Forum();
        var visible = Sample.Post(author, forum, "Visible");
        var hidden = Sample.Post(author, forum, "Hidden");
        hidden.ToggleHidden(true);
        _posts.Items[visible.Id.Value] = visible;
        _posts.Items[hidden.Id.Value] = hidden;

        Assert.Single(await Service.GetByForum(forum.Id.Value, Ct));
        Assert.Single(await Service.GetByPoster(author.Id.Value, Ct));
        Assert.Equal(2, (await Service.GetMyPosts(author.Id.Value, Ct)).Count);
        await Assert.ThrowsAsync<NotFoundException>(() => Service.GetById(hidden.Id.Value, Ct));
    }

    [Fact]
    public async Task Missing_posts_throw_not_found_for_modifications()
    {
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<NotFoundException>(() => Service.GetById(id, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.Update(id, "Text", Guid.NewGuid(), Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.HideOrUnhideSelf(id, Guid.NewGuid(), true, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.Delete(id, Guid.NewGuid(), Ct));
    }
}
