![Language](https://img.shields.io/badge/language-Csharp-green)
![Platform](https://img.shields.io/badge/platform-Linux%20%7C%20macOS%20%7C%20Windows-blue)

# 🚀 Task Management API as a Backend Showcase

Showcase project of my C# backend development expertise using the latest tech stack and best practices.

## 📋 Project Idea: Multi-Tenant Task Management API

Core Concept: A task management system where multiple organizations (tenants) can manage their teams' tasks.
Users authenticate via Keycloak, create tasks, assign them to teammates, and receive notifications when tasks are updated.

## 🛠️ Tech Stack

🔵 **.NET 10** — Latest LTS framework  
🔐 **Keycloak** — Enterprise authentication patterns  
⚡ **FusionCache** — Distributed caching  
📨 **MassTransit** — Async messaging  
🏗️ **MediatR** — Clean architecture  
✅ **FluentValidation** — Validation patterns  
📝 **Serilog** — Structured logging  
🗄️ **EF Core** — ORM best practices  
🚀 **Dapper** — Performance-critical queries  
🛡️ **Polly** — Resilience patterns  
🧪 **xUnit + Testcontainers** — Testing rigor  
📖 **Swagger** — API documentation  
📊 **GraphQL** — Flexible data fetching  
💚 **HealthChecks** — Production readiness

## ✨ Feature Set

| Feature | Tech Justification |
|---------|-------------------|
| 👤 User authentication & SSO | Keycloak integration with .NET |
| ✏️ Create/read/update/delete tasks | EF Core + MediatR for clean architecture |
| 🔍 Task filtering & pagination | Dapper for optimized list queries |
| 👥 Assign tasks to team members | MassTransit event for notifications |
| 🔔 Task status change notifications | Async messaging with MassTransit |
| 💾 Caching task lists | FusionCache for distributed cache |
| ✔️ Input validation | FluentValidation on commands |
| 📊 Structured logging | Serilog with correlation IDs |
| 🔄 Retry logic on failures | Polly resilience policies |
| 📚 API documentation | Swagger + GraphQL schema |
| 💓 Health checks | Database, cache, message broker health |
| 🧬 Comprehensive tests | xUnit + Testcontainers |

## 🏛️ Architecture Sketch

```
API Layer (REST + GraphQL endpoints)
    ↓
MediatR Handlers (Commands/Queries)
    ↓
Service Layer (Business logic)
    ↓
Data Layer (EF Core + Dapper)
    ↓
Database (PostgreSQL)

Side Channels:
- MassTransit → RabbitMQ/Azure Service Bus
- FusionCache → Distributed cache
- Keycloak → External auth
- Polly → Resilience wrapper
```

## 📦 Core Entities

- **Organization** (tenant)
- **User** (linked to KeyCloak ID)
- **Task** (title, description, status, assigned user, due date)
- **TaskComment** (optional, adds depth without complexity)

## 🔐 KeyCloak Integration and Multi-tenancy

The application does not create users, KeyCloak does. So when user logs in with JWT token for the first time, the user gets created in the Database.

On the other hand, tenants themselves do not exist in KeyCloak, they are created in the app and users are linked to one or multiple tenants - this means a single user can connect to multiple tenants, so the API request headers should include tenant id as well, for example:

```text
GET /api/tasks
Authorization: Bearer <token>
X-Tenant-Id: 7d19d6c3-7c5d-4d90-98d2-81c9b3e7e99f
```
