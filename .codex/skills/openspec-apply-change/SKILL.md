---
name: openspec-apply-change
description: Implement tasks from an OpenSpec change. Use when the user wants to start implementing, continue implementation, or work through tasks.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1"
---

Implement tasks from an OpenSpec change.

**Input**: Optionally specify a change name. If omitted, check if it can be inferred from conversation context. If vague or ambiguous you MUST prompt for available changes.

**Steps**

1. **Select the change**

   If a name is provided, use it. Otherwise:
   - Infer from conversation context if the user mentioned a change
   - Auto-select if only one active change exists
   - If ambiguous, run `openspec list --json` to get available changes and use the **AskUserQuestion tool** to let the user select

   Always announce: "Using change: <name>" and how to override (e.g., `/opsx:apply <other>`).

2. **Check status to understand the schema**
   ```bash
   openspec status --change "<name>" --json
   ```
   Parse the JSON to understand:
   - `schemaName`: The workflow being used (e.g., "spec-driven")
   - Which artifact contains the tasks (typically "tasks" for spec-driven, check status for others)

3. **Get apply instructions**

   ```bash
   openspec instructions apply --change "<name>" --json
   ```

   This returns:
   - Context file paths (varies by schema - could be proposal/specs/design/tasks or spec/tests/implementation/docs)
   - Progress (total, complete, remaining)
   - Task list with status
   - Dynamic instruction based on current state

   **Handle states:**
   - If `state: "blocked"` (missing artifacts): show message, suggest using openspec-continue-change
   - If `state: "all_done"`: congratulate, suggest archive
   - Otherwise: proceed to implementation

4. **Read context files**

   Read the files listed in `contextFiles` from the apply instructions output.
   The files depend on the schema being used:
   - **spec-driven**: proposal, specs, design, tasks
   - Other schemas: follow the contextFiles from CLI output

5. **Show current progress**

   Display:
   - Schema being used
   - Progress: "N/M tasks complete"
   - Remaining tasks overview
   - Dynamic instruction from CLI

5.5 **Optional GStack checkpoints (policy-driven)**

   Use this only when `.codex/opsx-gstack-policy.json` enables it.

   - If policy is missing: assume `mode=off`, run baseline OpenSpec apply flow.
   - If policy is invalid or gstack is unavailable: warn and continue baseline OpenSpec apply flow.
   - If mode is `assist` or `strict`:
     - Only use `allowlist.apply`.
     - Never run any `blocklist` skill.
     - Before each gstack invocation:
       - Notify user which skill is about to run and why.
       - Request explicit confirmation when policy `confirmation.enabled=true` and `confirmation.mode=ask-per-invocation`.
       - If confirmation is denied, skip that checkpoint and continue apply flow.
     - Persist checkpoint output to:
       - `openspec/changes/<name>/gstack-evidence.md`
     - Keep task progression unchanged (`- [ ]` -> `- [x]`) regardless of checkpoint tooling.

   **Apply phase default allowlist contract**
   - `gstack-review`
   - `gstack-qa`
   - `gstack-qa-only`
   - `gstack-design-review`
   - `gstack-cso`
   - `gstack-investigate`
   - `gstack-browse`

   **Hard blocklist contract**
   - `gstack-ship`
   - `gstack-land-and-deploy`
   - `gstack-setup-deploy`

   **Browse profile rule**
   - `gstack-browse` is allowed only in apply flow.
   - Treat browse as advisory diagnostics, never as a strict blocking gate.
   - Use browse only for explicit UI automation/inspection requests.

