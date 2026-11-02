# TeamUp

Student collaboration platform: team formation, student profiles and scoring, student activities, events and workshops, gamification.

**Stack:** SQL Server, ASP.NET Core Web API, EF Core, JWT auth.

## Database design (TeamUpDB)

| Deliverable | Path |
|---|---|
| Chen ERD (13 entities, 19 relationships) | [ERD/TeamUp-Chen-ERD.png](ERD/TeamUp-Chen-ERD.png) |
| ERD description | [ERD/description.md](ERD/description.md) |
| Relational mapping (23 tables, row-of-cells) | [Mapping/TeamUp-Relational-Mapping.png](Mapping/TeamUp-Relational-Mapping.png) |
| Mapping description and 3NF check | [Mapping/description.md](Mapping/description.md) |

Diagram generators: ERD/source/gen_erd.py, Mapping/source/gen_schema.py (PNG export via ERD/source/render.ps1, Edge headless).

## Modules

Universities and Faculties, Users and Profiles, Skills, Projects and Teams, Student Activities, Events, Scoring, Achievements, Notifications.
