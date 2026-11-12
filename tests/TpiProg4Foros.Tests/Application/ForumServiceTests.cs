using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class ForumServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task Create_persists_valid_forum()
    {
        var repo = new FakeForums();
        var result = await new ForumServices(repo).Create(new CreateForumDTO(" New ", " Description "), Ct);
        Assert.Single(repo.Items);
        Assert.True(result.State);
        Assert.Equal("New", result.Title);
    }

    [Fact]
    public async Task Create_rejects_invalid_fields_without_persisting()
    {
        var repo = new FakeForums();
        await Assert.ThrowsAsync<DomainException>(() =>
            new ForumServices(repo).Create(new CreateForumDTO("", "Description"), Ct));
        Assert.Empty(repo.Items);
    }

    [Fact]
    public async Task Update_requires_existing_forum()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new ForumServices(new FakeForums()).Update(Guid.NewGuid(),
                new UpdateForumDTO("Title", "Description"), Ct));
    }

    [Fact]
    public async Task Update_invalid_data_does_not_save()
    {
        var repo = new FakeForums();
        var forum = Sample.Forum();
        repo.Items[forum.Id.Value] = forum;

        await Assert.ThrowsAsync<DomainException>(() =>
            new ForumServices(repo).Update(forum.Id.Value,
                new UpdateForumDTO("Changed", " "), Ct));

        Assert.Equal("General", forum.Title);
        Assert.Equal(0, repo.Updates);
    }

    [Fact]
    public async Task ChangeState_persists_transitions_and_rejects_repeats()
    {
        var repo = new FakeForums();
        var forum = Sample.Forum();
        repo.Items[forum.Id.Value] = forum;
        var service = new ForumServices(repo);

        await service.ChangeState(forum.Id.Value, false, Ct);
        Assert.False(forum.State);
        Assert.Equal(1, repo.Updates);
        await Assert.ThrowsAsync<DomainException>(() => service.ChangeState(forum.Id.Value, false, Ct));

        await service.ChangeState(forum.Id.Value, true, Ct);
        Assert.True(forum.State);
        Assert.Equal(2, repo.Updates);
    }

    [Fact]
    public async Task Get_and_delete_missing_forums_throw_not_found()
    {
        var service = new ForumServices(new FakeForums());
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetById(Guid.NewGuid(), Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => service.Delete(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Delete_removes_existing_forum()
    {
        var repo = new FakeForums();
        var forum = Sample.Forum();
        repo.Items[forum.Id.Value] = forum;

        await new ForumServices(repo).Delete(forum.Id.Value, Ct);

        Assert.Empty(repo.Items);
        Assert.Equal(1, repo.Deletes);
    }
}
