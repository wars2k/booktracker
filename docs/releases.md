# Releases

## Current status

This is planned delivery policy for [DaHokeyPokey/booktracker](https://github.com/DaHokeyPokey/booktracker), not an active publisher or release checklist proven by execution. No active `.github/workflows` directory exists in the inspected workspace; no remote Actions activation, registry visibility, or image publication was verified. No fork image has been verified or published by this workflow.

The intended public namespace is `ghcr.io/dahokeypokey/booktracker`. It is not a working installation target. The original `wars2k/booktracker:latest` is not a verified build of this fork or an automatic rollback substitute. See [deployment status](deployment.md#status-and-prerequisites).

## Approved design defaults, not activated settings

| Area | Planned behavior |
| --- | --- |
| Project and PR base | `DaHokeyPokey/booktracker`, branch `main` |
| CI provider | GitHub Actions in this repository |
| Quality checks | PRs targeting `main` and ordinary `main` pushes check quality; neither publishes images |
| Image platform | `linux/amd64` initially; no implied multi-architecture build |
| Publication trigger | A maintainer-published stable GitHub Release with a validated `vX.Y.Z` tag, not a PR, ordinary push, draft, or prerelease |
| Image identity | Record version and commit tags plus an immutable registry digest |
| `latest` policy | Move only for the deliberately current stable release; never implicitly for an older release or prerelease |
| Delivery boundary | Image publication only; no automatic host deployment |

Being a GitHub fork must not disable this project's own workflow. Future publish guards must validate the exact approved repository/namespace rather than blanket-rejecting `fork=true`. No upstream PR/sync, fork detachment, or remote rewiring is required.

## Gates before first publication

Each implementation/review and any external activation needs its own approval. Current documentation approval authorizes none of these operations:

1. Implement and verify fork CI quality/security gates. Untrusted PRs must not receive registry credentials or publication privileges; PR/main checks never publish.
2. Complete clean image/context packaging and the immediate privacy review. The current Dockerfile copies host `Booktracker/external` data and is unsafe to build with user data; a successful source build is not packaging/privacy evidence.
3. Complete release plumbing and its security review, then the requested frontend follow-ups and their reviews in the recorded order. Release plumbing can be prepared while publication stays disabled.
4. Separately approve and resolve the supported-runtime assessment/upgrade. Both .NET projects currently target unsupported `net7.0`. The eventual publisher must reject that unsupported target **before registry login or write**. .NET 7 compatibility checks are not production-release approval.
5. Separately approve first publication, public package visibility, and any required GitHub-admin changes. These are not implied by the intended public namespace or by completed local code/reviews.

The proposed publisher must verify repository, stable tag, exact commit SHA/main-line eligibility, runtime gate and image facts before granting publish privilege. Do not promote untrusted PR artifacts/caches into a trusted release. A maintainer publishing a release could trigger the future workflow after activation; do not create a release as a harmless validation probe.

## Final release facts

Story 004 Task 002 (release plumbing) is responsible for updating this guide, deployment guidance and README with final verified release facts. Do not fill them in from assumptions during the documentation task.

Before treating an image as available, record:

- Actual registry/namespace and verified package visibility/availability.
- Published stable version/tag, exact source commit SHA, and recorded platform.
- Published version/commit image references and immutable digest.
- Whether `latest` was intentionally moved, and why this is the current stable release.
- Actual quality, packaging/privacy, release/security and runtime evidence; distinguish local checks from remote Actions and registry results.
- The prior verified own-image version/digest and data-compatibility/backup recovery plan, if a rollback target exists.

Do not advertise an available image, invent a digest/version, or add runnable GHCR pull/Compose instructions until those facts are verified under the relevant approval. Publication does not update any running host: the operator separately chooses and approves deployment and any [backup/rollback](deployment.md#backups-and-rollback) actions.

## Validation limits

This task checks documentation locally only. It does not run Git, CI, builds/tests, containers, live-provider calls, release creation, registry authentication/writes, visibility changes, or deployment. External links are text references, not proof of reachability or remote state. See [development](development.md) for the existing focused compatibility checks and their limitations.
