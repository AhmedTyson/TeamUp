### Pull Request Overview
<!-- Summarize the changes made in this PR and state which task (T[N]) it implements. -->

**Task Reference:** `T[ ]`  
**Team Member:** M1 / M2 / M3 / M4  
**Module:** Identity / Skills & Projects / Organizations & Events / Gamification / Infrastructure  

---

### Acceptance Criteria Checklist
- [ ] Backend builds with zero warnings (`dotnet build backend/TeamUp.sln`)
- [ ] Backend unit & integration tests pass (`dotnet test backend/TeamUp.sln`)
- [ ] Frontend tests & build pass (`cd frontend && npm run test && npm run build`)
- [ ] Soft-delete rule adhered to (no physical hard deletes, `DeletedAt` set)
- [ ] Foreign keys use `ON DELETE NO ACTION ON UPDATE NO ACTION`
- [ ] Filtered unique indexes used if unique constraint can collide after soft-delete
- [ ] No MVC views or `.cshtml` added outside `backend/src/TeamUp.Infrastructure/Templates/` (emails/reports only)
- [ ] PR touches only files owned by this module or agreed cross-cutting extensions

---

### Testing Evidence
<!-- Paste test command output or describe verification steps. -->
```
dotnet test backend/TeamUp.sln
cd frontend && npm run test && npm run build
```
