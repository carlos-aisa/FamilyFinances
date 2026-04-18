---
description: Start a new change using the experimental artifact workflow (OPSX)
---

Start a new change using the experimental artifact-driven approach.

**Input**: The argument after `/opsx:new` is the change name (kebab-case), OR a description of what the user wants to build.

**Steps**

1. **If no input provided, ask what they want to build**

   Use the **AskUserQuestion tool** (open-ended, no preset options) to ask:
   > "What change do you want to work on? Describe what you want to build or fix."

   From their description, derive a kebab-case name (for example, "add user authentication" -> `add-user-auth`).

   **IMPORTANT**: Do NOT proceed without understanding what the user wants to build.

2. **CRITICAL: Ask detailed questions to eliminate ambiguity**

   **Artifact quality depends on information gathered now.**

   Before creating the change, ask thorough questions:

   **For UI/Frontend Changes:**
   - "Where exactly in the UI?" (specific page, exact location)
   - "What is the navigation flow?" (from where -> to where, parameters)
   - "Should we reuse existing pages/components? Which ones?"
   - "What is the user interaction?" (tap, modal, full-page)
   - "How should it look?" (describe or reference patterns)

   **For Data/Backend:**
   - "What data should be stored?" (tables, columns, relationships)
   - "Extend existing tables or create new ones?"
   - "What calculations/logic are needed?" (exact formulas)
   - "What happens to existing data?" (migration)

   **For Behavior:**
   - "Expected behavior when user does X?"
   - "Edge cases?" (empty states, errors, conflicts)
   - "Validations needed?" (required, unique, ranges)
   - "Auto-inferred values? Based on what logic?"

   **Release impact (mandatory):**
   - Ask explicitly: "What release impact should this change have: `patch`, `minor`, or `major`?"
   - Confirm the meaning:
     - `patch`: bug fix or internal change, backward compatible
     - `minor`: new backward-compatible functionality
     - `major`: breaking or behaviorally incompatible change

   **Do not accept vague answers:**
   - User: "add a button" -> Ask: "Where? Which page? Where on page? What text? What happens on tap?"
   - User: "make it like X" -> Ask: "Reusing X component? Or similar? What stays the same? What differs?"

   **Goal:** By end of questioning, you should be able to sketch:
   - Complete navigation flows
   - Exact UI layouts
   - Complete data model
   - Component reuse decisions
   - Behavior and logic
   - Edge cases

   **If you cannot sketch these, ask more questions.**

   For complex changes, suggest: "This might need exploration first. Want to use `/opsx:explore` to think through the design?"

3. **Determine the workflow schema**

   Use the default schema (omit `--schema`) unless the user explicitly requests a different workflow.

   **Use a different schema only if the user mentions:**
   - A specific schema name -> use `--schema <name>`
   - "show workflows" or "what workflows" -> run `openspec schemas --json` and let them choose

   **Otherwise**: Omit `--schema` to use the default.

4. **Create the change directory**
   ```bash
   openspec new change "<name>"
   ```
   Add `--schema <name>` only if the user requested a specific workflow.
   This creates a scaffolded change at `openspec/changes/<name>/` with the selected schema.

5. **Show the artifact status**
   ```bash
   openspec status --change "<name>"
   ```
   This shows which artifacts need to be created and which are ready (dependencies satisfied).

6. **Get instructions for the first artifact**
   The first artifact depends on the schema. Check the status output to find the first artifact with status "ready".
   ```bash
   openspec instructions <first-artifact-id> --change "<name>"
   ```
   This outputs the template and context for creating the first artifact.

7. **Ensure the proposal will contain release impact metadata in markdown**

   Tell the user that the proposal must include this section exactly:

   ```markdown
   ## Release Impact

   Type: <patch|minor|major>
   Rationale: <short reason>
   ```

   This section is required by `/opsx:archive` to auto-create PR and semantic tag.

8. **STOP and wait for user direction**

**Output**

After completing the steps, summarize:
- Change name and location
- Schema/workflow being used and its artifact sequence
- Current status (0/N artifacts complete)
- The template for the first artifact
- The required markdown block for `Release Impact`
- Prompt: "Ready to create the first artifact? Run `/opsx:continue` or just describe what this change is about and I will draft it."

**Guardrails**
- Do NOT create any artifacts yet - just show the instructions
- Do NOT advance beyond showing the first artifact template
- If the name is invalid (not kebab-case), ask for a valid name
- If a change with that name already exists, suggest using `/opsx:continue` instead
- Pass `--schema` only when using a non-default workflow
- Do NOT continue if release impact (`patch|minor|major`) is still undefined
