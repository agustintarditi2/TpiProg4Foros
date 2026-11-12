using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class ForumTests
{
    [Fact]
    public void Constructor_creates_active_forum_and_trims_fields()
    {
        var forum = new Forum("  General  ", "  Discussions  ");

        Assert.NotEqual(Guid.Empty, forum.Id.Value);
        Assert.Equal("General", forum.Title);
        Assert.Equal("Discussions", forum.Description);
        Assert.True(forum.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_empty_title(string? title) =>
        Assert.Throws<DomainException>(() => new Forum(title!, "Description"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_empty_description(string? description) =>
        Assert.Throws<DomainException>(() => new Forum("Title", description!));

    [Fact]
    public void Title_accepts_100_characters_but_not_101()
    {
        Assert.Equal(100, new Forum(new string('x', 100), "Description").Title.Length);
        Assert.Throws<DomainException>(() => new Forum(new string('x', 101), "Description"));
    }

    [Fact]
    public void Description_accepts_500_characters_but_not_501()
    {
        Assert.Equal(500, new Forum("Title", new string('x', 500)).Description.Length);
        Assert.Throws<DomainException>(() => new Forum("Title", new string('x', 501)));
    }

    [Fact]
    public void Update_trims_both_fields()
    {
        var forum = new Forum("Old title", "Old description");
        forum.Update(" New title ", " New description ");
        Assert.Equal("New title", forum.Title);
        Assert.Equal("New description", forum.Description);
    }

    [Fact]
    public void Update_rejects_invalid_fields_without_partial_mutation()
    {
        var forum = new Forum("Original", "Original description");

        Assert.Throws<DomainException>(() => forum.Update("Changed", " "));
        Assert.Equal("Original", forum.Title);
        Assert.Equal("Original description", forum.Description);

        Assert.Throws<DomainException>(() => forum.Update(" ", "Changed"));
        Assert.Equal("Original", forum.Title);
        Assert.Equal("Original description", forum.Description);

        Assert.Throws<DomainException>(() => forum.Update(new string('x', 101), "Changed"));
        Assert.Throws<DomainException>(() => forum.Update("Changed", new string('x', 501)));
    }

    [Fact]
    public void Deactivate_then_activate_changes_state()
    {
        var forum = new Forum("Title", "Description");
        forum.Deactivate();
        Assert.False(forum.State);
        forum.Activate();
        Assert.True(forum.State);
    }

    [Fact]
    public void Deactivating_twice_is_rejected()
    {
        var forum = new Forum("Title", "Description");
        forum.Deactivate();
        Assert.Throws<DomainException>(forum.Deactivate);
    }

    [Fact]
    public void Activating_twice_is_rejected()
    {
        var forum = new Forum("Title", "Description");
        Assert.Throws<DomainException>(forum.Activate);
    }
}
