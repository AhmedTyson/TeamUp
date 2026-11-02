# Module Plan 03: Student Activities, Organizations, Events & Attendance

**Owner:** Ibrahim (M3)  
**Entities:** `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventRegistration`, `EventSession`, `SessionAttendance`  
**Dependencies:** Module 01 (`Student` identity & JWT auth)  
**Status:** Scheduled  

---

## 1. Module Overview & Scope
- **Organizations:** Student clubs, societies (IEEE, GDG, Enactus), leadership roles, member governance.
- **Recruitment Applications:** Join requests during recruitment seasons, review by `OrgAdmin`.
- **Events & Workshops:** Event scheduling, location, multi-session timetables (`EventSession`).
- **Registrations & Attendance:** Registration capacity verification, session check-in (`QRCode`, `RFID`, `ManualByOrganizer`).

---

## 2. API Endpoints Covered
- Organizations: `GET/POST /api/v1/organizations`, `GET/PATCH/DELETE /api/v1/organizations/{id}`, `GET/PATCH/DELETE /api/v1/organizations/{id}/members/{studentId}`
- Recruitment Apps: `GET/POST /api/v1/organizations/{id}/applications`, `PATCH/DELETE .../{appId}`
- Events & Sessions: `GET/POST /api/v1/events`, `GET/PATCH/DELETE /api/v1/events/{id}`, `GET/POST /api/v1/events/{id}/sessions`, `PUT/DELETE /api/v1/sessions/{id}`
- Registration & Attendance: `GET/POST/DELETE /api/v1/events/{id}/registrations`, `GET/POST/DELETE /api/v1/sessions/{id}/attendance`
