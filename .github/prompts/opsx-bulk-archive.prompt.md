---
description: Archive multiple completed changes at once
---

Archive multiple completed changes in a single operation.

This flow supports automated release handoff after bulk archive:
- create or reuse a pull request
- generate and push one semantic tag
- choose tag bump using the highest release impact among archived changes

**Input**: None required (prompts for selection)

**Steps**

1. **Get active changes**

   Run `openspec list --json`.

   If no active changes exist, inform user and stop.

2. **Prompt for change selection**

   Use **AskUserQuestion tool** with multi-select:
   - Show each change with schema
   - Include "All changes"
   - Allow 1+ selections

   **IMPORTANT**: Do NOT auto-select.

3. **Batch validation for all selected changes**

   For each selected change, collect:

   a. **Artifact status** via `openspec status --change "<name>" --json`

   b. **Task completion** from `openspec/changes/<name>/tasks.md`

   c. **Delta specs** from `openspec/changes/<name>/specs/`

   d. **Release impact type** from `openspec/changes/<name>/proposal.md`:

   ```markdown
   ## Release Impact

   Type: <patch|minor|major>
   Rationale: <short reason>
   ```

   Rules:
   - If `Type` is present and valid, use it
   - If missing/invalid, ask user explicitly for that change
   - Never guess

4. **Detect spec conflicts**

   Build `capability -> [changes]` map.
   Conflict exists when 2+ selected changes touch the same capability spec.

5. **Resolve conflicts agentically**

   For each conflict:
   - Read conflicting delta specs
   - Verify implementation evidence in codebase
   - Decide sync order:
     - one implemented -> sync that one
     - both implemented -> chronological order (older first)
     - neither implemented -> skip sync with warning

6. **Show consolidated status table**

   Include at least:
   - Change
   - Artifacts
   - Tasks
   - Specs/conflicts
   - Release impact (`patch|minor|major`)
   - Status

7. **Confirm batch operation**

   Ask once using **AskUserQuestion tool**:
   - Archive all selected
   - Archive only ready changes
   - Cancel

8. **Execute archive per confirmed change**

   For each confirmed change:
   - Sync specs when applicable
   - Move to `openspec/changes/archive/YYYY-MM-DD-<name>`
   - Track per-change result (success/failed/skipped)

9. **Compute aggregate release bump**

   Use only successfully archived changes.

   Aggregate rule (highest wins):
   - any `major` -> aggregate `major`
   - else any `minor` -> aggregate `minor`
   - else aggregate `patch`

   If no successful archives, skip release automation.

10. **Create or reuse PR automatically**

   - Ensure archive changes are committed
   - Push current branch
   - Resolve base from repository default branch
   - Reuse existing open PR for `<head> -> <base>` when present
   - Else create PR automatically

   PR recommendations:
   - Title: `release: <next-tag> - bulk archive`
   - Body includes:
     - archived changes list
     - per-change release impact
     - aggregate release impact
     - sync/conflict summary
     - warnings/failures

11. **Create and push semantic tag automatically**

   - Compute next tag from latest `v*.*.*` using aggregate release impact
   - Create annotated tag on current `HEAD`
   - Push to `origin`

   Collision handling:
   - Refetch tags
   - Recompute once
   - Retry once
   - Stop and report if still colliding

12. **Display summary**

   Include:
   - Archived changes
   - Skipped/failed changes
   - Spec sync/conflict summary
   - Aggregate release impact
   - Created tag
   - PR URL

**Output On Success**

```markdown
## Bulk Archive Complete

Archived N changes:
- <change-1> -> archive/YYYY-MM-DD-<change-1>/
- <change-2> -> archive/YYYY-MM-DD-<change-2>/

Release impact (aggregate): <patch|minor|major>
Tag: <vX.Y.Z>
Pull request: <url>
```

**Output On Partial Success**

```markdown
## Bulk Archive Complete (partial)

Archived N changes:
- <change-1> -> archive/YYYY-MM-DD-<change-1>/

Skipped M changes:
- <change-2> (reason)

Failed K changes:
- <change-3>: <error>

Release impact (aggregate): <patch|minor|major>
Tag: <vX.Y.Z>
Pull request: <url>
```

**Output When No Changes**

```markdown
## No Changes to Archive

No active changes found. Use `/opsx:new` to create a new change.
```

**Guardrails**
- Always prompt for selection
- Never guess release impact type for any change
- Use highest impact rule (`major > minor > patch`) for bulk tag
- Do not create tag if PR creation failed
- If zero changes were archived successfully, skip PR/tag automation
- Preserve `.openspec.yaml` when moving change directories
