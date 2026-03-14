# FRONTEND_STANDARDS_AI.md

## Purpose
This document defines mandatory frontend rules for AI-assisted development.

If a rule is missing or unclear, the AI must ask for clarification.

---

## Core Rules
- Language: TypeScript
- Single UI framework per project
- Component-based architecture only
- Feature-based folder structure

---

## Architecture
- No business logic in UI components
- API access only through services
- No circular dependencies

---

## Coding
- Explicit and readable code
- One component per file
- No magic values
- camelCase for variables, PascalCase for components

---

## State Management
- Local state by default
- Global state only when required
- Unidirectional data flow

---

## Styling
- Scoped styles
- No inline styles unless approved
- In protected frontend paths, do not introduce new hardcoded presentation literals (hex colors, fixed inline geometry values) unless an explicit allowlist entry and justification are added.

---

## Testing
- Unit tests for logic
- Component tests for UI
- E2E tests for critical flows

---

## Performance
- Avoid unnecessary re-renders
- Lazy load non-critical features

---

## Security
- Never trust input
- Never store secrets
- No auth logic in components

---

## Change Scope
- Modify only required files
- Do not refactor unrelated code

---

## Decision Rule
If unsure, stop and ask. Do not guess.
