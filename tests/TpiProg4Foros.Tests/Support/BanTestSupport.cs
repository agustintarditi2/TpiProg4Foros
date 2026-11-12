using System.Reflection;
using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;

namespace TpiProg4Foros.Tests.Support;

// DispatchProxy keeps the tests compatible with both repository contracts:
// the current develop version (void Update/Delete, no HasActiveBan)
// and the version being implemented (Task Update/Delete + HasActiveBan).
public class BanRepositoryProxy : DispatchProxy
{
    public Dictionary<Guid, Ban> Items { get; } = [];
    public int Adds { get; private set; }
    public int Updates { get; private set; }
    public int Deletes { get; private set; }

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method is null || args is null)
            throw new InvalidOperationException("Invalid repository invocation.");

        switch (method.Name)
        {
            case "Get":
                return Task.FromResult(Items.Values.ToList());
            case "GetById":
                return Task.FromResult(Items.GetValueOrDefault(((BanId)args[0]!).Value));
            case "Add":
            {
                var ban = (Ban)args[0]!;
                Items[ban.Id.Value] = ban;
                Adds++;
                return Task.FromResult(ban);
            }
            case "Update":
            {
                var ban = (Ban)args[0]!;
                Items[ban.Id.Value] = ban;
                Updates++;
                return method.ReturnType == typeof(void) ? null : Task.CompletedTask;
            }
            case "Delete":
            {
                var ban = (Ban)args[0]!;
                Items.Remove(ban.Id.Value);
                Deletes++;
                return method.ReturnType == typeof(void) ? null : Task.CompletedTask;
            }
            case "GetActiveByUserId":
            {
                var userId = (UserId)args[0]!;
                return Task.FromResult(Items.Values
                    .Where(b => b.UserId == userId && b.Status == BanStatus.Activo)
                    .ToList());
            }
            case "HasActiveBan":
            {
                var userId = (UserId)args[0]!;
                var date = ((DateTime)args[1]!).Date;
                var found = Items.Values.Any(b =>
                    b.UserId == userId &&
                    b.Status == BanStatus.Activo &&
                    b.StartDate <= date &&
                    b.EndDate > date);
                return Task.FromResult(found);
            }
            default:
                throw new NotSupportedException($"Unexpected IBanRepository call: {method.Name}");
        }
    }
}

internal static class BanTestSupport
{
    public static (IBanRepository Repository, BanRepositoryProxy Spy) CreateRepository()
    {
        var repository = DispatchProxy.Create<IBanRepository, BanRepositoryProxy>();
        return (repository, (BanRepositoryProxy)(object)repository);
    }

    // The Tests branch may temporarily use the previous four-argument constructor.
    // Adapt only test creation, never production code, while branches are synchronized.
    public static AuthServices CreateAuth(
        IUserRepository users, IBanRepository bans, IPasswordHasher hasher,
        IJwtServices jwt, IClock clock)
    {
        var constructor = typeof(AuthServices).GetConstructors().Single();
        var args = constructor.GetParameters().Select(parameter =>
        {
            if (parameter.ParameterType == typeof(IUserRepository)) return (object)users;
            if (parameter.ParameterType == typeof(IBanRepository)) return bans;
            if (parameter.ParameterType == typeof(BanServices)) return new BanServices(bans, users);
            if (parameter.ParameterType == typeof(IPasswordHasher)) return hasher;
            if (parameter.ParameterType == typeof(IJwtServices)) return jwt;
            if (parameter.ParameterType == typeof(IClock)) return clock;
            throw new InvalidOperationException($"Unknown AuthServices dependency: {parameter.ParameterType.Name}");
        }).ToArray();
        return (AuthServices)constructor.Invoke(args);
    }

    // PostServices on newer develop versions has a BanServices dependency.
    public static PostServices CreatePost(
        IPostRepository posts, IForumRepository forums, IUserRepository users,
        IClock clock, ISanitizer sanitizer, IBanRepository bans)
    {
        var constructor = typeof(PostServices).GetConstructors().Single();
        var args = constructor.GetParameters().Select(parameter =>
        {
            if (parameter.ParameterType == typeof(IPostRepository)) return (object)posts;
            if (parameter.ParameterType == typeof(IForumRepository)) return forums;
            if (parameter.ParameterType == typeof(IUserRepository)) return users;
            if (parameter.ParameterType == typeof(IClock)) return clock;
            if (parameter.ParameterType == typeof(ISanitizer)) return sanitizer;
            if (parameter.ParameterType == typeof(BanServices)) return new BanServices(bans, users);
            throw new InvalidOperationException($"Unknown PostServices dependency: {parameter.ParameterType.Name}");
        }).ToArray();
        return (PostServices)constructor.Invoke(args);
    }
}
