---
name: openspec-archive-change
description: Archive a completed change in the experimental workflow. Use when the user wants to finalize and archive a change after implementation is complete.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1"
---

Archive a completed change in the experimental workflow.

**Input**: Optionally specify a change name. If omitted, infer from context only when unambiguous; otherwise prompt for selection.

**Steps**

1. **If no change name provided, prompt for selection**

   Run `openspec list --json` and use the **AskUserQuestion tool** to select a change.

   Show only active changes (not archived).
   Include schema used for each change when available.

   **IMPORTANT**: Do NOT auto-select.

2. **Check artifact completion status**

   Run `openspec status --change "<name>" --json`.

   Parse:
   - `schemaName`
   - `artifacts` list (`done` or other)

   If any artifacts are not `done`:
   - Show warning listing incomplete artifacts
   - Ask for confirmation
   - Continue only if confirmed

3. **Check task completion status**

   Read tasks file (`openspec/changes/<name>/tasks.md` when present).

   Count:
   - Incomplete: `- [ ]`
   - Complete: `- [x]`

   If incomplete tasks exist:
   - Show warning
   - Ask for confirmation
   - Continue only if confirmed

4. **Assess delta spec sync state**

   Check delta specs at `openspec/changes/<name>/specs/`.

   If delta specs exist:
   - Compare with `openspec/specs/<capability>/spec.md`
   - Summarize changes before prompting

   Prompt options:
   - If changes needed: "Sync now (recommended)", "Archive without syncing"
   - If already synced: "Archive now", "Sync anyway", "Cancel"

   If user chooses sync, execute `/opsx:sync` logic (openspec-sync-specs skill).

5. **Perform the archive**

   Create archive directory when needed:
   ```bash
   mkdir -p openspec/changes/archive
   ```

   Target format: `YYYY-MM-DD-<change-name>`.

   If target exists, stop with error.
   Otherwise move:
   ```bash
   mv openspec/changes/<name> openspec/changes/archive/YYYY-MM-DD-<name>
   ```

6. **Determine release impact from proposal markdown**

   Read `openspec/changes/archive/YYYY-MM-DD-<name>/proposal.md` and require:

   ```markdown
   ## Release Impact

   Type: <patch|minor|major>
   Rationale: <short reason>
   ```

   Rules:
   - Accept only `patch`, `minor`, or `major`
   - If missing/invalid, ask user explicitly
   - Do not guess

7. **Compute next semantic tag**

   Read latest tag matching `v*.*.*` and apply bump:
   - `major`: `vX.Y.Z` -> `v(X+1).0.0`
   - `minor`: `vX.Y.Z` -> `vX.(Y+1).0`
   - `patch`: `vX.Y.Z` -> `vX.Y.(Z+1)`

   If no prior tags exist:
   - `major` -> `v1.0.0`
   - `minor` -> `v0.1.0`
   - `patch` -> `v0.0.1`

8. **Prepare git branch and create PR automatically**

   - Ensure archive changes are committed
   - Push current branch
   - Resolve base branch from repository default branch
   - Reuse existing open PR for `<head> -> <base>` when present
   - Otherwise create PR automatically

   PR recommendation:
   - Title: `release: <next-tag> - <change-name>`
   - Body includes archive path, release impact, sync status, and warnings

9. **Create and push tag automatically**

   Only after PR exists:
   - Create annotated tag on current `HEAD`
   - Push tag to `origin`

   If tag collision occurs remotely:
   - Refetch tags
   - Recompute once
   - Retry once
   - If still collides, stop and report

10. **Display summary**

   Include:
   - Change name
   - Schema
   - Archive location
   - Spec sync status
   - Release impact type
   - Created tag
   - PR URL
   - Warnings

**Output On Success**

```markdown
## Archive Complete

**Change:** <change-name>
**Schema:** <schema-name>
**Archived to:** openspec/changes/archive/YYYY-MM-DD-<name>/
**Specs:** Synced to main specs (or No delta specs / Sync skipped)
**Release impact:** <patch|minor|major>
**Tag:** <vX.Y.Z>
**Pull request:** <url>
```

**Guardrails**
- Always prompt for selection when change name is missing
- Use artifact graph (`openspec status --json`) for completion checks
- Do not block archive on warnings unless user cancels
- Preserve `.openspec.yaml` by moving the whole change directory
- Release type must come from markdown or explicit user choice
- Do not guess release type
- Do not create tag if PR creation failed
- Final summary must include PR URL and tag
