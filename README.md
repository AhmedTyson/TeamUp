# TeamUp — Master Backend Architecture (C# / .NET 10 & ASP.NET Core)

> **Enterprise Backend Implementation for TeamUp Student Collaboration & Gamification Platform**  
> **Core Technology:** C# 14 / .NET 10.0 (ASP.NET Core Web API)  
> **Database:** Entity Framework Core + SQLite (Thread-safe WAL mode)  
> **Authentication:** JWT Bearer (Role-based & Resource-ownership guards)  
> **Assigned Scope:** Tasks T22 through T27 (24 Endpoints: #68–#72, #77–#95)  
> **Owner:** M3 — Ibrahim  

---

## 1. Quick Start Guide

### Prerequisites
- .NET 10.0 SDK (`dotnet --version`)

### Run the Backend Server
```bash
dotnet run --project src/TeamUp.Api
```
- **Base HTTP URL:** `http://localhost:5106`
- **Interactive Swagger UI:** 👉 **[http://localhost:5106/swagger](http://localhost:5106/swagger)**
- **Health Check:** `http://localhost:5106/health`

### Run Automated Tests (xUnit)
```bash
dotnet test TeamUp.slnx
```

---

## 2. Clean Architecture Folder Structure

```text
teamup-backend-architect-(t22-t27)/
├── src/
│   └── TeamUp.Api/                     # ASP.NET Core Web API Project
│       ├── Auth/                       # JWT Generation, Token Validation & Role Guards
│       │   └── JwtTokenService.cs
│       ├── Controllers/                # All 24 API Endpoints (#68–#72, #77–#95)
│       │   ├── EventsController.cs          # [T22] Endpoints #68–#72
│       │   ├── RegistrationsController.cs   # [T23] Endpoints #77–#79
│       │   ├── AttendanceController.cs      # [T24] Endpoints #80–#82
│       │   ├── GamificationController.cs    # [T25] Endpoints #83–#85
│       │   ├── BadgesController.cs          # [T26] Endpoints #86–#91
│       │   └── NotificationsController.cs   # [T27] Endpoints #92–#95
│       ├── Data/                       # EF Core DbContext & Domain Entities
│       │   ├── Entities/
│       │   │   └── Entities.cs         # Relational entities mapped to Chen ERD
│       │   └── TeamUpDbContext.cs      # ModelBuilder constraints & initial seed data
│       ├── Models/
│       │   └── DTOs/                   # Strongly-typed Records with Data Annotations
│       │       ├── EventDtos.cs
│       │       ├── RegistrationDtos.cs
│       │       ├── AttendanceDtos.cs
│       │       ├── GamificationDtos.cs
│       │       ├── BadgeDtos.cs
│       │       └── NotificationDtos.cs
│       ├── Services/                   # Business Logic & Cross-Module Invariants
│       │   ├── EventService.cs
│       │   ├── RegistrationService.cs
│       │   ├── AttendanceService.cs
│       │   ├── GamificationService.cs
│       │   ├── BadgeService.cs
│       │   └── NotificationService.cs
│       └── Program.cs                  # Dependency Injection, Middleware & Swagger Auth
├── tests/
│   └── TeamUp.Tests/                   # xUnit Test Suite
│       ├── CustomWebApplicationFactory.cs
│       ├── GamificationMathTests.cs    # Level formulas & mathematical boundary tests
│       └── EndpointIntegrationTests.cs # Full cross-module integration tests
├── TeamUp.slnx                         # Solution file
└── README.md
```

---

## 3. Implemented Endpoints Reference Matrix (#68–#72, #77–#95)

| # | Task | Method | Route | Description | Authorization |
|---|---|---|---|---|---|
| **68** | **T22** | `GET` | `/api/v1/events` | Browse upcoming workshops, hackathons, and competitions | Auth |
| **69** | **T22** | `POST` | `/api/v1/events` | Create a new event hosted by an organization | OrgAdmin / PlatformAdmin |
| **70** | **T22** | `GET` | `/api/v1/events/{id}` | Event agenda, location, dates, and registration count | Auth |
| **71** | **T22** | `PATCH` | `/api/v1/events/{id}` | Update event venue, dates, or registration capacity | OrgAdmin |
| **72** | **T22** | `DELETE` | `/api/v1/events/{id}` | Soft-delete event and cascade soft-delete to sessions | OrgAdmin |
| **77** | **T23** | `GET` | `/api/v1/events/{id}/registrations` | List applications/registrations for an event | OrgAdmin / PlatformAdmin |
| **78** | **T23** | `POST` | `/api/v1/events/{id}/registrations` | Submit registration for an event (Capacity check & Waitlist) | Auth (Student) |
| **79** | **T23** | `DELETE` | `/api/v1/events/{id}/registrations` | Cancel registration (Attendance lock check & FIFO promotion) | Applicant / OrgAdmin |
| **80** | **T24** | `GET` | `/api/v1/sessions/{id}/attendance` | List attendance for an event session | OrgAdmin / PlatformAdmin |
| **81** | **T24** | `POST` | `/api/v1/sessions/{id}/attendance` | Verify student attendance & award +100 ledger points | OrgAdmin / PlatformAdmin |
| **82** | **T24** | `DELETE` | `/api/v1/sessions/{id}/attendance/{studentId}` | Revoke attendance with compensating -100 ledger record | OrgAdmin / PlatformAdmin |
| **83** | **T25** | `GET` | `/api/v1/students/{id}/score` | Retrieve total points, level, and progress | Owner / PlatformAdmin |
| **84** | **T25** | `GET` | `/api/v1/students/{id}/score-transactions` | Retrieve score transaction history (Immutable audit ledger) | Owner / PlatformAdmin |
| **85** | **T25** | `POST` | `/api/v1/score-transactions` | Create manual bonus or adjustment | PlatformAdmin |
| **86** | **T26** | `GET` | `/api/v1/badges` | List active badge catalog | Auth |
| **87** | **T26** | `POST` | `/api/v1/badges` | Create a badge | PlatformAdmin |
| **88** | **T26** | `PUT` | `/api/v1/badges/{id}` | Update badge | PlatformAdmin |
| **89** | **T26** | `DELETE` | `/api/v1/badges/{id}` | Soft-delete badge (Preserves historical awards) | PlatformAdmin |
| **90** | **T26** | `GET` | `/api/v1/students/{id}/badges` | List student badges | Owner / PlatformAdmin |
| **91** | **T26** | `POST` | `/api/v1/students/{id}/badges/{badgeId}` | Manually award badge | PlatformAdmin |
| **92** | **T27** | `GET` | `/api/v1/notifications` | Retrieve authenticated student's notifications | Auth (Student) |
| **93** | **T27** | `PATCH` | `/api/v1/notifications/{id}` | Update notification read status | Owner (Student) |
| **94** | **T27** | `PATCH` | `/api/v1/notifications/mark-all-read` | Mark all own notifications as read | Auth (Student) |
| **95** | **T27** | `DELETE` | `/api/v1/notifications/{id}` | Soft-delete notification | Owner (Student) |

---

## 4. Key Business Rules & Architectural Highlights

1. **Immutable Audit Ledger (T24 & T25):**
   - `ScoreTransaction` rows are append-only. No edits or deletions are permitted.
   - Attendance revocation appends a compensating `-100` points entry. Repeated revocations are prevented (idempotent).
2. **Deterministic Gamification Math (T25):**
   - $\text{Level} = \lfloor\sqrt{\frac{\max(\text{TotalPoints}, 0)}{100}}\rfloor + 1$
   - $\text{CurrentLevelStart} = (\text{Level} - 1)^2 \times 100$
   - $\text{NextLevelThreshold} = \text{Level}^2 \times 100$
   - Verified against boundary values: $2540 \rightarrow \text{Level } 6, 3.64\% \text{ progress}$.
3. **Capacity Floor & Cascading Soft-Delete (T22):**
   - Event capacity cannot be updated below the number of confirmed registrations.
   - Soft-deleting an event automatically cascades `IsDeleted = true` to its child sessions.
4. **Attendance Lock Guard & Waitlist Auto-Promotion (T23):**
   - If a student has recorded attendance, registration cancellation is blocked (`409 Conflict`).
   - Cancelling a confirmed registration promotes the oldest waitlisted student (FIFO) to confirmed and sends a notification.

