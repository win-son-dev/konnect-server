# PostgreSQL

## What's running

The image is `pgvector/pgvector:pg17` — official PostgreSQL 17 with the `pgvector` extension pre-installed. We picked this image (rather than vanilla `postgres:17` plus a custom init script) because the application needs vector storage and pre-baked is simpler than maintaining init scripts.

```mermaid
flowchart LR
    Compose["docker compose up -d"] --> Container[konnect-postgres<br/>pgvector/pgvector:pg17]
    Container --> Volume[(named volume:<br/>postgres-data)]
    Container --> Port[127.0.0.1:5432]
    Container --> Health["healthcheck:<br/>pg_isready -U konnect -d konnect"]

    classDef infra fill:#336791,stroke:#002b5b,color:#fff
    class Container,Volume,Port,Health infra
```

## Connection

| Setting | Value |
|---|---|
| Host | `127.0.0.1` |
| Port | `5432` |
| Database | `konnect` |
| User | `konnect` |
| Password | `konnect_dev_only` *(dev only — production uses Key Vault)* |
| Container data dir | `/var/lib/postgresql/data/pgdata` |

Connection string for local dev:

```
Host=127.0.0.1;Port=5432;Database=konnect;Username=konnect;Password=konnect_dev_only
```

## Healthcheck

Compose runs `pg_isready -U konnect -d konnect` every 5s after a 5s startup grace period. `docker compose ps` shows `(healthy)` when the database is accepting connections.

## Volume

The named volume `postgres-data:/var/lib/postgresql/data` survives `docker compose down` and `docker compose up -d`. To wipe and start fresh:

```bash
docker compose down -v   # -v removes named volumes; data is gone
docker compose up -d
```

## Common operations

```bash
# Open a psql shell against the running container
docker compose exec postgres psql -U konnect -d konnect

# Confirm pgvector is available
docker compose exec postgres psql -U konnect -d konnect \
  -c "SELECT extname, extversion FROM pg_extension ORDER BY extname;"
```

## Schema

The Phase 1 schema is documented on [Database Schema](Database-Schema). High level: nine tables — the seven ASP.NET Identity tables (`users`, `roles`, `user_roles`, `user_claims`, `user_logins`, `user_tokens`, `role_claims`) plus `companies` and `job_postings`, all in snake_case. The `users` table holds both `JobSeekerUser` and `RecruiterUser` rows via TPH (one table, `audience` discriminator column).

The `pgvector` extension is installed in the image but no `vector` columns exist yet — those land in Phase 3 (resume + job-posting embeddings).

## Code touchpoints

| File | Role |
|---|---|
| [`Konnect.Platform/docker-compose.yml`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/docker-compose.yml) | Container definition, healthcheck, volume |
| [`Konnect.Repositories/KonnectDbContext.cs`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/Konnect.Repositories/KonnectDbContext.cs) | EF Core session — `IdentityDbContext<User, IdentityRole<Guid>, Guid>` with snake_case naming + table renames forced on the Identity tables |
| [`Konnect.Repositories/Migrations/`](https://github.com/win-son-dev/konnect-server/tree/main/Konnect.Platform/Konnect.Repositories/Migrations) | EF migrations. The single `InitialCreate` migration ships every Phase 1 table. |
| [`Konnect.WebAPI/appsettings.Development.json`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/Konnect.WebAPI/appsettings.Development.json) | Dev `ConnectionStrings.Postgres` — points at the compose container above |
