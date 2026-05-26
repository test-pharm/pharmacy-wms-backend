<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Render-46E3B7?logo=render&logoColor=black" alt="Render">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
</p>

# Pharmacy WMS — Backend

RESTful API for a Pharmacy Warehouse Management System built with ASP.NET Core 8.0 and Entity Framework Core. Auto-detects PostgreSQL (production) or SQLite (development).

---

## Features

- **JWT Authentication** — Login, token validation, role-based access
- **Product Management** — Full CRUD with stock batch tracking & expiry dates
- **Order Processing** — Material dispatch, stock additions, refunds, invoice tracking
- **Stock Control** — Low-stock alerts, expiry change requests with supervisor approval
- **Audit Logging** — Auto-cleanup service, detailed action history
- **Threshold Settings** — Configurable low-stock & expiring-soon thresholds
- **Reset Codes** — Email-based password recovery service

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8.0 |
| ORM | Entity Framework Core 8.0 |
| Database (Production) | PostgreSQL 16 via Npgsql |
| Database (Local) | SQLite |
| Auth | JWT Bearer (System.IdentityModel.Tokens.Jwt) |
| Password Hashing | PBKDF2-SHA256 with salt |
| Metrics | Prometheus (`/metrics` endpoint) |
| Containerization | Docker / Docker Compose |
| CI/CD | GitHub Actions → GHCR → Render |

---

## Database Schema

| Table | Purpose |
|-------|---------|
| `Users` | Authentication & role management |
| `Products` | Material inventory with categories |
| `StockBatches` | Per-batch expiry & quantity tracking |
| `Orders` | Material, dispatch, edit & refund orders |
| `AuditLogs` | Action history with auto-cleanup |
| `ExpiryChangeRequests` | Supervisor approval workflow |
| `ThresholdSettings` | Configurable alert thresholds |
| `Notifications` | User notifications |

---

## Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for PostgreSQL + monitoring)

### Run with SQLite (local)
```bash
git clone https://github.com/test-pharm/pharmacy-wms-backend.git
cd pharmacy-wms-backend
dotnet run
```

> The API starts on `http://localhost:10000` with an auto-created SQLite database.

### Run with Docker Compose (PostgreSQL + monitoring)
```bash
docker compose up -d
```

This starts:
| Service | URL | Purpose |
|---------|-----|---------|
| Backend API | `http://localhost:10000` | REST API |
| PostgreSQL | `localhost:5432` | Database |
| Prometheus | `http://localhost:9090` | Metrics collection |
| Grafana | `http://localhost:3000` | Dashboards (admin/admin) |

---

## API Endpoints

### Auth
| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/Auth/login` | Authenticate user |
| `POST` | `/api/Auth/register` | Create new user |
| `POST` | `/api/Auth/forgot-password` | Send reset code |
| `POST` | `/api/Auth/reset-password` | Reset with code |

### Products
| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/Products` | List all products |
| `GET` | `/api/Products/{id}` | Get product details |
| `POST` | `/api/Products` | Create product |
| `PUT` | `/api/Products/{id}` | Update product |
| `DELETE` | `/api/Products/{id}` | Delete product |

### Orders
| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/Orders` | List all orders |
| `POST` | `/api/Orders` | Create order |
| `GET` | `/api/Orders/invoices` | Get invoice groups |

### More
| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/version` | App version info |
| `GET` | `/metrics` | Prometheus metrics |
| `GET` | `/api/Dashboard` | Dashboard KPIs |
| `GET` | `/api/AuditLog` | Audit trail |
| `GET` | `/api/Approvals` | Pending expiry changes |
| `GET` | `/api/Settings` | Threshold settings |

---

## Deployment (Render)

1. Push to `main` → GitHub Actions builds & pushes Docker image to GHCR
2. Render pulls the image from `ghcr.io/test-pharm/pharmacy-wms-backend:latest`
3. Set environment variable `DATABASE_URL` in ADO.NET format:

```
Host=your-host;Port=5432;Database=your-db;Username=your-user;Password=your-pass;SSL Mode=Require;Trust Server Certificate=true
```

Live at: [https://pharmacy-wms-backend.onrender.com](https://pharmacy-wms-backend.onrender.com)

---

## Monitoring

Grafana dashboard with Prometheus data source is auto-provisioned in Docker Compose:

<p align="center">
  <code>docker compose up -d</code> → <code>http://localhost:3000</code> → "Pharmacy WMS"
</p>

**Available metrics:**
- Request duration & rate
- Active connections
- Memory & CPU usage
- Error rate (4xx / 5xx)
- Database connection pool
- Per-route request distribution
