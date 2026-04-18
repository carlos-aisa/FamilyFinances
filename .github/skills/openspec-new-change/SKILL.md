---
name: openspec-new-change
description: Start a new OpenSpec change using the experimental artifact workflow. Use when the user wants to create a new feature, fix, or modification with a structured step-by-step approach.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1"
---

Start a new change using the experimental artifact-driven approach.

**Input**: The user's request should include a change name (kebab-case) OR a description of what they want to build.

**Steps**

1. **If no clear input provided, ask what they want to build**

   Use the **AskUserQuestion tool** (open-ended, no preset options) to ask:
   > "What change do you want to work on? Describe what you want to build or fix."

   From their description, derive a kebab-case name (for example, "add user authentication" -> `add-user-auth`).

   **IMPORTANT**: Do NOT proceed without understanding what the user wants to build.

2. **CRITICAL: Ask detailed, in-depth questions**

   The quality of future artifacts depends on the depth of information gathered now.

   Before proceeding to create the change, ask thorough questions to gather all necessary information.

   **UI/Frontend Changes:**
   - "Where exactly in the UI should this appear?" (specific page, exact location)
   - "What is the navigation flow?" (from where -> to where, with what parameters)
   - "Should we reuse any existing pages/components? Which ones?"
   - "What is the user interaction pattern?" (tap, swipe, modal, full-page)
   - "How should it look visually?" (describe or reference existing patterns)
   - "Are there similar existing features we should stay consistent with?"

   **Data/Backend Changes:**
   - "What data needs to be stored?" (tables, columns, relationships)
   - "Are we extending existing tables or creating new ones?"
   - "What are the entities and their relationships?"
   - "What calculations or business logic are involved?" (exact formulas)
   - "What happens to existing data?" (migration strategy)

   **Feature Behavior:**
   - "What is the expected behavior when the user does X?"
   - "What are the edge cases?" (empty states, errors, conflicts)
   - "What validations are needed?" (required fields, unique constraints, ranges)
   - "Should values be inferred automatically? Based on what logic?"

   **Integration Points:**
   - "Does this integrate with existing services/features? How?"
   - "What existing code should be modified vs what is new?"
   - "Are there dependencies on other changes or features?"

   **Release impact (mandatory):**
   - Ask explicitly: "What release impact should this change have: `patch`, `minor`, or `major`?"
   - Confirm the meaning:
     - `patch`: bug fix or internal change, backward compatible
     - `minor`: new backward-compatible functionality
     - `major`: breaking or behaviorally incompatible change

   **DO NOT accept vague answers.**

   If the user says "add a button", ask:
   - "Where exactly? Which page?"
   - "Where on that page? After what element?"
   - "What should it say?"
   - "What happens when the user taps it?"

   If the user says "make it like X", verify:
   - "So we are reusing X component? Or creating something similar?"
   - "What specifically are we keeping from X? What is different?"

   **Goal**: By the end of this questioning, you should be able to mentally sketch:
   - Complete navigation flows
   - Exact UI component locations and layouts
   - Complete data model
   - Component reuse decisions
   - Behavior/inference logic
   - Edge cases and error handling

   If you cannot mentally sketch these, ask more questions.

   Suggest explore mode for complex changes:
   > "This sounds like it may need exploration to work through details. Want to use `opsx:explore` first?"

3. **Determine the workflow schema**

   Use the default schema (omit `--schema`) unless the user explicitly requests a different workflow.

   **Use a different schema only if the user mentions:**
   - A specific schema name -> use `--schema <name>`
   - "show workflows" or "what workflows" -> run `openspec schemas --json` and let them choose

   Otherwise, omit `--schema` to use the default.

4. **Create the change directory**
   ```bash
   openspec new change "<name>"
   ```
   Add `--schema <name>` only if the user requested a specific workflow.

5. **Show the artifact status**
   ```bash
   openspec status --change "<name>"
   ```

6. **Get instructions for the first artifact**
   Check the status output to find the first artifact with status "ready".
   ```bash
   openspec instructions <first-artifact-id> --change "<name>"
   ```

7. **Require release impact metadata in proposal markdown**

   Tell the user that `proposal.md` must include:

   ```markdown
   ## Release Impact

   Type: <patch|minor|major>
   Rationale: <short reason>
   ```

   `/opsx:archive` uses this section to auto-create PR and semantic tag.

8. **STOP and wait for user direction**

**Output**

After completing the steps, summarize:
- Change name and location
- Schema/workflow and artifact sequence
- Current status (0/N artifacts complete)
- The template for the first artifact
- The required markdown block for `Release Impact`
- Prompt: "Ready to create the first artifact? Just describe what this change is about and I will draft it, or ask me to continue."

**Guardrails**
- Do NOT create artifacts yet
- Do NOT advance beyond showing the first artifact template
- If name is invalid (not kebab-case), ask for a valid name
- If change already exists, suggest continuing that change instead
- Pass `--schema` only when using a non-default workflow
- Do NOT continue if release impact (`patch|minor|major`) is undefined
