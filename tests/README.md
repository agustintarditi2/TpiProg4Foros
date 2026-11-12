# Automated tests (Forum, Post, User, Ban)

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
- **Domain / User:** initial state, names, email/password hash, minimum registration age of 16 (boundary tests), absence of public birth-date editing, role change, anonymizing deactivation.
- **Application / Forum:** persistence, missing forum, invalid updates, state transitions.
- **Application / Post:** authenticated author, existing/active forum, existing author, own edit/hide/delete, hidden post filtering.
- **Application / User:** registration (including age 16 accepted and under 16 rejected without persistence), unique email, own profile updates, SysAdmin-only role changes, soft deletion.
- **Application / Auth:** login and token validation, inactive account restrictions.
- **Web:** declared authorization attributes on mutating endpoints (metadata only).
- **Domain / Ban:** creation, invalid duration/reason, dates, modification, expiration and revocation.
- **Application / Ban:** creation, missing users/admins, invalid input, duplicate active bans, reading, expiration status refresh, modification, revocation, removal, and access guards.
- **Application / Auth with Ban:** reject banned users on login and explicit token validation; allow expired and revoked bans.
- **Application / Post with Ban:** prevent banned authors from creating and editing posts.
- **Infrastructure / Ban:** GetActiveByUserId query for active-status bans in isolated in-memory SQLite (not the real database).
- **Web / Ban:** Admin/SysAdmin authorization metadata and JwtBearer `OnTokenValidated` event behavior (without a real HTTP request).

Fakes are per-test in-memory collaborators, not the production SQLite repository.
The workflow `.github/workflows/tests.yml` runs on pushes to `Tests` and relevant pull requests.

## Known coverage limits / findings

- Full HTTP integration tests (including deletion cascading to nested comments), HTTP status codes and signed-token authorization are **not** covered. A focused SQLite repository test and JwtBearer event tests are included for bans.
- **Branch synchronization:** `develop` currently includes new ban rules (role hierarchy, no duplicate active ban, expiration refresh, `IBanRepository.GetActiveByUserId`, and `PostServices` ban guards) that have not been merged into `Tests`. `develop` does **not yet** contain ban checks in `AuthServices` or `AddJwtAuthentication`. Regression tests intentionally expose these gaps; expect red tests until production changes are integrated and the remaining behavior is implemented. This work changed only files inside `tests/`.
- Test helpers adapt to old/new `AuthServices` and `PostServices` constructors, and old/new `IBanRepository` method signatures, to keep the test project compilable while the branches are synchronized.
- Ban authorization tests inspect `[Authorize]` metadata only; they do not assert actual HTTP 401/403 responses.
- JWT event tests call the registered validation event directly; they do not perform real signed-JWT HTTP integration.
- `BanRepositoryTests` use an ephemeral SQLite database via `Data Source=:memory:`.
- Current `develop` permits an Admin to ban another Admin despite the comment requiring a lower target role. `BanTests.Admin_cannot_ban_another_admin` is a deliberate failing regression until that authorization rule is corrected.
- Post's domain constructor does not explicitly reject an empty `Poster` ID; the application service does reject an empty authenticated ID.
- **Age and birth-date regression:** `Tests` still has the earlier production `User.cs` (without minimum age and with `ChangeDateOfBirth`). New tests expect the agreed changes from `IntegrarForum`: registration permitted at 16, rejected before 16, and no public birth-date change method. Those cases will fail until the updated production code is merged into this branch.
- `UserServices.ChangeEmail` checks duplicate existence without exempting the current account. Re-submitting the same email may be rejected.
- `PostServices.Create` verifies an author exists but does not check `IsActive` on that user; authentication and other request-handling layers must enforce account status.

These are observations rather than behavior silently 'fixed' by tests. Existing production code is left unchanged.

## Test status

The commit was written through the GitHub connector. The authoring environment did not contain the .NET SDK, so compilation and passing test results must be verified by GitHub Actions or locally. Never treat unexecuted tests as passing.
