# Development

This fork's primary project and PR base are [DaHokeyPokey/booktracker](https://github.com/DaHokeyPokey/booktracker) and its `main` branch. See [contribution guidance](../CONTRIBUTING.md) for change boundaries.

## Current environment and limits

- The application and console harness both target `net7.0`. .NET 7 is out of support. A compatible .NET 7 SDK environment is for isolated compatibility checks, not a supported-production-runtime claim. Runtime/dependency upgrades require separate planning and approval.
- The existing JavaScript checks require Node.js 18 or newer. Use a supported Node.js release; this minimum describes test compatibility, not lifecycle support. No npm install or frontend build tool is needed for these checks.
- The current workflow host is recorded as lacking a local .NET SDK; SDK availability has not been rechecked or changed. Historical Docker validation is not standing approval to start containers, pull images, restore packages, or run tests. Arrange an explicitly approved SDK environment first.
- A [CI definition](../.github/workflows/ci.yml) now configures focused checks. No broad automated coverage, remotely validated CI, verified fork image, or validated general local-startup procedure is claimed here.

## Focused checks

From the repository root, in an approved validation environment:

```bash
dotnet restore Booktracker.Tests/Booktracker.Tests.csproj
dotnet run --project Booktracker.Tests/Booktracker.Tests.csproj --no-restore
node --test Booktracker.Tests/ci.test.js Booktracker.Tests/search.test.js
```

Restore can contact NuGet. `dotnet run --no-restore` still builds and executes the console harness; it does not enforce network isolation. Offline execution needs separately configured isolation after the required dependencies and compiled output are available. Obtain environment/network/build/test/container approvals before executing the relevant operations.

The [console project](../Booktracker.Tests/Booktracker.Tests.csproj) is an executable, not a `dotnet test` project. Its fake HTTP responses exercise Google Books requests/retries and provider failure handling. [Endpoint checks](../Booktracker.Tests/EndpointTests.cs) create temporary SQLite data and remove the temporary directories; no real key, live Google request, or user database is needed.

The [search checks](../Booktracker.Tests/search.test.js) execute `search.js` with stub fetch/DOM objects, including request-ordering cases. The [CI checks](../Booktracker.Tests/ci.test.js) guard the workflow's fixed text layout and reject unsafe event/permission/pin/command mutations; they are not a YAML parser or actual Actions execution. Neither suite is a browser rendering, layout, accessibility, or full-application check. Compilation alone does not validate authentication, ownership, persistence, or deployment safety. Historical passing results are not a fresh run.

## CI definition and activation

[`.github/workflows/ci.yml`](../.github/workflows/ci.yml) configures two stable check names, `Node checks` and `.NET compatibility`, for pull requests targeting `main` and pushes to `main`. Its exact repository guard allows `DaHokeyPokey/booktracker` even when GitHub marks it as a fork. Runs use hosted `ubuntu-24.04`, 10/20-minute job limits and cancellation of older checks for the same PR/ref.

- Checkout/setup-node use reviewed full commit pins, checkout credentials are not persisted, permissions are `contents: read`, and package-manager caching is disabled. There are no secret inputs, privileged PR triggers, release artifacts, registry writes or deployment steps.
- The Node job selects `24.22.1`, verifies the pinned actionlint archive SHA-256 before extraction/execution, validates workflow files, then runs every `Booktracker.Tests/*.test.js` suite. No frontend package installation is needed. Both actions use Node 24 and require runner >=2.327.1; a different local Node patch is not proof of the configured CI environment.
- The backend job uses digest-pinned .NET 7 SDK/ASP.NET images for `linux/amd64`. One SDK container restores the console project and builds it in Release (including its application project reference), preserving existing warnings. A separate runtime container mounts the compiled checks read-only and executes `dotnet /checks/Booktracker.Tests.dll` from `/tmp` with `--network none`; fixtures create disposable data there. It does not start the application server or build the repository Dockerfile.
- **Network isolation applies to the harness container, not image pulls or restore/build.** MCR/GitHub downloads and NuGet restore require their own approval. .NET 7 remains unsupported compatibility-only, not production-release approval.

CI uses a fresh repository checkout, not a working directory containing real `external/` data or credentials. Do not reuse its SDK bind-mount recipe against user data. Never supply a production Google Books key to these checks.

Local policy-test passes do not establish valid YAML, working containers or active remote CI. Before claiming activation:

1. Obtain approval for the pinned workflow validator, isolated Release build/offline console run and any downloads; record the actual results or explicit missing-evidence limits.
2. The owner manages branch/commit/push and enables Actions in this fork if required. No Git, dispatch, settings, cancel/disable or branch-rule action is automatic.
3. Observe the expected own-fork PR/main check names, statuses, permissions and failure behavior using approved inspection or owner-provided evidence. No remote run has been verified in this work.
4. Complete the required CI/security review before image/release work. Compatibility CI does not waive the supported-runtime or first-publication gates.

## Working directory and source builds

The application uses process-relative paths. Its working directory must provide `init.sql`, static content under `wwwroot/`, and writable `external/db`, `external/log`, and `external/export` directories. The database path is `external/db/database.db`. Starting from the wrong directory can fail or select a different database; do not experiment against user data.

[Startup](../Booktracker/Program.cs) calls [database bootstrap](../Booktracker/DB.cs) before building the web host. Bootstrap reads `init.sql` and executes split statements; this is not a versioned migration/rollback system. Do not infer safe schema upgrades from startup code or a successful build.

The [Dockerfile](../Dockerfile) records restore/build steps, runtime working directory `/app`, and `dotnet Booktracker.dll`. **Do not build the current Dockerfile with real `Booktracker/external` data:** it copies that directory into the image, and the current [.dockerignore](../.dockerignore) is insufficient to exclude sensitive build context. Safe packaging is separate work. No new container-build or general startup recipe is introduced here.

Keep source/build work isolated from real databases, logs, exports, keys, and production configuration. Preserve existing routes/casing, SQLite compatibility, DLL/product names and page globals. Follow [CONTRIBUTING](../CONTRIBUTING.md) before protected-boundary changes.

## Related guides

- [Deployment contracts and backup/rollback responsibilities](deployment.md)
- [Planned releases and publication gates](releases.md)
- [README and original-image status](../README.md#installation)

These guides document current source facts and planned delivery, not permission to run commands or activate external systems. Report checks as passed, failed, or not run, with their actual limits.
