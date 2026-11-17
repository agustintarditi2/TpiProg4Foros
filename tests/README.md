# Automated tests for TpiProg4Foros

This branch runs the **same production code as `main`** plus a dedicated .NET 10 xUnit test project. It does not modify `main`.

## Synchronization — 2026-11-17

The complete `src/` tree matches `main` commit `289bb19fcdc9c234056b87f45e26820249bbaa4b` exactly, including removal of the three obsolete Class1 files. Production code is copied without test-only changes. Other branches are untouched.

Ban authorization metadata tests now consider controller-level and action-level policies and reject anonymous overrides. Four additional HTTP cases check `GET /Ban`: anonymous (401), User (403), Admin (200), and SysAdmin (200). Current validation is available in the [Automated tests workflow](../../actions/workflows/tests.yml); the historical results below predate this synchronization.

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
- **Web:** JwtBearer `OnTokenValidated` event, endpoint authorization metadata, and real HTTP requests through the in-process ASP.NET Core TestServer with signed JWTs and role checks.
- **Infrastructure:** active-ban query and visibility-flag persistence for posts and comments using isolated SQLite `Data Source=:memory:`, never the repository's checked-in database.

Unit/application tests use isolated fakes. The HTTP integration tests use real services, repositories, the JWT validator and a fresh in-memory SQLite database for each test factory. The JWT event test registers `BanServices` to match its production dependency injection needs.

## Historical verified run before main synchronization — 2026-11-16

[GitHub Actions run 43901279859](../../actions/runs/43901279859), synchronized with the then-current `develop` commit `0dc1867`:

- Restore/build: **passed**.
- Tests: **222 passed, 0 failed, 0 skipped**.
- The previous **202 tests pass unchanged**, plus **20 HTTP integration cases** in `tests/TpiProg4Foros.Tests/Http/`.
- HTTP cases start the actual ASP.NET Core application via `WebApplicationFactory<PostController>` and `TestServer`; they exercise controllers, ASP.NET Core middleware, JwtBearer and its `OnTokenValidated` event, application services and real EF Core/SQLite repositories.
- Each factory keeps an independent SQLite `:memory:` connection open. Real login produces a signed JWT; test configuration uses a separate JWT secret, issuer and audience with the real validation logic. The existing `ForosTup.db` is never accessed.
- Cases cover public/authenticated requests; valid and invalid credentials; malformed, bad-signature and expired tokens; `User`, `Admin` and `SysAdmin` permissions; account deactivation; role changes; banning after token issuance; post creation/editing, public visibility, comment soft deletion, reactions and persistence.
- Assertions cover HTTP `200`, `201`, `204`, `400`, `401`, `403` and `404` responses.
- HTTP requests run **in process** rather than over a deployed HTTPS network. This verifies the real HTTP application pipeline but does not constitute an external deployment or browser test.

## Business rule to confirm

The current `main` version of `User.cs` requires **16 years** for registration. This is what the tests assert because the test branch must match production. A previous discussion specified **13 years**. Do not change the assertions to 13 without an approved production rule change.

## Coverage limitations and precautions

- HTTP integration now verifies a representative set of routes and genuine JWT handling; it does **not** cover every endpoint or external deployment/network/TLS configuration.
- The in-memory SQLite factory uses `EnsureCreated` and does not exercise every historical database migration. A separate reaction identity migration test uses a specially constructed earlier schema.
- Concurrency/races between simultaneous reactions or bans are not covered.
- Full delete-cascade scenarios for deeply nested comments remain uncovered.
- The external time API is not exercised in these tests, keeping CI independent of an internet service.
- `UserServices.ChangeEmail` may reject the user's existing email when the casing differs; this is not covered by the current suite.
- A new commit on `main` does not automatically synchronize `Tests`; compare branches before interpreting a run as reflecting current production code.
