description: Standards and best practices for technical documentation in this project, including documentation structure, update processes, and language rules.
globs:

- "**/*.md"
alwaysApply: true

# Rules and Patterns for Documentation

## Documentation

When writing technical documentation such as:

- Data models
- Unit tests documentation
- README files
- API specifications
- Any other Markdown documentation

You MUST ALWAYS write in **English**, including:

- Documentation files
- Code comments
- Function, method, and field explanations

This rule applies both when:

- Creating new documentation
- Updating existing documentation
- Writing documentation within the codebase (comments, explanations, annotations)

Before making any commit or git push, or when explicitly asked to document a change, you MUST always review which technical documentation needs to be updated.

---

## Documentation Updates

When updating documentation, you MUST follow this process:

1. Review all recent changes in the codebase.
2. Identify which documentation files must be updated based on those changes.

   Clear examples include:
   - For **data model changes**:  
     Update the data model definition section in `data-model.md`.
   - For **API changes**:  
     Update `api-spec.yml`.
   - For **library, database, migration, or installation process changes**:  
     Update `*-standards.md` or the relevant installation or configuration documentation.

3. Update each affected documentation file **in English**, maintaining consistency with existing documentation.
4. Ensure all documentation is properly formatted and follows the established structure.
5. Verify that all changes are accurately reflected in the documentation.
6. Report which files were updated and summarize what changes were made.

---

## Documentation Quality Rules

- Documentation must be:
  - Clear
  - Explicit
  - Consistent
  - Up to date
- Avoid ambiguous language.
- Avoid assumptions about reader knowledge.
- Prefer explicit explanations over implicit behavior.
- Keep documentation aligned with the actual implementation.

---

## OpenSpec Change Documentation

When working within an OpenSpec change (`openspec/changes/<change-name>/`), the change documentation files (`proposal.md`, `design.md`, `tasks.md`) serve as the **complete record** of what was implemented, not just what was initially planned.

### Rule: Document All Significant Modifications

Any modification made **during or after implementation** must be evaluated for documentation:

**MUST be documented:**
- Bug fixes discovered during implementation
- UX improvements or behavior refinements
- New architectural decisions or patterns introduced
- Additional features or capabilities added
- Scope changes or deviations from original plan
- New tests or validation approaches
- Navigation or flow changes

**Documentation locations:**
- `proposal.md`: Update capabilities list, modified files, and feature scope
- `design.md`: Add decision sections for architectural choices, alternatives, and rationale
- `tasks.md`: Add new task sections (e.g., 9.6, 11.7) with subtasks for new work

**Examples from real changes:**
- Timestamp parameter for historical dates → Documented in proposal (new capability), design (Decision 9), tasks (Section 9.5)
- Dynamic UI text for date context → Documented in design (Decision 10), tasks (Section 9.6)
- Tab navigation reset behavior → Documented in design (Decision 11), tasks (Task 11.7)

### Process

1. After implementing any modification (fix, improvement, new feature):
   - Evaluate: "Does this change the behavior, architecture, or scope?"
   - If YES → identify which change files need updates
2. Update `proposal.md` if capabilities or scope changed
3. Add decision sections to `design.md` if architectural choices were made
4. Add task sections to `tasks.md` for new implementation work
5. Use clear, structured format matching existing sections
6. Mark tasks as completed `[x]` with validation notes where applicable

This ensures change documentation is an accurate historical record of the evolution of the feature during development.

---

## AI-Specific Rules

- The AI must not assume documentation is up to date.
- The AI must proactively identify missing or outdated documentation.
- The AI must not skip documentation updates when code changes affect behavior, APIs, configuration, or data models.
- If documentation requirements are unclear, the AI must ask for clarification before proceeding.

Documentation is considered part of the deliverable, not an optional task.
