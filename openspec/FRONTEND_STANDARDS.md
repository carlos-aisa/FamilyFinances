# Frontend Development Standards

## 1. Purpose and Scope
This document defines mandatory frontend development standards. It must be followed by all human developers and AI assistants. If something is not explicitly defined, clarification is required before implementation.

---

## 2. Technology Stack
### 2.1 Core Technologies
- Language: TypeScript
- Package manager: npm or yarn
- Build system: framework default (no custom webpack unless approved)

### 2.2 UI Framework
- Use a single UI framework per project.
- No mixing of component libraries.

### 2.3 State Management and Data Flow
- Unidirectional data flow.
- State must be explicit and centralized when shared.

### 2.4 Testing Frameworks
- Unit & component testing framework defined by the project.
- E2E testing with Cypress or equivalent.

### 2.5 Development Tools
- Linter and formatter are mandatory.
- EditorConfig must be respected.

---

## 3. Project Structure
### 3.1 Folder and Module Organization
- Feature-based structure preferred.
- Shared code must live in shared/core modules.

### 3.2 Feature vs Layer Organization
- Features encapsulate UI, logic, and services.

### 3.3 Shared and Core Modules
- Core: app-wide services and configuration.
- Shared: reusable UI components and utilities.

---

## 4. Architectural Guidelines
### 4.1 Architectural Style
- Component-based architecture.
- Clear separation of concerns.

### 4.2 Component Architecture
- Components must be small and focused.
- No business logic in UI components.

### 4.3 Service Layer Architecture
- API calls only via services.
- Services must be framework-agnostic when possible.

### 4.4 Dependency and Import Rules
- No circular dependencies.
- Import from public APIs only.

---

## 5. Coding Standards
### 5.1 Language and Naming Conventions
- Components: PascalCase
- Files: kebab-case
- Variables and functions: camelCase

### 5.2 Component Conventions
- One component per file.
- Inputs and outputs must be explicit.

### 5.3 State Management Rules
- Local state for local concerns.
- Global state only when required.

### 5.4 Styling and CSS Strategy
- Scoped styles preferred.
- No inline styles unless justified.
- In protected frontend paths (`src/FamilyFinances.Web/Components/**/*.razor`, `src/FamilyFinances.Web/wwwroot/css/*.css`, `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`), new hardcoded presentation literals (hex colors, fixed inline geometry values) are forbidden unless explicitly allowlisted and documented.

---

## 6. UI / UX Standards
### 6.1 Design Consistency
- Follow design system strictly.

### 6.2 Bootstrap or UI Library Integration
- No direct DOM manipulation.

### 6.3 Form Handling
- Use reactive forms or framework equivalent.
- Validation must be explicit.

### 6.4 Navigation Patterns
- Centralized routing configuration.

### 6.5 Accessibility (a11y)
- Semantic HTML.
- Keyboard navigation supported.

---

## 7. Testing Standards
### 7.1 Unit Testing
- Test pure logic and services.

### 7.2 Component Testing
- Test rendering and interaction.

### 7.3 End-to-End Testing
- Test critical user flows.

### 7.4 Test Organization and Naming
- Tests mirror source structure.

---

## 8. Performance Guidelines
### 8.1 Rendering and Change Detection
- Avoid unnecessary re-renders.

### 8.2 State and Memory Management
- Clean up subscriptions and listeners.

### 8.3 Bundle Size and Lazy Loading
- Lazy load non-critical features.

---

## 9. Security Guidelines
### 9.1 Input Validation and Sanitization
- Never trust user input.

### 9.2 Authentication and Authorization Handling
- No auth logic in components.

### 9.3 Secure Storage and Sensitive Data
- Never store secrets in frontend.

---

## 10. Configuration Standards
### 10.1 Environment Configuration
- Use environment files only.

### 10.2 Build and Deployment Configuration
- Build must be reproducible.

### 10.3 Feature Flags
- Feature flags must be centralized.

---

## 11. Development Workflow
### 11.1 Git Workflow
- Feature branches only.

### 11.2 Code Review Rules
- No direct commits to main.

### 11.3 Code Quality Gates
- Tests and lint must pass.

### 11.4 AI Usage Rules
- AI must not invent patterns.
- AI must follow this document strictly.
