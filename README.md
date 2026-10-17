# ProductApi – DevOps final assignment

A small REST API (.NET 10 minimal API + PostgreSQL) used as the application for the CI/CD pipeline in the NOVI DevOps final assignment.

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/` | Name, message and running version (Git commit SHA) |
| GET | `/health` | Health check, including the database connection |
| GET | `/products` | All products |
| GET | `/products/{id}` | One product |
| POST | `/products` | Create a product: `{ "name": "Coffee", "price": 2.50 }` |
| PUT | `/products/{id}` | Update a product |
| DELETE | `/products/{id}` | Delete a product |

## Run locally

Requirements: Docker Desktop.

```bash
docker compose up --build
curl http://localhost:8080/health
```

Run the API from your IDE instead: start only the database with `docker compose up db`, then `dotnet run --project src/ProductApi`.

## Tests

```bash
dotnet test
```

- `tests/ProductApi.UnitTests` – validation rules, no dependencies.
- `tests/ProductApi.IntegrationTests` – the full API against a real PostgreSQL container (Testcontainers). Needs Docker.

## Configuration

| Setting | Environment variable | Notes |
|---|---|---|
| Database connection | `ConnectionStrings__ProductDb` | Required. Only `appsettings.Development.json` contains a local default. |
| Version | `APP_VERSION` | Set by the pipeline to the commit SHA. |

## Pipeline

`.github/workflows/ci.yml` runs on every pull request and push to `main`:

1. Restore, build and run unit and integration tests.
2. Build the Docker image (multi-stage, non-root).
3. On `main` only: push the image to GitHub Container Registry as `ghcr.io/mas-devtools/<repo>:sha-<commit>` and `:latest`.
