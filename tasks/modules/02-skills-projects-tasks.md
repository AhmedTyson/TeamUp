# Module Plan 02: Skills, Projects, Teams & Tasks

**Owner:** Ossama (M2)  
**Entities:** `Skill`, `StudentSkill`, `Project`, `ProjectRequiredSkill`, `ProjectMember`, `ProjectApplication`, `ProjectTask`  
**Dependencies:** Module 01 (`Student` identity & JWT auth)  
**Status:** In Progress (Projects, Applications & Tasks Verified & Done; Skills Pending)  

---

## 1. Module Overview & Scope
- **Skills Catalog:** Master technical and soft skills repository, student claimed skills with proficiency ratings (`Beginner`, `Intermediate`, `Advanced`). *(Pending: T16)*
- **Projects & Team Formation:** Project creation (creator is Lead), recruiting status, required skills matching. *(Verified & Done: T17)*
- **Applications:** Candidate application submission, acceptance (creates `ProjectMember`), rejection, and re-application via surrogate key with filtered index. *(Verified & Done: T18)*
- **Task Management:** Sprint task board within projects (`Todo`, `InProgress`, `InReview`, `Done`), assignment to confirmed members, member removal unassigns tasks. *(Verified & Done: T19)*

---

## 2. API Endpoints Covered
- Skills: `GET/POST /api/v1/skills`, `GET/PUT/DELETE /api/v1/skills/{id}`, `GET/PUT/DELETE /api/v1/students/{id}/skills/{skillId}`
- Projects: `GET/POST /api/v1/projects`, `GET/PATCH/DELETE /api/v1/projects/{id}`
- Required Skills: `GET/PUT/DELETE /api/v1/projects/{id}/required-skills/{skillId}`
- Members: `GET /api/v1/projects/{id}/members`, `PATCH/DELETE /api/v1/projects/{id}/members/{studentId}`
- Applications: `GET/POST /api/v1/projects/{id}/applications`, `PATCH /api/v1/projects/{id}/applications/{appId}`, `DELETE .../{appId}`
- Tasks: `GET/POST /api/v1/projects/{id}/tasks`, `GET/PATCH/DELETE /api/v1/tasks/{id}`, `PUT/DELETE /api/v1/tasks/{id}/assignee`
