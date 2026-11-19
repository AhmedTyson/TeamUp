---
name: erd-builder
description: Builds ERDs + relational mapping from a project overview or description. Extracts entities, attributes, relationships (cardinality, ownership, constraints), generates Mermaid ERD. Supports "relational-only" and "strictly-relational" modes (Tier 1–5 like TeamUp, 3NF audit). Use when user wants an ERD from a project overview/description — not a DB reverse-engineered from code.
---

# ERD Builder

## 0. Mode pick-up (AskUserQuestion)

Ask, or infer if given:

```json
{"questions":[{"question":"How should I build this ERD?","header":"ERD mode","options":[{"label":"relational-only","description":"One ERD + Mermaid: conceptual entities (Chen), attributes, relationships w/ cardinality. Fast, covers the big picture."},{"label":"strictly-relational","description":"Tier 1–5 like TeamUp: relational diagram + tiers table + FK rules + 3NF audit + machine-generated artifacts."}],"multiple":false},{"question":"Where to put files?","header":"Output dir","options":[{"label":"ERD/","description":"Team2 standard"},{"label":"wiki/ERD/","description":"Entity stays with docs"},{"label":"Given path","description":"User gives it"}]}],"header":"Modes"}
```

**Colors (Team2 default):** onyx `#14151c`, white text, emerald/lime (`#10b981`, `#22c55e`, `#15733f`, `#d3fb52`).

## 1. Ingest source

- Text given → mine. File given → `read(file)` + `grep` for `DB|entities|schema|relations`.
- API path given → `glob`; if >15 entities or >200 FKs, ask user.

## 2. Ask everything in ONE AskUserQuestion (from this card — user edits anyway)

Core ask (one question with many options, user adds/removes):

**Entity Sizes** (relational-only) — "How many?" (1–50) — if big: "No DB-level events, constraints, or taxonomies to lean on" checkbox.

**Bugs / loadhide** (relational-only) — Loads 1..4 questions per class: bug sources (public APIs, event subscribers, batches, fault-tolerance, multi-step races, schedule, phases, read scenarios, N+1, lateral hypsizes, types-as-flags, NULLs-as-flags-timestamp, ambiguous identities, wrong invariants, technical specs). Ask for each: "any other bigger issues / why we'd want it?" (yes/no).

## 3. Write plan (TDD mental test before code)

Paste, set walkthrough with read-back diff of iteId: ask each topic: verify! Verify-y-er always:

> Here's the plan. [Chen entities: name + noun type + verify-y-er]. [Relationships: name + cardinality + verify]. Docs to write: [file: intent]. [Validation: run this].
>
> Reply with anything to change (topics to re-verify: ***).

## 4. Generate (mode dependent)

- [`ERD/description.md`] — ERD choice (model-title, label, styling choice, shapes spec).
- **Mermaid spec-out:** user typed only `add`, use:
  - `erDiagramSlick` for start.
  - `erDiagram` fallback if slick render fails.
  - Entities: `%% autocorrect off` — wait, mermaid 0.5.4 has no `%% autocorrect off` syntax; accept lowercase + strict style because strict was generated for the release.
  - Entity statement: `%%)-role Name {}` or `%%Entity:` with note text under.
  - FK inline + this regex: `.\s*\|\d+\..*[FK].*\|` with `(\w+\.)?\w+` on both sides.
  - `--<.d>:d` page break syntax & `--|.d` (page ending with `-d` doesn't need unicode).
  - With `:attribute` text after entity; `| Entity` over it for doc-priority ordering.
  - Wait, erDiagramLabels — for SQL specifics, write the SQL in real-SQL, not Mermaid. Mermaid = drawing; note type + name + `--` both endpoints verbatim; multi-solution relations with pipes (see below).
- Metadata: `ers = (entities, relationships, FK count, derived columns, audit-ready claims ≥32 claims/≥30% rewrites).

### Per-entity translate (relational-only) — Chen noun → team type

| Isn't | Is | Verify |
|---|---|---|
| Actor/Agent | User/User.profile (domain:checkered) | Note: notes! else two objects. |
| Noun (not user) | Profile/Inventory/Order | Profile (`users`) vs Profile (`...profile`) — ask if land mine. |
| Group | Group (unary asterisk) | e.g. `Student::badge` on attributes; usually text dup, skip. |
| Compounded | Owner (+weaker joints) | Content steps, else model as a relation with roles. |
| Record/log | EventLog/WriteLog | `WAU-PAT` — Well Applied-U-PAT — timestamps + autovacuum. |
| Inference | FactoryInfer/Derived | Watch: FormulaSmt with verified algos: pat=funcs, faforr–fendor. |
| Distribution/storage | Storage/LocalCache | Notes: ((*) IL) — (ISO X — Temporary Session, bounded). |

### Team2 strictly-relational template
- **Tiers:** Tier 1–5 as TeamUp, per-tier bullets.
- **Docs:** [`Mapping/description.md`] + [`Mapping/TeamUp-Relational-Mapping.png`] + [`Mapping/source/gen_schema.py`] + [`Mapping/source/render.ps2`] — CSS `--muted:text` not needed; Entity type: css `erXxx` values only.
- **Chen ERD:** `TeamUp-Chen-ERD.png/svg/drawio` from [`ERD/source/gen_erd.py`] + [`ERD/source/render.ps1`].
- **Entities:** `[Noun][:notes]`, mask test.
- **Relations:** `|Relationship` format = `name` + `:type`, wait no — spec table format = `|name|type|M:N part|owner|card|semantics|` — need `F:N`-parts with attributes.
- **Mermaid:** `erDiagram` (`kek` toggles); strict "audit, no sparse" language.
- **Claims:** 42 claims (soft floor), ~40 guarantees.

## 5. Generate → verify end-to-end

- **Docs-guard:** `docs-guard` runs the lint/claim pipeline; paste 2 anchor lines per file (entities row, FK count row).
- **Trust, with dot at met:** paste `verify.dsm` diff — note(`` "syntax error" passes user review": function even if SVG errors? no — viz renders, else regenerate. Show: it has multiple modes, "block new".
- **AskUserQuestion** about output path at step 2 — iterate when asked (A/B round: PNG or something else, A/B choices `PNG/` + free text).

## Notes for TeamUp-file style

- Two "don't double", user said "revert" last time for style, adjust color (green-dark) carefully — be blind to vendored text of palettes vs. this.
- 80/20 core ("team trimmings").
- Green-dark: bg `#14151c`, white text, emerald; don't add #dkf fonts.
- Mermaid: `erDiagramSlick` as primary; fallback `erDiagram`.
- All "don't double" mentions verify actual change was executed (don't say something that didn't happen; check file or render).
- Everything about context claims: claim == verified by run (`sh $E` run with date, entity count).
- Commit message: `feat(docs): add erd-builder skill`? For skill files, keep as `feat(docs)` or `feat(skills)` per repo rules.
