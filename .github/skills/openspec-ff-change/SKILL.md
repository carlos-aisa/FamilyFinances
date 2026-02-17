---
name: openspec-ff-change
description: Fast-forward through OpenSpec artifact creation. Use when the user wants to quickly create all artifacts needed for implementation without stepping through each one individually.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1"
---

Fast-forward through artifact creation - generate everything needed to start implementation in one go.

**Input**: The user's request should include a change name (kebab-case) OR a description of what they want to build.

**Steps**

1. **If no clear input provided, ask what they want to build**

   Use the **AskUserQuestion tool** (open-ended, no preset options) to ask:
   > "What change do you want to work on? Describe what you want to build or fix."

   From their description, derive a kebab-case name (e.g., "add user authentication" → `add-user-auth`).

   **IMPORTANT**: Do NOT proceed without understanding what the user wants to build.

2. **Create the change directory**
   ```bash
   openspec new change "<name>"
   ```
   This creates a scaffolded change at `openspec/changes/<name>/`.

3. **Get the artifact build order**
   ```bash
   openspec status --change "<name>" --json
   ```
   Parse the JSON to get:
   - `applyRequires`: array of artifact IDs needed before implementation (e.g., `["tasks"]`)
   - `artifacts`: list of all artifacts with their status and dependencies

4. **Create artifacts in sequence until apply-ready**

   Use the **TodoWrite tool** to track progress through the artifacts.

   Loop through artifacts in dependency order (artifacts with no pending dependencies first):

   a. **For each artifact that is `ready` (dependencies satisfied)**:
      - Get instructions:
        ```bash
        openspec instructions <artifact-id> --change "<name>" --json
        ```
      - The instructions JSON includes:
        - `context`: Project background (constraints for you - do NOT include in output)
        - `rules`: Artifact-specific rules (constraints for you - do NOT include in output)
        - `template`: The structure to use for your output file
        - `instruction`: Schema-specific guidance for this artifact type
        - `outputPath`: Where to write the artifact
        - `dependencies`: Completed artifacts to read for context
      - Read any completed dependency files for context
      - Create the artifact file using `template` as the structure
      - Apply `context` and `rules` as constraints - but do NOT copy them into the file
      - Show brief progress: "✓ Created <artifact-id>"

   b. **Continue until all `applyRequires` artifacts are complete**
      - After creating each artifact, re-run `openspec status --change "<name>" --json`
      - Check if every artifact ID in `applyRequires` has `status: "done"` in the artifacts array
      - Stop when all `applyRequires` artifacts are done

   c. **If an artifact requires user input** (unclear context):
      - Use **AskUserQuestion tool** to clarify
      - Then continue with creation

5. **Show final status**
   ```bash
   openspec status --change "<name>"
   ```

**Output**

After completing all artifacts, summarize:
- Change name and location
- List of artifacts created with brief descriptions
- What's ready: "All artifacts created! Ready for implementation."
- Prompt: "Run `/opsx:apply` or ask me to implement to start working on the tasks."

**Artifact Creation Guidelines**

- Follow the `instruction` field from `openspec instructions` for each artifact type
- The schema defines what each artifact should contain - follow it
- Read dependency artifacts for context before creating new ones
- Use `template` as the structure for your output file - fill in its sections
- **IMPORTANT**: `context` and `rules` are constraints for YOU, not content for the file
  - Do NOT copy `<context>`, `<rules>`, `<project_context>` blocks into the artifact
  - These guide what you write, but should never appear in the output

**CRITICAL: EXTREME DETAIL REQUIREMENT FOR ALL ARTIFACTS**

**⚠️ The implementation quality directly depends on artifact detail and explicitness ⚠️**

Fast-forward mode does NOT mean "quick and vague" - it means "create all artifacts quickly BUT with EXTREME DETAIL". Every artifact MUST be as explicit and detailed as if you were creating it step-by-step.

**Apply ALL requirements from openspec-continue-change skill's "EXTREME DETAIL REQUIREMENT" section**, specifically:

