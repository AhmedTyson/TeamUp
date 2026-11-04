# Phase 03: Generative UI Dynamic Form Hydration

**Pillar:** I — Conversational Onboarding & Intent Capture  
**Module:** `TeamUp.AiGateway.GenerativeUi`  
**Target:** Real-Time Interactive Component Synthesis from Semantic Stream  

---

## 1. Problem & Architectural Challenge
Pure text-based conversational interfaces suffer from severe ergonomic flaws:
1. **Low Information Density:** Reading conversational transcripts takes 5x longer than scanning a visual card.
2. **Editing Friction:** If the AI misspells a name or gets an academic year wrong, the user has to type another conversational prompt (*"No, change my year to 3"*) instead of simply clicking a number.
3. **Loss of Visual Feedback:** Users feel anxiety when they cannot see what the computer is actually preparing to save.

**Solution:** **Generative UI**. The chat interface renders a live visual **Student Card Preview** that updates in real time as the conversation streams.

---

## 2. Research & Human-Computer Interaction Grounding

### The Principle of Dual-Coding (Paivio, 1986 / Norman, 2013)
*Citation: Don Norman, "The Design of Everyday Things"; Budiu (2024), "Generative AI UI Patterns".*

Human users process visual-spatial cards 60,000x faster than textual dialogue. Generative UI achieves the optimal balance:
- **Natural Language as the INTAKE (Input):** User speaks or types fluidly without navigating forms.
- **Direct Manipulation as the SURFACE (Output):** The interface renders structured UI components with inline inputs, toggles, and dropdowns.

```
Conversational Stream ──────────► JsonSchema State ──────────► Reactive UI Card
 "Ahmed Tyson, 3rd year"           { first: "Ahmed", ... }      [ First Name: Ahmed  ]
                                                                [ Year: [ 1 | 2 | (3) | 4 ] ]
```

---

## 3. Component Architecture & Data Contracts

### Generative UI Component Payload (Server-to-Client Stream)
```json
{
  "componentType": "StudentProfileDraftCard",
  "fields": {
    "firstName": { "value": "Ahmed", "status": "validated", "editable": true },
    "lastName": { "value": "Tyson", "status": "validated", "editable": true },
    "email": { "value": "ahmed@cu.edu.eg", "status": "validated", "editable": true },
    "academicYear": { "value": 3, "status": "validated", "editable": true, "options": [1, 2, 3, 4, 5] },
    "university": { "value": "Cairo University", "id": 1, "status": "resolved" },
    "faculty": { "value": "Faculty of Computers and AI", "id": 1, "status": "resolved" },
    "interests": { "value": ["Artificial Intelligence", "Robotics"], "status": "editable_tags" }
  },
  "actions": [
    { "type": "submit", "label": "Confirm & Create Profile", "enabled": true },
    { "type": "cancel", "label": "Start Over", "enabled": true }
  ]
}
```

### Frontend State Hydration (TypeScript / React Contract)
```typescript
export interface FieldState<T> {
  value: T;
  status: 'empty' | 'inferred' | 'validated' | 'conflict';
  editable: boolean;
  validationMessage?: string;
}

export interface StudentDraftCardProps {
  fields: {
    firstName: FieldState<string>;
    lastName: FieldState<string>;
    email: FieldState<string>;
    academicYear: FieldState<number>;
    universityId: FieldState<number>;
    facultyId: FieldState<number>;
    interests: FieldState<string[]>;
  };
  onFieldChange: (field: string, newValue: any) => void;
  onConfirm: () => void;
}
```

---

## 4. Failure Modes & Mitigations
- **Desynchronization between Chat and Card:** The Card state is the single source of truth. If the user edits a field directly on the Card, the FSM state machine updates its slot memory immediately.
- **Rendering flicker during token streaming:** Use debounced JSON patch streaming (JSON-RPC or Server-Sent Events with RFC 6902 JSON Patch) rather than re-rendering the whole component on every token.
