# Local Development

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | `10.0.201` | Pinned in [`Konnect.Platform/global.json`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/global.json). `rollForward: latestFeature` allows newer 10.0.* patches |
| Docker | latest | For Postgres, RabbitMQ, Fuseki. Optional Ollama via the `ai-local` profile |
| GitHub CLI | latest (`brew install gh`) | Used by the issue-first workflow |

## First-time setup

```bash
# 1. Authenticate gh and pin this repo as the default
gh auth login
gh repo set-default win-son-dev/konnect-server

# 2. Bring up local infra (postgres, rabbitmq, fuseki)
cd Konnect.Platform
docker compose up -d

# 3. Build and run
dotnet build Konnect.Platform.slnx
dotnet run --project Konnect.WebAPI
```

WebAPI listens on whatever ASP.NET Core's launch profile picks (see `Konnect.WebAPI/Properties/launchSettings.json`). Today the only routes that respond are `POST /graphql` (returns `{ healthcheck: "ok" }`), `GET /graphql` (Banana Cake Pop UI in dev), and `GET /openapi/v1.json`. No REST controllers exist yet.

To run the worker process:

```bash
dotnet run --project Konnect.Worker
```

(The worker host has no consumers registered yet, so it just starts and idles.)

## Service ports

Every port binds to `127.0.0.1` — nothing is exposed to the LAN.

```mermaid
flowchart LR
    Dev[Developer machine<br/>127.0.0.1]

    Dev -->|5432| Postgres[(postgres<br/>pgvector/pgvector:pg17)]
    Dev -->|5672 AMQP<br/>15672 management| Rabbit[(rabbitmq<br/>4-management)]
    Dev -->|3030| Fuseki[(fuseki<br/>stain/jena-fuseki:5.0.0)]
    Dev -->|2525 SMTP<br/>5050 UI| Smtp[(smtp4dev<br/>rnwood/smtp4dev:v3)]
    Dev -. profile=ai-local<br/>11434 .-> Ollama[(ollama)]

    classDef infra fill:#264653,stroke:#1b2a3a,color:#fff
    classDef opt fill:#6c757d,stroke:#343a40,color:#fff
    class Postgres,Rabbit,Fuseki,Smtp infra
    class Ollama opt
```

| Service | URL | Credentials (dev only — production uses Key Vault) |
|---|---|---|
| Postgres | `postgresql://konnect:konnect_dev_only@127.0.0.1:5432/konnect` | user `konnect` / password `konnect_dev_only` |
| RabbitMQ AMQP | `amqp://konnect:konnect_dev_only@127.0.0.1:5672/konnect` | user `konnect` / password `konnect_dev_only` / vhost `konnect` |
| RabbitMQ management UI | http://127.0.0.1:15672 | as above |
| Fuseki | http://127.0.0.1:3030 | admin password `konnect_dev_only` |
| smtp4dev SMTP | `127.0.0.1:2525` | none (dev catcher — captures, never relays) |
| smtp4dev inbox UI | http://127.0.0.1:5050 | none |
| Ollama (optional) | http://127.0.0.1:11434 | none — start with `docker compose --profile ai-local up -d` |

## Day-to-day commands

```bash
# Build
dotnet build Konnect.Platform.slnx

# Test
dotnet test Konnect.Platform.slnx

# Test with coverage (matches CI)
dotnet test Konnect.Platform.slnx \
  --logger "trx;LogFileName=test-results.trx" \
  --collect:"XPlat Code Coverage"
```

## CI parity

Local commands above mirror what runs in [`.github/workflows/ci.yml`](https://github.com/win-son-dev/konnect-server/blob/main/.github/workflows/ci.yml). Four jobs run on every PR to `main` — see the [CI Pipeline page](infrastructure/CI-Pipeline) for detail.

## Workflow

Issue-first, PR-based — never direct commits to `main`.

1. Open a Story issue + sub-issues on GitHub (one PR per sub-issue).
2. Branch off `main`: `dev/<issue-number>` for features/stories/tasks, `bug/<issue-number>` for bugs.
3. Make small commits. Reference the issue in the commit message (`Closes #N`).
4. Open a PR; CI runs automatically.
5. After review approval, merge with a plain merge commit (not squash).
6. Pull `main`, branch again for the next sub-issue.

If a code change has documentation impact — a new endpoint, a new container, a changed wiring — update the relevant wiki page in the same PR. The wiki is part of the codebase, not a separate document.
