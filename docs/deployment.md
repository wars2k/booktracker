# Deployment

## Status and prerequisites

No fork-owned image has been verified or published by this workflow. `ghcr.io/dahokeypokey/booktracker` is the intended namespace, not an available installation target. The [README's original-project Compose example](../README.md#installation) uses `wars2k/booktracker:latest`; it is not this fork's build, and its contents/availability have not been verified here. Do not substitute the planned namespace into that example and assume installation works.

The fork currently targets unsupported .NET 7. Compatibility tests do not establish production readiness. Safe image packaging, the required reviews/frontend follow-ups, a separately approved supported-runtime plan, and first-publication approval remain gates. The [release-plumbing task](releases.md#final-release-facts) must supply final verified image/version/digest instructions later.

**Do not build the current Dockerfile with real `Booktracker/external` data.** It copies that directory into the runtime image. The current `.dockerignore` only excludes `bin/`, `obj/`, and `Properties/`; it does not provide a safe data/secret/agent-state build context. Do not use a build as a way to validate user data.

## Existing container and persistence contract

These are source/documentation contracts, not a container smoke-test result:

| Item | Existing contract |
| --- | --- |
| Runtime working directory | `/app` |
| Runtime command recorded in Dockerfile | `dotnet Booktracker.dll` |
| Application listener | Port `5000` inside the container |
| Original README host-port example | `2341:5000`; the host-side port is operator-chosen |
| Persistent volume example | `./data:/app/external` |
| Database | `/app/external/db/database.db` |
| Logs | `/app/external/log/` |
| Exports | `/app/external/export/` |
| Required application assets | `/app/init.sql` and `/app/wwwroot/` |

With that volume mapping, the host's `data` directory contains `db`, `log`, and `export` subdirectories. Runtime writes must reach the intended persistent mount, not an image layer or an accidentally empty replacement directory. Ownership/writability and correct mount resolution require separately approved checks; this task does not change permissions, a container user, or data paths.

[Startup](../Booktracker/Program.cs) and [database code](../Booktracker/DB.cs) use relative paths and apply startup schema statements from `init.sql`. There is no versioned migration/rollback ledger. Do not infer database upgrade/downgrade compatibility or safe first-run behavior from these contracts alone.

## Runtime Google Books configuration

Supply any optional key at runtime through the operator's protected configuration/environment mechanism, not a Docker build argument, image layer, committed file, command transcript, or log. No real key belongs in an example.

Configuration first checks `GOOGLE_BOOKS_API_KEY`; when blank/whitespace it falls back to `GoogleBooks:ApiKey`. `GoogleBooks__ApiKey` is the ASP.NET environment-variable mapping for the latter. The client trims a supplied key. The [README](../README.md#installation) shows variable names and an empty configuration example only.

Without a key, unauthenticated Google Books requests remain possible, subject to provider limits. Provider failures are surfaced on Add New Book; Manual Entry remains the independent fallback. Do not validate credentials using live requests under this documentation approval, or assume the original image implements this fork's configuration behavior.

## Backups and rollback

Backups, retention/access controls, maintenance windows, restore verification, and host deployment are operator responsibilities. Database, log, and export contents may be private; do not inspect, upload, or copy user data into validation/build contexts.

Before an approved upgrade, the operator should:

1. Record the currently running **verified own-image** version, commit and immutable digest, plus relevant mount/runtime configuration. If no prior verified own image exists, there is no established own-image rollback target; do not substitute upstream `latest`.
2. Arrange downtime and stop all writers before taking a consistent filesystem backup. Do not blindly copy an actively written SQLite database. Preserve the full persistent `external` tree, including any SQLite journal/WAL sidecar files present; protect configuration/secrets separately.
3. Keep the backup intact and access-restricted. Verify a restore using a separate, disposable environment before relying on it. This task performs no backup, stop, copy, restore, or database action.
4. Confirm the proposed application's schema/data compatibility and recovery plan. Startup DDL is not a reversible migration system, and restarting an older image does not undo database writes or guarantee compatibility.

If an upgrade fails, an approved operator-controlled rollback selects the previous recorded own-image version/digest, checks database compatibility, and restores its paired backup only when required by the reviewed recovery plan. Do not overwrite the only backup or test rollback against the sole live database. Application restart also loses in-memory sessions; no session-continuity guarantee is made.

No runnable pull/deploy/restore commands are supplied for an unverified image. Final operator instructions must use actual verified release facts, not floating `latest` or invented digests.

## Security and validation limits

Existing authentication, authorization/ownership, rich HTML, logging/privacy, and unsupported-runtime risks remain unresolved by documentation. Public diagnostics are present in current startup code; reverse-proxy/TLS/security controls have not been validated. A reverse proxy or successful compilation is not proof of application security or production readiness.

No container, browser, database, remote registry, network, production configuration, or real data was exercised by this task. See [development checks](development.md) for their narrower scope and [release gates](releases.md) before any publication/deployment decision.
