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

---

## CRITICAL: EXTREME DETAIL REQUIREMENT FOR ALL ARTIFACTS

**⚠️ THESE RULES ARE MANDATORY FOR ALL ARTIFACTS ⚠️**

All artifacts MUST be created with EXTREME EXPLICITNESS to prevent any AI implementation from "inventing" or "interpreting" requirements.

### General Principles for ALL Artifacts

1. **NEVER assume the implementer has context from exploration**
   - Document EVERY decision made during exploration
   - Include WHY choices were made, not just WHAT was chosen
   - Document alternatives considered and why they were rejected

2. **NEVER leave room for interpretation**
   - Be prescriptive, not suggestive
   - Use MUST/SHALL, not "should" or "could"
   - Specify EXACT locations, names, patterns, colors, layouts

3. **NEVER omit seemingly "obvious" details**
   - Where buttons go (EXACTLY which page, EXACTLY where on that page)
   - Which existing components to reuse vs creating new ones
   - Navigation flows with every intermediate step
   - Data flow between components

### Artifact-Specific Requirements

#### PROPOSAL.MD - Add at the very beginning:

**MUST include** a "CRITICAL IMPLEMENTATION CONSTRAINTS" section at the top with:
- List of non-negotiable design decisions (numbered)
- Specific locations/components mentioned (e.g., "Button MUST be in SettingsPage")
- Interaction patterns that MUST be followed (e.g., "MUST reuse SearchPage for X")
- Visual patterns that MUST be maintained (e.g., "MUST show [A] badge for foods")
- Explicit list of things NOT to do (❌ DO NOT create...)

**MUST be explicit about**:
- EXACT navigation paths for new features (PageA → PageB → PageC with parameters)
- Which existing pages/components are modified vs created new
- Where new navigation entry points are added (which menu, which section, EXACTLY where)

#### DESIGN.MD - Add at the very beginning:

**MUST include** "IMPLEMENTATION RULES - DO NOT DEVIATE" section with:
- ❌ Forbidden actions list (what NOT to do)
- ✅ Required actions list (what MUST be done)
- Architecture patterns that MUST be followed
- Component reuse rules

**MUST include** detailed sections:
1. **DETAILED UI FLOWS AND COMPONENT REUSE** (MANDATORY):
   - Break down EVERY user flow into numbered steps
   - Example: "Flow 1: Accessing Recipe Management"
     - Starting Point: [exact page]
     - Navigation Path: [PageA → PageB → PageC with transition types]
     - Step 1: User taps [exact button name] on [exact page]
     - Step 2: System executes [exact command name]
     - Step 3: Navigation calls [exact code: await Shell.Current.GoToAsync("...")]
     - etc. for EVERY interaction in that flow
   - Include 5-10 critical flows depending on feature complexity

2. **DETAILED PAGE WIREFRAMES** (MANDATORY for any UI changes):
   - ASCII art or structured text layout of EVERY page
   - Show EXACT component placement
   - Include Component Breakdown listing all UI elements with bindings

3. **COMPONENT REUSE MATRIX** (MANDATORY):
   - Table showing Component | Reused As-Is | Modified | New
   - EXPLICIT about what gets reused vs created

4. **CODE EXAMPLES FOR CRITICAL COMPONENTS** (MANDATORY):
   - Provide 3-6 complete, functional code examples
   - Cover the most complex/ambiguous components
   - Include both XAML and C# where applicable
   - Examples should be COPY-PASTE ready

5. **CRITICAL UX BEHAVIORS** (MANDATORY):
   - Document EXACT behavior for inference logic (e.g., time-based defaults)
   - Specify EXACT format for visual indicators (e.g., "⭐ [R] Recipe Name")
   - Include code snippets for behavior logic

6. **IMPLEMENTATION VERIFICATION CHECKLIST** (MANDATORY):
   - 50-100 checkbox items organized by category
   - Cover: Navigation, Display, Data Model, Services, Testing
   - Include "CRITICAL DEVIATIONS CHECK" section with deal-breaker items
   - Format: "- [ ] ✅ [positive check]" and "- [ ] ❌ [negative check - if true, WRONG]"

**Database changes MUST include**:
- EXACT migration SQL/code
- EXACT schema with column names, types, constraints
- EXACT indexes to create
- Migration verification steps

**Service changes MUST include**:
- EXACT interface method signatures
- EXACT implementation logic for complex methods
- Code examples showing how services are called

#### SPECS/*.MD

**MUST be testable and unambiguous**:
- Each requirement has multiple scenarios
- Each scenario uses WHEN/THEN format
- Be specific about EXACT behavior, not vague descriptions
- Include edge cases and error conditions

#### TASKS.MD

**EACH task MUST**:
- Be COMPLETELY self-contained (implementer doesn't need to read other docs to understand)
- Specify EXACT file paths and class names
- Include EXACT code patterns or references to design.md examples
- Specify EXACT validation criteria (how to know it's done correctly)

**Example of GOOD task detail level**:
"Create EditRecipePage.xaml in MasQueDieta.App/Pages with Entry for recipe name, 'Add Ingredient' button, CollectionView for ingredients with item template showing food name + quantity Entry + unit Picker + remove button, Label for total points, Save and Cancel buttons"

**Example of BAD task (too vague)**:
"Create EditRecipePage"

**Example of GOOD task with code specifics**:
"Implement MealType Picker in XAML with ItemsSource={'Desayuno','Comida','Cena','Snack'}, SelectedItem bound to MealType property, infer MealType in OnNavigatedTo: if current time 6:00-11:00 set 'Desayuno', 11:00-16:00 set 'Comida', 16:00-21:00 set 'Cena', else 'Snack' (SAME logic as EditEntryPage for food items)"

### During Exploration Phase

**BEFORE creating any artifacts**, ensure you've gathered:
- [ ] All navigation flows discussed with user
- [ ] All UX patterns and their consistency with existing app
- [ ] All UI wireframes or layouts discussed
- [ ] All data model decisions (tables, columns, relationships)
- [ ] All service architecture decisions
- [ ] All component reuse decisions
- [ ] All alternatives considered and rejected
- [ ] All edge cases and error scenarios
- [ ] All testing strategies

**If any of the above is unclear, ASK the user in explore mode before proceeding to artifact creation.**

### Document Interconnection

Artifacts MUST reference each other:
- design.md MUST reference specific requirements from specs by name
- tasks.md MUST reference design.md sections for implementation details
- specs MUST reference capabilities from proposal.md

**Cross-reference format**: "See design.md 'Decision 3: Snapshot Strategy' for implementation approach" or "Implements requirement 'Track recipe with decimal servings' from specs/recipe-consumption/spec.md"

### Quality Check Before Submitting Artifact

Ask yourself:
1. Could another AI implement this WITHOUT guessing or inventing? (If no → add more detail)
2. Are EXACT locations specified for all UI elements? (If no → add wireframes/layouts)
3. Are all reused components explicitly called out? (If no → add component reuse matrix)
4. Are code examples provided for ambiguous parts? (If no → add examples)
5. Is there a verification checklist? (If no → add one)

**If any answer is NO, the artifact is incomplete.**

---

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
