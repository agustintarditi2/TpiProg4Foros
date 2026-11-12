using MyApp.Application.Interfaces;
using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class PostBanTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakeForums _forums = new();
    private readonly FakePosts _posts = new();
    private readonly FakeClock _clock = new();
    private readonly IBanRepository _banRepository;
    private readonly BanRepositoryProxy _bans;

    public PostBanTests()
    {
        (_banRepository, _bans) = BanTestSupport.CreateRepository();
    }

    private PostServices Service => BanTestSupport.CreatePost(
        _posts, _forums, _users, _clock, new FakeSanitizer(), _banRepository);

    private (User Author, Forum Forum) SeedBannedUser()
    {
        var author = Sample.User();
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);
        var forum = Sample.Forum();
        _users.Items[author.Id.Value] = author;
        _forums.Items[forum.Id.Value] = forum;
        var ban = new Ban(author, admin, 5, "Violation");
        _bans.Items[ban.Id.Value] = ban;
        return (author, forum);
    }

    [Fact]
    public async Task Banned_user_cannot_create_post()
    {
        var (author, forum) = SeedBannedUser();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Create(new CreatePostDTO(forum.Id.Value, "Message"), author.Id.Value, Ct));

        Assert.Empty(_posts.Items);
    }

    [Fact]
    public async Task Banned_user_cannot_edit_post()
    {
        var (author, forum) = SeedBannedUser();
        var post = Sample.Post(author, forum);
        _posts.Items[post.Id.Value] = post;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Update(post.Id.Value, "Edited", author.Id.Value, Ct));

        Assert.Equal("Hello", post.Body.Value);
        Assert.Equal(0, _posts.Updates);
    }
}
