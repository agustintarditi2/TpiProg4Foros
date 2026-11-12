# Automated tests (Forum, Post, User)

This suite tests the existing production behavior without changing `src/`.

## Run locally

Requires .NET SDK 10.

```bash
dotnet restore TpiProg4Foros.slnx
dotnet test TpiProg4Foros.slnx
```

Run from the repository root. For coverage:

```bash
dotnet test tests/TpiProg4Foros.Tests/TpiProg4Foros.Tests.csproj --collect:"XPlat Code Coverage"
```

## Coverage areas

- **Domain / Forum:** required title/description, trim, 100/500 boundaries, atomic updates, state transitions.
- **Domain / Post:** forum requirement, body validation/normalization, edit metadata and visibility.
- **Domain / User:** initial state, names, email/password hash, birth date, role change, anonymizing deactivation.
- **Application / Forum:** persistence, missing forum, invalid updates, state transitions.
- **Application / Post:** authenticated author, existing/active forum, existing author, own edit/hide/delete, hidden post filtering.
- **Application / User:** registration, unique email, own profile updates, SysAdmin-only role changes, soft deletion.
- **Application / Auth:** login and token validation, inactive account restrictions.
- **Web:** declared authorization attributes on mutating endpoints (metadata only).

Fakes are per-test in-memory collaborators, not the production SQLite repository.
The workflow `.github/workflows/tests.yml` runs on pushes to `Tests` and relevant pull requests.

## Known coverage limits / findings

- Integration tests against SQLite (including deletion cascading to nested comments), HTTP status codes and actual JWT authorization pipeline are **not** covered by these unit tests.
- Ban-related behavior is outside the scope of this suite.
- Post's domain constructor does not explicitly reject an empty `Poster` ID; the application service does reject an empty authenticated ID.
- User's constructor currently does not check the maximum age, while `ChangeDateOfBirth` does.
- `UserServices.ChangeEmail` checks duplicate existence without exempting the current account. Re-submitting the same email may be rejected.
- `PostServices.Create` verifies an author exists but does not check `IsActive` on that user; authentication and other request-handling layers must enforce account status.

These are observations rather than behavior silently 'fixed' by tests. Existing production code is left unchanged.

## Test status

The commit was written through the GitHub connector. The authoring environment did not contain the .NET SDK, so compilation and passing test results must be verified by GitHub Actions or locally. Never treat unexecuted tests as passing.
