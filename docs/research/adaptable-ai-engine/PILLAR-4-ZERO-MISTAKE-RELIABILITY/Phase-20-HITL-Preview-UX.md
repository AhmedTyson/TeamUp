# Phase 20: Visual Diff & Human-in-the-Loop (HITL) Affirmation UX

**Pillar:** IV — The Zero-Mistake Deterministic Reliability Shield  
**Module:** `TeamUp.AiGateway.HitlUx`  
**Target:** Safe Autonomy via Tiered Risk Evaluation & Visual Confirmation UX  

---

## 1. Problem & Architectural Challenge
Full end-to-end unguided autonomy without human review creates catastrophic failure modes:
- An AI might infer the wrong date on a 500-seat workshop flyer and publish it to the whole campus.
- A user might accidentally delete an entire project team because an AI misconstrued a prompt (*"Clean up my team tasks"* ➔ AI interprets as *"Delete team"*).

**Requirement:** An autonomous UX framework that categorizes actions by risk level, automatically executing safe tasks while forcing **Visual Diff Previews** for impactful actions.

---

## 2. Research & The 3 Tiers of Autonomy

### Tiered Action Risk Matrix (Amodei et al., 2016 / NIST AI RMF 1.0)
*Citation: Amodei et al., "Concrete Problems in AI Safety"; NIST (2023), "Artificial Intelligence Risk Management Framework".*

```
┌──────────────────┬─────────────────────────────┬────────────────────────────────────┐
│ RISK TIER        │ EXAMPLES                    │ UX INTERACTION PARADIGM            │
├──────────────────┼─────────────────────────────┼────────────────────────────────────┤
│ 🟢 TIER 1: LOW   │ • Tagging student interests │ Autonomous Execution               │
│    Reversible,   │ • Updating profile bio      │ • Instant database commit          │
│    Self-scoped   │ • Creating draft backlog    │ • Displays toast with "Undo" (10s) │
├──────────────────┼─────────────────────────────┼────────────────────────────────────┤
│ 🟡 TIER 2: MEDIUM│ • Publishing a campus event │ Draft & Confirm Preview            │
│    Public facing,│ • Applying to a project     │ • Pre-filled Visual Diff card      │
│    Shared state  │ • Assigning sprint tasks    │ • Requires 1 explicit human click  │
├──────────────────┼─────────────────────────────┼────────────────────────────────────┤
│ 🔴 TIER 3: HIGH  │ • Soft-deleting account     │ Dual-Key / Explicit Affirmation   │
│    Destructive,  │ • Soft-deleting project     │ • Modal with typed confirmation    │
│    Irreversible  │ • Role promotion / admin    │ • Re-authenticates JWT credentials │
└──────────────────┴─────────────────────────────┴────────────────────────────────────┘
```

---

## 3. Visual Diff Preview Schema (JSON Contract)

```json
{
  "actionType": "PublishEvent",
  "riskTier": "Medium",
  "targetEntity": "Event",
  "summary": "Publish 'AI Hackathon 2026' with 2 sessions to Cairo University students",
  "diff": {
    "title": { "current": null, "proposed": "AI Hackathon 2026", "change": "added" },
    "capacity": { "current": null, "proposed": 100, "change": "added" },
    "sessionsCount": { "current": 0, "proposed": 2, "change": "added" },
    "startDate": { "current": null, "proposed": "2026-11-20T09:00:00Z", "change": "added" }
  },
  "affirmationRequired": true,
  "buttons": [
    { "action": "confirm", "label": "Publish Event", "style": "primary" },
    { "action": "edit", "label": "Edit Fields Manually", "style": "secondary" },
    { "action": "cancel", "label": "Discard Draft", "style": "danger" }
  ]
}
```

---

## 4. Failure Modes & Mitigations
- **User Fatigue ("Confirmation Blindness"):** If every trivial keystroke requires confirmation, users blindly click "Confirm". Only Tier 2 and Tier 3 actions require confirmation; Tier 1 actions execute instantly with an **Undo Snack-bar** (optimistic UI update).
- **Session Expiry on Preview:** If the user leaves the confirmation card open for 30 minutes before clicking "Confirm", the backend re-validates database state and JWT token freshness before final write.