6. **CRITICAL: Trust the Documentation - DO NOT INVENT**

   **⚠️ Implementation Rule #1: Follow artifacts EXACTLY - do not improvise or "improve" ⚠️**

   The artifacts (proposal.md, design.md, specs/*.md, tasks.md) contain EXTREMELY DETAILED instructions. They specify:
   - Exact UI element locations (which page, where on page)
   - Exact component reuse decisions (reuse X, modify Y, create new Z)
   - Exact navigation flows (PageA → PageB with parameters)
   - Exact XAML patterns (with property bindings)
   - Exact method signatures
   - Exact UX behaviors (inference logic, visual indicators, ordering)

   **When implementing, you MUST:**
   - ✅ Read ALL context files thoroughly before starting ANY task
   - ✅ Follow EXACTLY what the artifacts specify (locations, names, patterns)
   - ✅ Use the EXACT components/pages mentioned (do NOT substitute)
   - ✅ Copy XAML/code patterns from design.md examples when provided
   - ✅ Respect "FORBIDDEN" constraints from proposal.md/design.md
   - ✅ Follow component reuse matrix (reuse vs modify vs new)
   - ✅ Implement UI flows as documented (all navigation steps with parameters)
   - ✅ Use exact configuration values from tasks.md (e.g., ItemsSource lists)

   **You MUST NEVER:**
   - ❌ "Improve" or "optimize" the design (implement what's specified, suggest improvements separately)
   - ❌ Put UI elements in different locations than specified
   - ❌ Create new components when artifacts say to reuse existing ones
   - ❌ Substitute one page for another because "it seems similar"
   - ❌ Add features not in the artifacts (scope creep)
   - ❌ Skip steps in documented UI flows
   - ❌ Assume UX patterns without checking documentation
   - ❌ Make up method signatures when specs provide exact signatures

   **Example violations to AVOID:**
   - Artifact says "Add button in SettingsPage below Clear Target" → ❌ You put it in a submenu
   - Artifact says "Reuse SearchPage in ingredient-selection mode" → ❌ You create new IngredientSearchPage
   - Artifact says "Show [R] badge for recipes" → ❌ You use a different visual indicator
   - Artifact specifies exact method signature → ❌ You add extra parameters "for flexibility"

   **If you find issues with the documentation:**
   - PAUSE implementation
   - Report the specific issue or ambiguity
   - Suggest updating the artifact (use openspec-continue-change to fix it)
   - DO NOT "fix it yourself" by inventing - get clarification first

   **Trust the artifacts - they were created with extreme care and detail. Your job is faithful implementation, not creative interpretation.**

7. **Implement tasks (loop until done or blocked)**

   For each pending task:
   - Show which task is being worked on
   - Make the code changes required
   - Keep changes minimal and focused
   - Mark task complete in the tasks file: `- [ ]` → `- [x]`
   - Continue to next task

   **Pause if:**
   - Task is unclear → ask for clarification
   - Implementation reveals a design issue → suggest updating artifacts
   - Error or blocker encountered → report and wait for guidance
   - User interrupts

8. **On completion or pause, show status**

   Display:
   - Tasks completed this session
   - Overall progress: "N/M tasks complete"
   - If all done: suggest archive
   - If paused: explain why and wait for guidance

**Output During Implementation**

```
## Implementing: <change-name> (schema: <schema-name>)

Working on task 3/7: <task description>
[...implementation happening...]
✓ Task complete

Working on task 4/7: <task description>
[...implementation happening...]
✓ Task complete
```

**Output On Completion**

```
## Implementation Complete

**Change:** <change-name>
**Schema:** <schema-name>
**Progress:** 7/7 tasks complete ✓

### Completed This Session
- [x] Task 1
- [x] Task 2
...

All tasks complete! Ready to archive this change.
```

**Output On Pause (Issue Encountered)**

```
## Implementation Paused

**Change:** <change-name>
**Schema:** <schema-name>
**Progress:** 4/7 tasks complete

### Issue Encountered
<description of the issue>

**Options:**
1. <option 1>
2. <option 2>
3. Other approach

What would you like to do?
```

**Guardrails**
- Keep going through tasks until done or blocked
- Always read context files before starting (from the apply instructions output)
- If task is ambiguous, pause and ask before implementing
- If implementation reveals issues, pause and suggest artifact updates
- Keep code changes minimal and scoped to each task
- Update task checkbox immediately after completing each task
- Pause on errors, blockers, or unclear requirements - don't guess
- Use contextFiles from CLI output, don't assume specific file names

**Fluid Workflow Integration**

This skill supports the "actions on a change" model:

- **Can be invoked anytime**: Before all artifacts are done (if tasks exist), after partial implementation, interleaved with other actions
- **Allows artifact updates**: If implementation reveals design issues, suggest updating artifacts - not phase-locked, work fluidly
