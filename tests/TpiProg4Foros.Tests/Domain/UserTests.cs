using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_initializes_role_identity_email_and_active_account()
    {
        var user = new User("  Ana  ", new DateOnly(2000, 2, 2),
            "  ANA@EXAMPLE.COM  ", "valid-password-hash", Sample.Now);

        Assert.Equal("Ana", user.Name);
        Assert.Equal("ana@example.com", user.Email.Value);
        Assert.Equal(UserRole.User, user.Role);
        Assert.True(user.IsActive);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
        Assert.Equal(Sample.Now, user.DateCreated);
        Assert.NotNull(user.Reactions);
        Assert.Empty(user.Reactions!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required_when_creating_user(string? name) =>
        Assert.Throws<DomainException>(() => new User(name!, new DateOnly(2000, 1, 1),
            "ana@example.com", "valid-password-hash", Sample.Now));

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(15)]
    public void Registration_rejects_users_younger_than_16(int age)
    {
        var now = new DateTimeOffset(2026, 11, 12, 12, 0, 0, TimeSpan.Zero);
        var birthDate = DateOnly.FromDateTime(now.Date).AddYears(-age);

        Assert.Throws<DomainException>(() => new User(
            "Ana", birthDate, "ana@example.com", "valid-password-hash", now));
    }

    [Fact]
    public void Registration_rejects_user_one_day_before_16th_birthday()
    {
        var now = new DateTimeOffset(2026, 11, 12, 12, 0, 0, TimeSpan.Zero);
        var birthDate = DateOnly.FromDateTime(now.Date).AddYears(-16).AddDays(1);

        Assert.Throws<DomainException>(() => new User(
            "Ana", birthDate, "ana@example.com", "valid-password-hash", now));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    public void Registration_accepts_users_from_16th_birthday(int age)
    {
        var now = new DateTimeOffset(2026, 11, 12, 12, 0, 0, TimeSpan.Zero);
        var birthDate = DateOnly.FromDateTime(now.Date).AddYears(-age);

        var user = new User(
            "Ana", birthDate, "ana@example.com", "valid-password-hash", now);

        Assert.Equal(birthDate, user.DateOfBirth);
    }

    [Fact]
    public void Future_birth_date_is_rejected()
    {
        Assert.Throws<DomainException>(() => new User("Ana", new DateOnly(2030, 1, 1),
            "ana@example.com", "valid-password-hash", Sample.Now));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("ana@")]
    [InlineData("ana@example")]
    [InlineData("")]
    public void Invalid_email_is_rejected(string email) =>
        Assert.Throws<DomainException>(() => new User("Ana", new DateOnly(2000, 1, 1),
            email, "valid-password-hash", Sample.Now));

    [Theory]
    [InlineData("")]
    [InlineData("123456789012")]
    [InlineData("too-short")]
    public void Invalid_password_hash_is_rejected(string hash) =>
        Assert.Throws<DomainException>(() => new User("Ana", new DateOnly(2000, 1, 1),
            "ana@example.com", hash, Sample.Now));

    [Fact]
    public void Password_hash_cannot_exceed_64_characters()
    {
        var user = Sample.User();
        Assert.Throws<DomainException>(() => user.ChangePassword(new string('a', 65)));
    }

    [Fact]
    public void Rename_trims_and_rejects_blank_names_without_changing_previous_name()
    {
        var user = Sample.User();
        user.Rename(" New name ");
        Assert.Equal("New name", user.Name);

        Assert.Throws<DomainException>(() => user.Rename(" "));
        Assert.Equal("New name", user.Name);
    }

    [Fact]
    public void ChangeEmail_normalizes_and_rejects_invalid_addresses()
    {
        var user = Sample.User();
        user.ChangeEmail("  New@EXAMPLE.ORG  ");
        Assert.Equal("new@example.org", user.Email.Value);

        Assert.Throws<DomainException>(() => user.ChangeEmail("bad-email"));
        Assert.Equal("new@example.org", user.Email.Value);
    }

    [Fact]
    public void ChangePassword_preserves_valid_hash()
    {
        var user = Sample.User();
        user.ChangePassword("replacement-hash");
        Assert.Equal("replacement-hash", user.Password.Value);
    }

    [Fact]
    public void Birth_date_has_no_public_setter_or_change_method()
    {
        var property = typeof(User).GetProperty(nameof(User.DateOfBirth));

        Assert.NotNull(property);
        Assert.False(property!.SetMethod?.IsPublic ?? false);
        Assert.Null(typeof(User).GetMethod("ChangeDateOfBirth"));
    }

    [Fact]
    public void User_patch_does_not_allow_birth_date_changes()
    {
        Assert.Null(typeof(MyApp.Application.Models.PatchUserDTO)
            .GetProperty("DateOfBirth"));
    }

    [Fact]
    public void Deactivate_anonymizes_personal_data_and_revokes_role()
    {
        var user = Sample.User();
        user.SetRole(UserRole.Admin);

        user.DeactivateAccount("disabled-password-hash");

        Assert.False(user.IsActive);
        Assert.Equal("Usuario eliminado", user.Name);
        Assert.StartsWith("deleted-", user.Email.Value);
        Assert.EndsWith("@example.invalid", user.Email.Value);
        Assert.Equal(new DateOnly(1900, 1, 1), user.DateOfBirth);
        Assert.Equal("disabled-password-hash", user.Password.Value);
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public void Cannot_deactivate_same_account_twice()
    {
        var user = Sample.User();
        user.DeactivateAccount("disabled-password-hash");
        Assert.Throws<DomainException>(() => user.DeactivateAccount("disabled-password-hash"));
    }
}
