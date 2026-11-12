using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MyApp.Web.Controllers;
using Xunit;

namespace TpiProg4Foros.Tests.Web;

// These tests verify declared endpoint authorization metadata.
// They do NOT replace end-to-end HTTP authorization tests.
public class AuthorizationPolicyTests
{
    private static AuthorizeAttribute? Policy(Type controller, string method) =>
        controller.GetMethod(method)!
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true).SingleOrDefault();

    [Theory]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("ChangeState")]
    public void Forum_mutations_require_admin_or_sysadmin(string method)
    {
        Assert.Equal("Admin,SysAdmin", Policy(typeof(ForumController), method)?.Roles);
    }

    [Fact]
    public void Deleting_forum_requires_sysadmin()
    {
        Assert.Equal("SysAdmin", Policy(typeof(ForumController), "Delete")?.Roles);
    }

    [Theory]
    [InlineData("NewPost")]
    [InlineData("EditPost")]
    [InlineData("HideOrUnhidePost")]
    [InlineData("DeletePost")]
    [InlineData("GetMyPosts")]
    public void Post_owner_endpoints_require_authenticated_user(string method)
    {
        Assert.NotNull(Policy(typeof(PostController), method));
    }

    [Theory]
    [InlineData("UpdateUser")]
    [InlineData("DeleteUser")]
    public void User_account_modifications_require_authentication(string method)
    {
        Assert.NotNull(Policy(typeof(UserController), method));
    }

    [Fact]
    public void Changing_roles_requires_sysadmin()
    {
        Assert.Equal("SysAdmin", Policy(typeof(UserController), "ChangeUserRole")?.Roles);
    }
}
