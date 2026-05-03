# Konnect

Konnect is a job board backend built in .NET 10. PostgreSQL (with pgvector) is the system-of-record store, Apache Jena Fuseki holds a skills/occupation knowledge graph populated from the [ESCO](https://esco.ec.europa.eu/) ontology, RabbitMQ via MassTransit handles async work, and AI providers sit behind a single interface so Gemini, Ollama, and OpenAI are interchangeable. Both REST and GraphQL are exposed from one ASP.NET Core process.

This wiki is the deep-dive layer for collaborators. The repo `README.md` is the landing page; ADRs and planning notes live local-only and are not published.

## Pages

- **[Architecture](Architecture)** — project layout, build configuration, what the WebAPI process exposes today.
- **[Local Development](Local-Development)** — prerequisites, ports, and the commands to get a working dev environment.
- **Infrastructure** — services that run as part of the system.
  - [Overview](infrastructure/Overview)
  - [PostgreSQL](infrastructure/PostgreSQL)
  - [RabbitMQ](infrastructure/RabbitMQ)
  - [Apache Jena Fuseki](infrastructure/Apache-Jena-Fuseki)
  - [Docker Compose](infrastructure/Docker-Compose)
  - [CI Pipeline](infrastructure/CI-Pipeline)
- **Features** — feature pages get added when each feature is implemented.
- **API** — endpoint pages get added when each endpoint is implemented.

## Documentation principle

The wiki documents **what is implemented**, not what is planned. A wiki page describes only behaviour you can observe in the running code today. If a code change has wiki impact — a new endpoint, a new container, a changed wiring — the wiki update ships in the same PR. The wiki is part of the codebase.

## Conventions

- Issue-first, PR-based. Every change starts with a GitHub Story + sub-issues; never direct commits to `main`.
- Naming is meaningful — full domain words, no `usr` / `cfg` / `mgr` / `svc` / `ctx` / `dto` / `repo` shorthand.
- Documentation is local-only by default. The exceptions: `README.md` files, and this `wikis/` folder (committed and synced to the GitHub Wiki by [`.github/workflows/publish-wiki.yml`](https://github.com/win-son-dev/konnect-server/blob/main/.github/workflows/publish-wiki.yml)).
- Local infra ports bind to `127.0.0.1` only — never reachable from the LAN.
