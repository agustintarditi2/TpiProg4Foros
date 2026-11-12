# Automated tests for TpiProg4Foros

This branch runs the **same production code as `develop`** plus a dedicated .NET 10 xUnit test project. It does not modify `develop`.

## Run

From the repository root with the .NET 10 SDK:

```bash
dotnet restore TpiProg4Foros.slnx
dotnet test TpiProg4Foros.slnx --configuration Release
```

For coverage:

```bash
dotnet test tests/TpiProg4Foros.Tests/TpiProg4Foros.Tests.csproj --collect:"XPlat Code Coverage"
```

GitHub Actions: [Automated tests](../../actions/workflows/tests.yml). The workflow restores, builds, tests, and uploads TRX + coverage artifacts on pushes to `Tests` and relevant pull requests.

## Scope

- **Forum (Domain/Application):** required fields, boundary lengths, atomic updates, active/inactive transitions, persistence and missing-entity responses.
- **Post (Domain/Application):** required forum and body, text normalization, editing, visibility, author-only modifications, inactive forum restrictions, hidden-item public filtering.
- **User (Domain/Application):** registration, email and password-hash validation, role permissions, profile updates, deactivation, age boundary, immutable date of birth.
- **Authentication and authorization:** credentials, JWT claims, inactive accounts, role-based endpoint metadata.
- **Ban (Domain/Application):** role hierarchy, duration and reason, active ban uniqueness, expiration/revocation, persisted status changes, banned-user restrictions on posts and authentication.
- **Web:** JwtBearer `OnTokenValidated` event and endpoint authorization metadata.
- **Infrastructure:** active-ban query using isolated SQLite `Data Source=:memory:`, never the repository's checked-in database.

The test collaborators use per-test isolated fakes. The JWT event test registers `BanServices` to match its production dependency injection needs.

## Latest verified run — 2026-11-12

[GitHub Actions run 43314944851](../../actions/runs/43314944851):

- Build: **passed**.
- Tests: **155 passed, 2 failed, 0 skipped (157 total)**.

The two failing tests demonstrate defects in production. **Do not make these tests pass by weakening the assertions or by changing `src/` on the `Tests` branch.** Fix production in a normal feature/fix branch, merge it into `develop`, and resynchronize `Tests`.

1. **`BanTests.Admin_cannot_ban_another_admin`:** in `src/Domain/Entities/Ban.cs`, the current condition `user.Role > admin.Role` allows same-role bans, although the documented business rule requires the target to have a strictly lower role. Review whether to reject `>=` (plus any special SysAdmin policy).
2. **`BanRepositoryTests.Query_returns_active_bans_only_for_requested_user`:** the SQLite/EF Core model fails to initialize. `CommentConfiguration` maps `Comment.Invisible`, but `Invisible` in `TextEntity` is a computed read-only property with no backing field. `PostConfiguration` maps the same property, so it also needs review. The test currently cannot reach the actual ban query until mapping is fixed.

## Business rule to confirm

The current `develop` version of `User.cs` requires **16 years** for registration. This is what the tests assert because the test branch must match production. A previous discussion specified **13 years**. Do not change the assertions to 13 without an approved production rule change.

## Coverage limitations and precautions

- Endpoint metadata and the JWT event are tested; there is **no complete HTTP integration test with a signed JWT and real 401/403 responses**.
- Full delete-cascade scenarios for deeply nested comments are not covered.
- The `BanRepositoryTests` SQLite test cannot validate its query until the EF mapping error above is resolved.
- A passing mock-based test does not guarantee database migrations and real HTTP requests work.
- `UserServices.ChangeEmail` may reject the user's existing email if the submitted casing differs; this is not covered by the current suite.
- A new commit on `develop` does not automatically update `Tests`; merge/synchronize `develop` again to avoid running tests against stale production code.
