# RabbitMQ

> Broker running locally via docker compose. The `Konnect.Worker` project that will host MassTransit consumers exists but is empty other than the csproj. This page will expand to cover the topology, exchanges, and outbox configuration when they're added.

## What's running

The image is `rabbitmq:4-management` — RabbitMQ 4.x with the management UI plugin enabled, so the broker exposes both the AMQP wire protocol and a browser-based admin UI.

```mermaid
flowchart LR
    Compose["docker compose up -d"] --> Container[konnect-rabbitmq<br/>rabbitmq:4-management]
    Container --> Volume[(named volume:<br/>rabbitmq-data)]
    Container --> Amqp[127.0.0.1:5672<br/>AMQP]
    Container --> UI[127.0.0.1:15672<br/>management UI]
    Container --> Health["healthcheck:<br/>rabbitmq-diagnostics -q ping"]

    classDef infra fill:#ff6600,stroke:#cc4400,color:#fff
    class Container,Volume,Amqp,UI,Health infra
```

## Connection

| Setting | Value |
|---|---|
| Host | `127.0.0.1` |
| AMQP port | `5672` |
| Management UI | http://127.0.0.1:15672 |
| User | `konnect` |
| Password | `konnect_dev_only` |
| Vhost | `konnect` |

AMQP URI:

```
amqp://konnect:konnect_dev_only@127.0.0.1:5672/konnect
```

## Healthcheck

Compose runs `rabbitmq-diagnostics -q ping` every 10s after a 15s startup grace period. The grace period is generous because RabbitMQ takes a few seconds to fully boot.

## Volume

The named volume `rabbitmq-data:/var/lib/rabbitmq` persists message state and definitions across restarts.

```bash
docker compose down -v    # wipes the volume — fresh broker on next up
```

## Common operations

```bash
# Open the management UI (creds: konnect / konnect_dev_only)
open http://127.0.0.1:15672

# Verify the broker is up and the vhost exists
docker compose exec rabbitmq rabbitmqctl list_vhosts
```

## Code touchpoints

| File | Role |
|---|---|
| [`Konnect.Platform/docker-compose.yml`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/docker-compose.yml) | Container, healthcheck, volume, vhost env vars |

The `Konnect.Worker` project exists but currently contains only the csproj. The MassTransit setup, consumers, and outbox configuration will be documented here when they're added.
