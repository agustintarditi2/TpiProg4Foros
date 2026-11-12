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
- **Infrastructure:** active-ban query and visibility-flag persistence for posts and comments using isolated SQLite `Data Source=:memory:`, never the repository's checked-in database.

The test collaborators use per-test isolated fakes. The JWT event test registers `BanServices` to match its production dependency injection needs.

## Latest verified run — 2026-11-12

[GitHub Actions run 43326226544](../../actions/runs/43326226544) (production synchronized with `develop` commit `3b5dd4f`):

- Build: **passed**.
- Tests: **165 passed, 0 failed, 0 skipped**.
- The original **157 tests all pass**, including the previously failing `BanRepositoryTests.Query_returns_active_bans_only_for_requested_user` SQLite test.
- Added **8 SQLite round-trip cases** across posts and comments covering all combinations of `HiddenByUser` and `SuppressedByAdmin`. Each verifies that the individual flags and computed `Invisible` state survive reloading in a new `ApplicationDbContext`.
- The `PersistVisibilityFlags` migration on `develop` adds `SuppressedByAdmin` to both tables and retains the existing `Hidden` column; the test suite uses `EnsureCreated` and does not run actual up/down migrations.

## Business rule to confirm

The current `develop` version of `User.cs` requires **16 years** for registration. This is what the tests assert because the test branch must match production. A previous discussion specified **13 years**. Do not change the assertions to 13 without an approved production rule change.

## Coverage limitations and precautions

- Endpoint metadata and the JWT event are tested; there is **no complete HTTP integration test with a signed JWT and real 401/403 responses**.
- Full delete-cascade scenarios for deeply nested comments are not covered.
- SQLite round-trip tests use a fresh in-memory database and do **not** validate upgrading an existing SQLite database via migrations or real HTTP requests.
- `UserServices.ChangeEmail` may reject the user's existing email if the submitted casing differs; this is not covered by the current suite.
- A new commit on `develop` does not automatically update `Tests`; merge/synchronize `develop` again to avoid running tests against stale production code.
