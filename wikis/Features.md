# Features

What Konnect does, organized by domain area. **How it's built** lives on the [Architecture](Architecture) page; **what's running** lives under [Infrastructure](infrastructure/Overview); **what each feature looks like in detail** is on its own per-feature page once shipped.

## How to read this page

- **Shipped** — behavior is in the running code today; click the wiki link for the deep-dive page.
- **In Progress** — an open pull request exists for the feature; click the issue link to find the PR.
- **Planned** — a GitHub Story / sub-issue captures the spec; no implementation yet. The issue body is the source of truth — this page is just the index.

The wiki documents what is **implemented**, not what is planned. For Planned items, the description, acceptance criteria, and design decisions live in the linked GitHub issue, **not** on this page.

## Standing rule

Every pull request that adds, ships, or removes a feature updates this page in the same PR — same convention as the existing wiki-in-same-PR rule:

- New Story created → add a row with status **Planned**.
- Implementation PR opened → flip status to **In Progress**.
- Implementation PR merges → flip status to **Shipped** and link the per-feature wiki page.
- Feature deprecated or scope reshaped → update the row.

Out-of-date rows are bugs.

---

## Foundation

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| 8-csproj solution layout under `Konnect.Platform/` | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [Architecture](Architecture) |
| Local docker-compose stack (Postgres + pgvector, RabbitMQ, Fuseki, smtp4dev) | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [Infrastructure overview](infrastructure/Overview) |
| GitHub Actions CI (build · test · EF migration script · coverage gate) | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [CI Pipeline](infrastructure/CI-Pipeline) |
| Branch protection on `main` + Konnect Backend project board | Shipped | [#7](https://github.com/win-son-dev/konnect-server/issues/7) | — |
| Root README + All Rights Reserved licence + ESCO attribution | Shipped | [#18](https://github.com/win-son-dev/konnect-server/issues/18) | — |
| Wiki auto-publish from `wikis/` to GitHub Wiki | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [Home](Home) |
| Architecture wiki page (8-csproj layering + dependency graph + WebAPI exposure) | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [Architecture](Architecture) |
| Features wiki index (this page) | In Progress | [#42](https://github.com/win-son-dev/konnect-server/issues/42) (parent [#41](https://github.com/win-son-dev/konnect-server/issues/41)) | this page |

## Authentication & accounts

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Identity tables + roles (`JobSeeker`, `Recruiter`, `CompanyAdmin`, `Admin`) + initial migration | Planned | [#21](https://github.com/win-son-dev/konnect-server/issues/21) (parent [#20](https://github.com/win-son-dev/konnect-server/issues/20)) | — |
| Seeker auth — `POST /api/auth/seeker/{register,login,refresh}` (`aud: "seeker"` JWT) | Planned | [#22](https://github.com/win-son-dev/konnect-server/issues/22) | — |
| Employer auth — `POST /api/auth/employer/*` with atomic Company + first Recruiter | Planned | [#23](https://github.com/win-son-dev/konnect-server/issues/23) | — |
| Authorization policies (`RequireJobSeeker`, `RequireRecruiter`, `RequireCompanyAdmin`, `RequireOwnsPosting`, etc.) | Planned | [#26](https://github.com/win-son-dev/konnect-server/issues/26) | — |

## Companies & recruiters

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Multi-recruiter Company model (Company has many Recruiter; CompanyAdmin role) | Planned | [#21](https://github.com/win-son-dev/konnect-server/issues/21) | — |
| Company GraphQL — public `Query.company(slug)` + recruiter-scoped queries / `updateCompany` mutation | Planned | [#24](https://github.com/win-son-dev/konnect-server/issues/24) | — |
| Recruiter invitation flow (existing CompanyAdmin invites another recruiter) | Planned | _(deferred to Phase 1.5 — needs email infrastructure)_ | — |

## Job postings

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Job posting GraphQL — public `Query.jobs` + recruiter-scoped `Mutation.employer.{create,update,delete}Job` | Planned | [#25](https://github.com/win-son-dev/konnect-server/issues/25) | — |
| Job posting expiry (`ExpiresAt` enforcement) | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5 — `ExpirePostingsFunction`) | — |
| Job posting `JobPosted` event publishing on create/update | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3 — added when embeddings land) | — |

## Applications

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Applications REST — apply / status update / withdraw with auth gating | Planned | [#28](https://github.com/win-son-dev/konnect-server/issues/28) (parent [#27](https://github.com/win-son-dev/konnect-server/issues/27)) | — |
| Applications GraphQL — `Query.me.applications` + `Query.employer.applicationsForPosting` | Planned | [#29](https://github.com/win-son-dev/konnect-server/issues/29) | — |
| Per-application messaging thread (seeker ↔ recruiters) | Planned | [#40](https://github.com/win-son-dev/konnect-server/issues/40) | — |

## Resume pipeline

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Resume upload — PDF/DOCX multipart, Azurite (local) / Azure Blob (real envs) | Planned | [#30](https://github.com/win-son-dev/konnect-server/issues/30) | — |
| Resume text extraction (raw text only, no AI) — `ResumeParseConsumer` | Planned | [#33](https://github.com/win-son-dev/konnect-server/issues/33) | — |
| Resume AI structured extraction (`ParsedJson` — name, summary, experience, skills) | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| Resume embeddings (pgvector, 768-dim, HNSW cosine) | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| Skill canonicalization on resume parse (raw label → ESCO URI) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| Orphaned resume blob sweep (daily) | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |

## Search

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Semantic search (pgvector cosine, top-K) — REST + GraphQL | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| Lexical search (Postgres FTS via `tsvector` + GIN) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| Graph-expanded search (ESCO skill-broader / occupation-narrower hops) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| 3-lane fan-out + weighted merge (`0.4 * lex + 0.4 * sem + 0.2 * graph`) with per-lane timeouts | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |

## Skills graph (ESCO + Apache Jena Fuseki)

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Fuseki container + ESCO RDF loader script | Shipped | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | [Apache Jena Fuseki](infrastructure/Apache-Jena-Fuseki) |
| `SkillsGraphRepository` — SPARQL queries (related skills · occupations using skills · narrower-occupations recursion) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| `SkillResolverService` — canonical ESCO URI from raw skill label (exact → trigram → embedding match) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| `Skill` and `Occupation` Postgres mirror tables (cheap join, canonical row in Jena) | Planned | [#35](https://github.com/win-son-dev/konnect-server/issues/35) (Phase 4) | — |
| ESCO monthly refresh (download → stage in side dataset → atomic swap) | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5 — `EscoReimportFunction`) | — |

## AI layer

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| `IAiRepository` interface (`CompleteAsync` · `ExtractStructuredAsync<T>` · `GenerateEmbeddingAsync`) | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| `AiRepository` — single class, internal switch on `Ai:Provider` config (Gemini · Ollama · OpenAI placeholder) | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| Embedding dimension locked at 768 across providers + startup integrity check | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |
| `IVectorStoreRepository` + `VectorStoreRepository` (pgvector backend; future Qdrant swap is a DI flip) | Planned | [#34](https://github.com/win-son-dev/konnect-server/issues/34) (Phase 3) | — |

## Asynchronous infrastructure

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| MassTransit + `Konnect.Worker` host bootstrap + smoke consumer | Planned | [#31](https://github.com/win-son-dev/konnect-server/issues/31) | [RabbitMQ](infrastructure/RabbitMQ) |
| Postgres outbox (atomic publish from `KonnectDbContext` transaction) | Planned | [#32](https://github.com/win-son-dev/konnect-server/issues/32) | [RabbitMQ](infrastructure/RabbitMQ) |
| Retry / redelivery / DLQ topology (3 immediate · 1m/5m/30m delayed · `*-error` queue) | Planned | [#31](https://github.com/win-son-dev/konnect-server/issues/31) | [RabbitMQ](infrastructure/RabbitMQ) |

## Notifications

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| `IEmailRepository` — single class, smtp4dev (local) / SendGrid (prod) by config | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| `EmailNotificationService` + `EmailConsumer` (consumes `notification.email`) | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| In-app notifications — `GET /api/me/notifications` · mark-as-read · GraphQL parity | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |

## Seeker tools

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| Job seeker profile — view, update, photo upload (reuses `BlobStorageRepository`), recruiter view (gated) | Planned | [#37](https://github.com/win-son-dev/konnect-server/issues/37) | — |
| Saved jobs (bookmark postings) — REST + GraphQL | Planned | [#38](https://github.com/win-son-dev/konnect-server/issues/38) | — |
| Job alerts — saved query + cadence; CRUD now, scheduled dispatch in Phase 5 | Planned | [#39](https://github.com/win-son-dev/konnect-server/issues/39) | — |

## Scheduled jobs (`Konnect.Serverless`)

All scheduled functions; consumers stay in `Konnect.Worker`.

| Feature | Status | Issue / Story | Wiki |
|---|---|---|---|
| `Konnect.Serverless` skeleton (isolated worker + first health timer + architecture test that asserts no `IConsumer<>` references) | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| `AlertDigestFunction` — daily 07:00 | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| `ExpirePostingsFunction` — every 15 min | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| `EscoReimportFunction` — monthly | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |
| `OrphanedDocumentSweepFunction` — daily | Planned | [#36](https://github.com/win-son-dev/konnect-server/issues/36) (Phase 5) | — |

---

## Phased delivery view

The same features arranged by execution phase, for planning:

| Phase | Story | Focus | State |
|---|---|---|---|
| Phase 0 | [#1](https://github.com/win-son-dev/konnect-server/issues/1) | Repo bootstrap — solution + compose + CI + branch protection + wiki | Done |
| Phase 1 | [#20](https://github.com/win-son-dev/konnect-server/issues/20) | Auth + Jobs CRUD (multi-recruiter, audience-split) | Planned |
| Phase 2 | [#27](https://github.com/win-son-dev/konnect-server/issues/27) | Applications + resume upload pipeline (text only — no AI yet) | Planned |
| Phase 3 | [#34](https://github.com/win-son-dev/konnect-server/issues/34) | AI layer (`IAiRepository` + Gemini/Ollama) + semantic search | Planned |
| Phase 4 | [#35](https://github.com/win-son-dev/konnect-server/issues/35) | Skills graph (ESCO + Fuseki) + 3-lane fan-out search | Planned |
| Phase 5 | [#36](https://github.com/win-son-dev/konnect-server/issues/36) | `Konnect.Serverless` + notifications (email + alerts + scheduled jobs) | Planned |

Cross-cutting Stories ship between phases as scope and dependencies allow:

| Cross-cutting Story | Earliest start | Issue |
|---|---|---|
| Job seeker profile | After Phase 1 (Phase 2 for recruiter view gating) | [#37](https://github.com/win-son-dev/konnect-server/issues/37) |
| Saved jobs | After Phase 1 | [#38](https://github.com/win-son-dev/konnect-server/issues/38) |
| Job alerts (CRUD now, dispatch in Phase 5) | After Phase 1 | [#39](https://github.com/win-son-dev/konnect-server/issues/39) |
| Application messaging thread | After Phase 2 | [#40](https://github.com/win-son-dev/konnect-server/issues/40) |
| Documentation foundation (Features index — this page) | Now | [#41](https://github.com/win-son-dev/konnect-server/issues/41) |
