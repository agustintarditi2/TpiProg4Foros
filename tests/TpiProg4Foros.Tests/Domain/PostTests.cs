using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class PostTests
{
    [Fact]
    public void Constructor_sets_author_forum_date_and_initial_flags()
    {
        var user = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(user, forum, "  Initial content  ");

        Assert.NotEqual(Guid.Empty, post.Id.Value);
        Assert.Equal(user.Id, post.Poster);
        Assert.Equal(forum.Id, post.ForumId);
        Assert.Equal("Initial content", post.Body.Value);
        Assert.Equal(Sample.Now, post.DateCreated);
        Assert.Null(post.DateEdited);
        Assert.False(post.Edited);
        Assert.False(post.Hidden);
    }

    [Fact]
    public void Constructor_requires_a_forum()
    {
        var author = Sample.User();
        Assert.Throws<DomainException>(() =>
            new Post("Content", author.Id, (ForumId)Guid.Empty, Sample.Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_body(string body)
    {
        var author = Sample.User();
        var forum = Sample.Forum();
        Assert.Throws<DomainException>(() => new Post(body, author.Id, forum.Id, Sample.Now));
    }

    [Fact]
    public void Body_maximum_length_is_enforced()
    {
        var author = Sample.User();
        var forum = Sample.Forum();

        var valid = new Post(new string('a', BodyText.MaxLength), author.Id, forum.Id, Sample.Now);
        Assert.Equal(BodyText.MaxLength, valid.Body.Value.Length);

        Assert.Throws<DomainException>(() =>
            new Post(new string('a', BodyText.MaxLength + 1), author.Id, forum.Id, Sample.Now));
    }

    [Fact]
    public void Body_allows_line_breaks_and_tabs_but_rejects_control_characters()
    {
        var post = Sample.Post(Sample.User(), Sample.Forum(), "first\nsecond\tpart");
        Assert.Contains("\n", post.Body.Value);
        Assert.Contains("\t", post.Body.Value);

        Assert.Throws<DomainException>(() =>
            Sample.Post(Sample.User(), Sample.Forum(), "invalid\u0001text"));
    }

    [Fact]
    public void Body_normalizes_unicode_to_form_c()
    {
        var post = Sample.Post(Sample.User(), Sample.Forum(), "cafe\u0301");
        Assert.Equal("café", post.Body.Value);
    }

    [Fact]
    public void EditBody_records_edit_time_and_flag()
    {
        var post = Sample.Post(Sample.User(), Sample.Forum());
        var editedAt = Sample.Now.AddMinutes(10);

        post.EditBody("  Updated  ", editedAt);

        Assert.Equal("Updated", post.Body.Value);
        Assert.Equal(editedAt, post.DateEdited);
        Assert.True(post.Edited);
    }

    [Fact]
    public void Invalid_edit_leaves_body_and_metadata_unchanged()
    {
        var post = Sample.Post(Sample.User(), Sample.Forum(), "Original");

        Assert.Throws<DomainException>(() => post.EditBody(" ", Sample.Now.AddMinutes(10)));

        Assert.Equal("Original", post.Body.Value);
        Assert.False(post.Edited);
        Assert.Null(post.DateEdited);
    }

    [Fact]
    public void Visibility_can_be_hidden_and_restored()
    {
        var post = Sample.Post(Sample.User(), Sample.Forum());
        post.HideOrUnhide(true);
        Assert.True(post.Hidden);
        post.HideOrUnhide(false);
        Assert.False(post.Hidden);
    }
}
