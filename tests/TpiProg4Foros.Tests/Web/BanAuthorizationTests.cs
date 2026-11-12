using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MyApp.Web.Controllers;
using Xunit;

namespace TpiProg4Foros.Tests.Web;

// Verifies declared controller policies; does not simulate actual HTTP 401/403 responses.
public class BanAuthorizationTests
{
    [Theory]
    [InlineData("Get")]
    [InlineData("GetById")]
    [InlineData("CreateBan")]
    [InlineData("ModifyBan")]
    [InlineData("Desban")]
    [InlineData("DeleteBan")]
    public void Ban_management_requires_admin_or_sysadmin(string methodName)
    {
        var method = typeof(BanController).GetMethod(methodName);
        Assert.NotNull(method);

        var policy = method.GetCustomAttributes<AuthorizeAttribute>(true).SingleOrDefault();
        Assert.NotNull(policy);
        Assert.Equal("Admin,SysAdmin", policy.Roles);
    }
}
