### Pull Request Overview
<!-- Summarize the changes made in this PR and state which task (T[N]) it implements. -->

**Task Reference:** `T[ ]`  
**Team Member:** M1 / M2 / M3 / M4  
**Module:** Identity / Skills & Projects / Organizations & Events / Gamification / Infrastructure  

---

### Acceptance Criteria Checklist
- [ ] Builds with zero warnings (`dotnet build TeamUp.sln`)
- [ ] Unit & integration tests pass (`dotnet test TeamUp.sln`)
- [ ] Soft-delete rule adhered to (no physical hard deletes, `IsDeleted` / `DeletedAt` set)
- [ ] Foreign keys use `ON DELETE NO ACTION ON UPDATE NO ACTION`
- [ ] Filtered unique indexes used if unique constraint can collide after soft-delete
- [ ] PR touches only files owned by this module or agreed cross-cutting extensions

---

### Testing Evidence
<!-- Paste test command output or describe verification steps. -->
```
dotnet test TeamUp.sln
```
