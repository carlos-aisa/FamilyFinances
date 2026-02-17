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

   From their description, derive a kebab-case name (e.g., "add user authentication" → `add-user-auth`).

   **IMPORTANT**: Do NOT proceed without understanding what the user wants to build.

2. **CRITICAL: Ask Detailed, In-Depth Questions**

   **⚠️ The quality of future artifacts depends on the depth of information gathered now ⚠️**

   Before proceeding to create the change, you MUST ask thorough, detailed questions to gather ALL necessary information. The goal is to eliminate ambiguity and ensure future artifacts (proposal, design, specs, tasks) can be extremely explicit.

   **Questions to Ask (adapt based on change type):**

   **UI/Frontend Changes:**
   - "Where exactly in the UI should this appear?" (specific page, exact location)
   - "What's the navigation flow?" (from where → to where, with what parameters)
   - "Should we reuse any existing pages/components? Which ones?"
   - "What's the user interaction pattern?" (tap, swipe, modal, full-page)
   - "How should it look visually?" (describe or reference existing patterns)
   - "Are there similar existing features we should maintain consistency with?"

   **Data/Backend Changes:**
   - "What data needs to be stored?" (tables, columns, relationships)
   - "Are we extending existing tables or creating new ones?"
   - "What are the entities and their relationships?"
   - "What calculations or business logic is involved?" (exact formulas)
   - "What happens to existing data?" (migration strategy)

   **Feature Behavior:**
   - "What's the expected behavior when the user does X?"
   - "What are the edge cases?" (empty states, errors, conflicts)
   - "What validations are needed?" (required fields, unique constraints, ranges)
   - "Should values be inferred automatically? Based on what logic?"

   **Integration Points:**
   - "Does this integrate with existing services/features? How?"
   - "What existing code needs to be modified vs what's new?"
   - "Are there dependencies on other changes or features?"

   **UX Consistency:**
   - "How do similar features work in the app?"
   - "What patterns should we follow for consistency?"
   - "How do users currently accomplish similar tasks?"

   **DO NOT accept vague answers.** If the user says "add a button", ask:
   - "Where exactly? Which page?"
   - "Where on that page? After what element?"
   - "What should it say?"
   - "What happens when the user taps it?"

   If the user says "make it like X", verify:
   - "So we're reusing X component? Or creating something similar?"
   - "What specifically are we keeping from X? What's different?"

   **Goal**: By the end of this questioning, you should be able to mentally sketch:
   - ✅ Complete navigation flows (page-by-page with parameters)
   - ✅ Exact UI component locations and layouts
   - ✅ Complete data model (tables, columns, relationships)
   - ✅ All component reuse decisions (reuse vs modify vs create new)
   - ✅ All behavior and inference logic (formulas, rules, defaults)
   - ✅ Edge cases and error handling patterns

   **If you can't mentally sketch these, ask more questions.**

   Suggest going to explore mode for complex changes:
   > "This sounds like it might need some exploration to work through the details. Would you like to use explore mode first to think through the design before creating the change?"

3. **Determine the workflow schema**

   Use the default schema (omit `--schema`) unless the user explicitly requests a different workflow.

   **Use a different schema only if the user mentions:**
   - A specific schema name → use `--schema <name>`
   - "show workflows" or "what workflows" → run `openspec schemas --json` and let them choose

   **Otherwise**: Omit `--schema` to use the default.

3. **Create the change directory**
   ```bash
   openspec new change "<name>"
   ```
   Add `--schema <name>` only if the user requested a specific workflow.
   This creates a scaffolded change at `openspec/changes/<name>/` with the selected schema.

4. **Show the artifact status**
   ```bash
   openspec status --change "<name>"
   ```
   This shows which artifacts need to be created and which are ready (dependencies satisfied).

5. **Get instructions for the first artifact**
   The first artifact depends on the schema (e.g., `proposal` for spec-driven).
   Check the status output to find the first artifact with status "ready".
   ```bash
   openspec instructions <first-artifact-id> --change "<name>"
   ```
   This outputs the template and context for creating the first artifact.

6. **STOP and wait for user direction**

**Output**

After completing the steps, summarize:
- Change name and location
- Schema/workflow being used and its artifact sequence
- Current status (0/N artifacts complete)
- The template for the first artifact
- Prompt: "Ready to create the first artifact? Just describe what this change is about and I'll draft it, or ask me to continue."

**Guardrails**
- Do NOT create any artifacts yet - just show the instructions
- Do NOT advance beyond showing the first artifact template
- If the name is invalid (not kebab-case), ask for a valid name
- If a change with that name already exists, suggest continuing that change instead
- Pass --schema if using a non-default workflow
