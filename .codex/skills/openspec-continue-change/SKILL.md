---
name: openspec-continue-change
description: Continue working on an OpenSpec change by creating the next artifact. Use when the user wants to progress their change, create the next artifact, or continue their workflow.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1"
---

Continue working on a change by creating the next artifact.

**Input**: Optionally specify a change name. If omitted, check if it can be inferred from conversation context. If vague or ambiguous you MUST prompt for available changes.

**Steps**

1. **If no change name provided, prompt for selection**

   Run `openspec list --json` to get available changes sorted by most recently modified. Then use the **AskUserQuestion tool** to let the user select which change to work on.

   Present the top 3-4 most recently modified changes as options, showing:
   - Change name
   - Schema (from `schema` field if present, otherwise "spec-driven")
   - Status (e.g., "0/5 tasks", "complete", "no tasks")
   - How recently it was modified (from `lastModified` field)

   Mark the most recently modified change as "(Recommended)" since it's likely what the user wants to continue.

   **IMPORTANT**: Do NOT guess or auto-select a change. Always let the user choose.

2. **Check current status**
   ```bash
   openspec status --change "<name>" --json
   ```
   Parse the JSON to understand current state. The response includes:
   - `schemaName`: The workflow schema being used (e.g., "spec-driven")
   - `artifacts`: Array of artifacts with their status ("done", "ready", "blocked")
   - `isComplete`: Boolean indicating if all artifacts are complete

3. **Act based on status**:

   ---

   **If all artifacts are complete (`isComplete: true`)**:
   - Congratulate the user
   - Show final status including the schema used
   - Suggest: "All artifacts created! You can now implement this change or archive it."
   - STOP

   ---

   **If artifacts are ready to create** (status shows artifacts with `status: "ready"`):
   - Pick the FIRST artifact with `status: "ready"` from the status output
   - Get its instructions:
     ```bash
     openspec instructions <artifact-id> --change "<name>" --json
     ```
   - Parse the JSON. The key fields are:
     - `context`: Project background (constraints for you - do NOT include in output)
     - `rules`: Artifact-specific rules (constraints for you - do NOT include in output)
     - `template`: The structure to use for your output file
     - `instruction`: Schema-specific guidance
     - `outputPath`: Where to write the artifact
     - `dependencies`: Completed artifacts to read for context
   - **Create the artifact file**:
     - Read any completed dependency files for context
     - Use `template` as the structure - fill in its sections
     - Apply `context` and `rules` as constraints when writing - but do NOT copy them into the file
     - Write to the output path specified in instructions
   - Show what was created and what's now unlocked
   - STOP after creating ONE artifact

   ---

   **If no artifacts are ready (all blocked)**:
   - This shouldn't happen with a valid schema
   - Show status and suggest checking for issues

4. **After creating an artifact, show progress**
   ```bash
   openspec status --change "<name>"
   ```

**Output**

After each invocation, show:
- Which artifact was created
- Schema workflow being used
- Current progress (N/M complete)
- What artifacts are now unlocked
- Prompt: "Want to continue? Just ask me to continue or tell me what to do next."

**Artifact Creation Guidelines**

The artifact types and their purpose depend on the schema. Use the `instruction` field from the instructions output to understand what to create.

Common artifact patterns:

**spec-driven schema** (proposal → specs → design → tasks):
- **proposal.md**: Ask user about the change if not clear. Fill in Why, What Changes, Capabilities, Impact.
  - The Capabilities section is critical - each capability listed will need a spec file.
- **specs/<capability>/spec.md**: Create one spec per capability listed in the proposal's Capabilities section (use the capability name, not the change name).
- **design.md**: Document technical decisions, architecture, and implementation approach.
- **tasks.md**: Break down implementation into checkboxed tasks.

For other schemas, follow the `instruction` field from the CLI output.

**CRITICAL: EXTREME DETAIL REQUIREMENT FOR ALL ARTIFACTS**

**⚠️ The implementation quality directly depends on artifact detail and explicitness ⚠️**

**General Principles for ALL Artifacts:**
1. **NEVER assume the implementer has context** - Write as if the implementer is seeing the codebase for the first time
2. **NEVER leave room for interpretation** - Specify exact locations, exact names, exact patterns
3. **NEVER omit "obvious" details** - What's obvious to you may not be to an AI or human weeks later

**Artifact-Specific Requirements:**

**proposal.md:**
- Add "CRITICAL IMPLEMENTATION CONSTRAINTS" section at top
- List ❌ FORBIDDEN actions (what must NOT be done)
- List ✅ REQUIRED actions (what MUST be done)
- Be explicit about what must NOT be invented

**design.md MUST include these 6 sections:**
1. "IMPLEMENTATION RULES - DO NOT DEVIATE" - List specific ❌/✅ items
2. "DETAILED UI FLOWS" - Numbered step-by-step for EVERY user interaction
3. "DETAILED PAGE WIREFRAMES" - ASCII art showing exact component placement
4. "COMPONENT REUSE MATRIX" - Table: what's reused vs modified vs new
5. "CODE EXAMPLES FOR CRITICAL COMPONENTS" - 4-6 copy-paste ready examples
6. "IMPLEMENTATION VERIFICATION CHECKLIST" - 50+ items with ✅/❌ format

**specs/*.md:**
- Exact method signatures with parameter types and return types
- Exact table schemas with column names, types, constraints
- Exact XAML snippets for UI property bindings
- Exact navigation parameters and their sources
- Error handling patterns and validation rules

**tasks.md:**
- Each task MUST be completely self-contained
- Include exact file paths (absolute from workspace root)
- Include exact code patterns to copy/paste
- Include exact XAML patterns with property bindings
- Include validation criteria to verify completion
- Specify exact values (e.g., "ItemsSource={'Desayuno','Comida','Cena','Snack'}")

**Quality Check Before Submission:**
Before marking an artifact complete, ask yourself:
1. Could an implementer complete this WITHOUT ASKING ANY QUESTIONS?
2. Is every component reuse decision explicitly stated?
3. Are all navigation flows documented start-to-end with parameters?
4. Are all UX patterns documented with exact behavior?
5. Can I point to exact file paths and code patterns for ambiguous concepts?

**If any answer is NO, add more detail to the artifact.**

**Guardrails**
- Create ONE artifact per invocation
- Always read dependency artifacts before creating a new one
- Never skip artifacts or create out of order
- If context is unclear, ask the user before creating
- Verify the artifact file exists after writing before marking progress
- Use the schema's artifact sequence, don't assume specific artifact names
- **IMPORTANT**: `context` and `rules` are constraints for YOU, not content for the file
  - Do NOT copy `<context>`, `<rules>`, `<project_context>` blocks into the artifact
  - These guide what you write, but should never appear in the output
