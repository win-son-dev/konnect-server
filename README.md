# Konnect

Konnect is a job-board backend built in .NET 10. It pairs PostgreSQL (with `pgvector` for semantic search) with an Apache Jena Fuseki triplestore that holds the [ESCO](https://esco.ec.europa.eu/) skills/occupation ontology, so search supports lexical, semantic, and ontology-aware skill expansion. Async work (resume parsing, embedding generation, notifications, alerts) runs through RabbitMQ via MassTransit. AI providers — Gemini today, Ollama for local dev, OpenAI as a future option — sit behind a single `IAiClient` interface. Both REST and GraphQL are exposed from one ASP.NET Core process.

This is the backend only. There is no frontend in this repo.

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (SDK pinned in [`Konnect.Platform/global.json`](Konnect.Platform/global.json)) |
| Web framework | ASP.NET Core (REST controllers + GraphQL via HotChocolate) |
| Relational + vector store | PostgreSQL 17 with `pgvector` |
| Knowledge graph | Apache Jena Fuseki, populated with ESCO RDF |
| Messaging | RabbitMQ via MassTransit |
| AI | `IAiClient` with Gemini (default), Ollama (local), OpenAI (future) |
| Local mail catcher | smtp4dev (dev only — prod uses a real provider) |
| Tests | xUnit (integration tests will use Testcontainers when repository / messaging code lands) |
| CI | GitHub Actions — see [`.github/workflows/ci.yml`](.github/workflows/ci.yml) |

## Repo layout

The .NET solution lives in [`Konnect.Platform/`](Konnect.Platform/). The repo root holds only this README and Git/GitHub metadata.

| Project | Role |
|---|---|
| [`Konnect.Infrastructure`](Konnect.Platform/Konnect.Infrastructure/) | Interfaces, models, contracts. No implementations. |
| [`Konnect.Services`](Konnect.Platform/Konnect.Services/) | Business logic; implements `Infrastructure` interfaces. Domain packages are folders inside this project. |
| [`Konnect.Repositories`](Konnect.Platform/Konnect.Repositories/) | EF Core `DbContext` + repository implementations. |
| [`Konnect.GraphQL`](Konnect.Platform/Konnect.GraphQL/) | HotChocolate schema. Hosted by `Konnect.WebAPI`. |
| [`Konnect.WebAPI`](Konnect.Platform/Konnect.WebAPI/) | ASP.NET Core entry point — REST + GraphQL. |
| [`Konnect.Worker`](Konnect.Platform/Konnect.Worker/) | `Microsoft.Extensions.Hosting` worker that hosts MassTransit consumers. |
| [`Konnect.Serverless`](Konnect.Platform/Konnect.Serverless/) | Azure Functions (isolated worker) — scheduled jobs only. |
| [`Konnect.Tests`](Konnect.Platform/Konnect.Tests/) | xUnit. Folder structure mirrors source. |

The deeper layout, dependency graph, and per-component documentation live in the [wiki](https://github.com/win-son-dev/konnect-server/wiki) (auto-published from [`wikis/`](wikis/)).

## Quickstart

Prerequisites: .NET SDK 10.0.201 (auto-resolved via `global.json`), Docker, GitHub CLI.

```bash
# 1. Clone
gh auth login
gh repo clone win-son-dev/konnect-server
cd konnect-server

# 2. Bring up local infra (postgres, rabbitmq, fuseki, smtp4dev)
cd Konnect.Platform
docker compose up -d

# 3. Build
dotnet build Konnect.Platform.slnx

# 4. Run the API (REST + GraphQL on the launch profile's port)
dotnet run --project Konnect.WebAPI

# 5. Run the worker host (consumers will appear here as they're added)
dotnet run --project Konnect.Worker
```

`docker compose ps` should show every service `(healthy)` before the API starts. Connection details, ports, and credentials are documented in the [Local Development wiki page](https://github.com/win-son-dev/konnect-server/wiki/Local-Development).

## Local-only by design

Every published port in [`Konnect.Platform/docker-compose.yml`](Konnect.Platform/docker-compose.yml) binds to `127.0.0.1` — nothing in the dev stack is reachable from the LAN. The credentials in compose (`konnect_dev_only`) are convenience defaults for development and are not safe outside the developer's machine. Production credentials live in Azure Key Vault / GitHub Secrets and are injected via environment overrides into the actual hosting platform.

## Workflow

Issue-first, PR-based — never direct commits to `main`.

1. Open a Story issue + sub-issues on GitHub. One PR per sub-issue.
2. Branch off `main`: `dev/<issue-number>` for features/stories/tasks, `bug/<issue-number>` for bugs.
3. Make small, focused commits. Reference the issue (`Closes #N`).
4. Open a PR. CI runs automatically — build, test, EF migration script, structural coverage gate.
5. After review, merge with a plain merge commit (not squash).
6. Pull `main`, branch again.

Documentation lives in the wiki and ships in the same PR as the code change it describes — a code change with documentation impact (new endpoint, new container, changed wiring) updates the relevant wiki page in the same PR. The wiki is part of the codebase.

## Documentation

| Where | What |
|---|---|
| `README.md` (this file) | Landing page, quickstart, attributions. The only `.md` committed at the repo root. |
| [`wikis/`](wikis/) | Deep-dive documentation, auto-synced to the [GitHub Wiki](https://github.com/win-son-dev/konnect-server/wiki) by [`.github/workflows/publish-wiki.yml`](.github/workflows/publish-wiki.yml). |
| `docs/` (gitignored) | Local-only working notes — ADRs, planning notes, design sketches. Not committed. |

## Attribution

This project uses the **European Skills, Competences, Qualifications and Occupations (ESCO)** classification, © European Union, 2024, licensed under the [Creative Commons Attribution 4.0 International (CC BY 4.0)](https://creativecommons.org/licenses/by/4.0/) licence. ESCO is not bundled with this repo — it is downloaded into the Fuseki container at first run by [`Konnect.Platform/infra/fuseki/load-esco.sh`](Konnect.Platform/infra/fuseki/load-esco.sh). See the [Apache Jena Fuseki wiki page](https://github.com/win-son-dev/konnect-server/wiki/Apache-Jena-Fuseki) for the loader pipeline and dataset structure.