**General Principles for ALL Artifacts:**
1. **NEVER assume the implementer has context** - Write as if the implementer is seeing the codebase for the first time
2. **NEVER leave room for interpretation** - Specify exact locations, exact names, exact patterns
3. **NEVER omit "obvious" details** - What's obvious to you may not be to an AI or human weeks later

**Artifact-Specific Detail Requirements:**

**proposal.md:**
- Add "CRITICAL IMPLEMENTATION CONSTRAINTS" section at top listing ❌ FORBIDDEN actions and ✅ REQUIRED actions
- Be explicit about what must NOT be done (component invention, pattern violations)

**design.md:**
- Add "IMPLEMENTATION RULES - DO NOT DEVIATE" section listing specific do/don't items
- Add "DETAILED UI FLOWS AND COMPONENT REUSE" section with numbered step-by-step flows for EVERY user interaction
- Add "DETAILED PAGE WIREFRAMES" with ASCII art showing exact component placement
- Add "COMPONENT REUSE MATRIX" table showing what's reused vs modified vs new
- Add "CODE EXAMPLES FOR CRITICAL COMPONENTS" with 4-6 copy-paste ready examples
- Add "CRITICAL UX BEHAVIORS" explaining inference logic, visual indicators, ordering
- Add "IMPLEMENTATION VERIFICATION CHECKLIST" with 50+ items organized by category using ✅/❌ format

**specs/*.md:**
- Specify exact method signatures with parameter types and return types
- Include exact table schemas with column names, types, constraints
- Show exact XAML snippets for UI property bindings
- Specify exact navigation parameters and their sources
- Include error handling patterns and validation rules

**tasks.md:**
- Each task MUST be completely self-contained
- Include exact file paths (absolute from workspace root)
- Include exact code patterns to copy/paste
- Include exact XAML patterns with property bindings
- Include validation criteria to verify task completion
- Specify exact values for configuration (e.g., "ItemsSource={'Desayuno','Comida','Cena','Snack'}")

**During artifact creation, continuously ask yourself:**
1. Could an implementer complete this WITHOUT ASKING ANY QUESTIONS? (if no, add more detail)
2. Is every component reuse decision explicitly stated? (if no, document it)
3. Are all navigation flows documented start-to-end with parameters? (if no, add flows)
4. Are all UX patterns documented with exact behavior? (if no, specify them)
5. Can I point to exact file paths and code patterns for ambiguous concepts? (if no, add examples)

**Red Flags - If you see these in your artifacts, they're INSUFFICIENT:**
- ❌ "Create a page for managing recipes" (MISSING: exact location in navigation, exact UI layout, exact component reuse)
- ❌ "Add a search feature" (MISSING: reusing SearchPage or creating new? what mode? what parameters?)
- ❌ "Store recipe data" (MISSING: exact table schema with column types and constraints)
- ❌ "Show meal type" (MISSING: exact UI format, inference logic, where displayed, how bound)
- ❌ "Handle errors appropriately" (MISSING: exact error messages, exact validation rules)

**Verification Before Moving to Next Artifact:**
Before marking an artifact as done and moving to the next:
- [ ] All sections have substantial, specific content (not placeholders)
- [ ] design.md has: UI Flows, Wireframes, Reuse Matrix, Code Examples, UX Behaviors, Verification Checklist
- [ ] tasks.md has: Every task self-contained with file paths, code patterns, validation criteria
- [ ] No ambiguous language ("appropriate", "suitable", "as needed")
- [ ] All component reuse decisions explicitly stated
- [ ] All navigation flows documented with parameters

**If any verification item is unchecked, enhance the artifact before proceeding.**

Remember: Fast-forward means "create all artifacts fast" NOT "create vague artifacts fast". Each artifact must be implementation-ready with zero ambiguity.

**Guardrails**
- Create ALL artifacts needed for implementation (as defined by schema's `apply.requires`)
- Always read dependency artifacts before creating a new one
- If context is critically unclear, ask the user - but prefer making reasonable decisions to keep momentum
- If a change with that name already exists, suggest continuing that change instead
- Verify each artifact file exists after writing before proceeding to next
