# Contributing to Booktracker

## Project and pull requests

This fork's primary project is [DaHokeyPokey/booktracker](https://github.com/DaHokeyPokey/booktracker). Open [pull requests in this repository](https://github.com/DaHokeyPokey/booktracker/pulls) with base branch [`main`](https://github.com/DaHokeyPokey/booktracker/tree/main). The original project is not the default contribution destination; upstream synchronization or fork detachment is not required.

Keep each contribution focused. Preserve the Booktracker product/DLL names, existing API routes and casing, request shapes, SQLite compatibility, and page-global JavaScript contracts unless a change is explicitly agreed upon.

Discuss authentication/authorization, schema or data changes, import/export, logging, rich HTML, dependencies/runtime, public API contracts, and container/deployment changes before implementing them. Do not bundle these protected changes into unrelated work.

## Focused development checks

Run these commands from the repository root in an approved, isolated validation environment:

```bash
dotnet restore Booktracker.Tests/Booktracker.Tests.csproj
dotnet run --project Booktracker.Tests/Booktracker.Tests.csproj --no-restore
node --test Booktracker.Tests/ci.test.js Booktracker.Tests/search.test.js
```

- Both .NET projects target `net7.0` and require a compatible .NET 7 SDK for these checks. .NET 7 is out of support; compatibility checks do not establish production readiness or authorize a runtime upgrade/release.
- The Node checks require Node.js 18 or newer. Use a supported Node.js release; this minimum is not a lifecycle-support recommendation.
- Restore can contact NuGet. `dotnet run --no-restore` still builds and executes the harness; it does not enforce network isolation. Obtain any required environment/network approval before running commands.
- The .NET console harness uses fake Google Books responses. Endpoint checks create and remove disposable temporary SQLite data. No real Google Books key or user database is needed.
- The Node checks cover workflow-policy/unsafe-mutation guards and `search.js` with stub fetch/DOM objects. They do not parse YAML, execute Actions, or validate real-browser rendering, layout, accessibility, or the whole application.

The .NET check is a console application, not a `dotnet test` test project. There is no frontend npm test setup; do not substitute npm scripts or add tooling just to run the existing checks. The [CI definition and activation guide](docs/development.md#ci-definition-and-activation) describes `Node checks` and `.NET compatibility`, pinned inputs and owner-managed activation. Local policy checks do not establish valid YAML, working containers, broad test coverage or remotely active CI; validator, backend and remote execution remain separate approvals/evidence.

## Data, documentation, and review

- Never commit API keys, credentials, session values, or user databases/logs/exports from `external/`. Use synthetic disposable data for validation, not production/user data or live-provider credentials.
- Follow the existing ASP.NET Core Minimal API, SQLite, and static HTML/CSS/vanilla JavaScript patterns. Prefer existing helpers and standard-library functionality over new dependencies.
- Include a focused regression check for behavior changes where practical. Report checks as passed, failed, or not run, with their limits; stub-DOM or compilation results are not broad safety evidence.
- Update relevant documentation when behavior or setup changes. Keep planned, locally checked, remotely activated, published, and deployed capabilities distinct.
- See the [README image status](README.md#installation) before using an image example. The intended `ghcr.io/dahokeypokey/booktracker` namespace is not a verified or published fork image from this workflow. Do not present it as an available installation target.
- Preserve [LICENSE](LICENSE) and its original Copyright (c) 2023 wars2k notice in copies or substantial portions of the software. Contribution documentation does not replace or rewrite the MIT notice.
