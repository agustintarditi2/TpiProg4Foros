using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Web.Controllers;
using Xunit;

namespace TpiProg4Foros.Tests.Web;

public class ReactionEndpointTests
{
    [Theory]
    [InlineData(typeof(PostController), "SetPostReaction", "PUT")]
    [InlineData(typeof(PostController), "RemovePostReaction", "DELETE")]
    [InlineData(typeof(CommentController), "SetCommentReaction", "PUT")]
    [InlineData(typeof(CommentController), "RemoveCommentReaction", "DELETE")]
    public void Reaction_endpoints_require_authentication_and_expose_correct_verb(
        Type controller, string methodName, string verb)
    {
        var method = controller.GetMethod(methodName);
        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
        var http = method.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal("{id:guid}/reaction", http.Template);
        Assert.Contains(verb, http.HttpMethods);
    }

    [Theory]
    [InlineData(typeof(PostController), "SetPostReaction")]
    [InlineData(typeof(CommentController), "SetCommentReaction")]
    public void Set_reaction_accepts_like_dislike_as_boolean(Type controller, string methodName)
    {
        var method = controller.GetMethod(methodName);
        Assert.NotNull(method);
        var body = method!.GetParameters().Single(p =>
            p.GetCustomAttribute<FromBodyAttribute>() is not null);
        Assert.Equal(typeof(bool), body.ParameterType);
    }
}
