# Module Plan 04: Gamification, Scoring Ledger, Badges & Notifications

**Owner:** Youssef (M4)  
**Entities:** `ScoreTransaction` (Append-Only), `Badge`, `StudentBadge`, `Notification`  
**Dependencies:** Module 01 (`Student` identity), Module 02 (Task triggers), Module 03 (Attendance triggers)  
**Status:** Scheduled  

---

## 1. Module Overview & Scope
- **Scoring Engine:** Immutable points audit ledger (`ScoreTransaction`). Automated triggers from task completion (+50 pts) and event attendance (+100 pts).
- **Derived Metrics:** Level computation formula: `Level = FLOOR(SQRT(TotalPoints / 100)) + 1`.
- **Badges Engine:** Automated evaluation of unlocked badges upon points/events/tasks thresholds.
- **Notifications Inbox:** User in-app notifications dispatched for application status changes, task assignments, and badge unlocks.

---

## 2. API Endpoints Covered
- Scoring: `GET /api/v1/students/{id}/score`, `GET /api/v1/students/{id}/score-transactions`, `POST /api/v1/score-transactions`
- Badges: `GET/POST /api/v1/badges`, `PUT/DELETE /api/v1/badges/{id}`, `GET/POST /api/v1/students/{id}/badges`
- Notifications: `GET /api/v1/notifications`, `PATCH /api/v1/notifications/{id}`, `PATCH /api/v1/notifications/mark-all-read`, `DELETE /api/v1/notifications/{id}`
