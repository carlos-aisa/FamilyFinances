---
description: Archive a completed change in the experimental workflow
---

Archive a completed change in the experimental workflow.

**Input**: Optionally specify a change name after `/opsx:archive` (for example, `/opsx:archive add-auth`). If omitted, infer from context only when unambiguous; otherwise prompt for selection.

**Steps**

1. **If no change name is provided, prompt for selection**

   Run `openspec list --json` to get available changes. Use the **AskUserQuestion tool** to let the user select.

   Show only active changes (not already archived).
   Include schema used for each change when available.

   **IMPORTANT**: Do NOT guess or auto-select.

2. **Check artifact completion status**

   Run `openspec status --change "<name>" --json`.

   Parse:
   - `schemaName`
   - `artifacts` with status (`done` or other)

   **If any artifacts are not `done`:**
   - Show a warning listing incomplete artifacts
   - Ask for confirmation
   - Proceed only if confirmed

3. **Check task completion status**

   Read `tasks.md` (typically `openspec/changes/<name>/tasks.md`).

   Count:
   - Incomplete tasks: `- [ ]`
   - Complete tasks: `- [x]`

   **If incomplete tasks exist:**
   - Show warning with counts
   - Ask for confirmation
   - Proceed only if confirmed

   If no tasks file exists, proceed without task warning.

4. **Assess delta spec sync state**

   Check for delta specs at `openspec/changes/<name>/specs/`.

   If delta specs exist:
   - Compare each delta spec with corresponding main spec at `openspec/specs/<capability>/spec.md`
   - Summarize adds/modifications/removals/renames

   **Prompt options:**
   - If changes are needed: "Sync now (recommended)", "Archive without syncing"
   - If already synced: "Archive now", "Sync anyway", "Cancel"

   If user chooses sync, execute `/opsx:sync` logic. Continue archive regardless of sync choice.

5. **Perform the archive**

   Create archive directory if needed:
   ```bash
   mkdir -p openspec/changes/archive
   ```

   Generate target name using current date: `YYYY-MM-DD-<change-name>`.

   If target already exists:
   - Stop with error
   - Suggest: rename existing archive or use different date

   Otherwise move the directory:
   ```bash
   mv openspec/changes/<name> openspec/changes/archive/YYYY-MM-DD-<name>
   ```

6. **Determine release type (semantic bump) from markdown**

   Read `openspec/changes/archive/YYYY-MM-DD-<name>/proposal.md` and look for:

   ```markdown
   ## Release Impact

   Type: <patch|minor|major>
   Rationale: <short reason>
   ```

   Resolution rules:
   - Use `Type` when valid (`patch`, `minor`, `major`)
   - If section is missing/invalid, ask user to choose explicitly
   - Never guess the release type

   SemVer bump rules:
   - `major`: `vX.Y.Z` -> `v(X+1).0.0`
   - `minor`: `vX.Y.Z` -> `vX.(Y+1).0`
   - `patch`: `vX.Y.Z` -> `vX.Y.(Z+1)`

   If no prior semver tags exist:
   - `major` -> `v1.0.0`
   - `minor` -> `v0.1.0`
   - `patch` -> `v0.0.1`

7. **Compute next tag and prepare git state**

   - Fetch tags and read latest semantic tag matching `v*.*.*`
   - Compute next tag from bump type
   - Ensure archive changes are committed (if not, stage and commit)
   - Commit message recommendation: `chore(openspec): archive <change-name>`

8. **Create pull request automatically**

   - Determine base branch from repository default branch (usually `main`)
   - Push current branch
   - Check whether an open PR already exists for `<head-branch> -> <base-branch>`
   - If not, create PR automatically

   PR recommendations:
   - Title: `release: <next-tag> - <change-name>`
   - Body includes:
     - Archived change path
     - Release impact type and rationale
     - Sync status
     - Warnings (if any)

   Preferred creation method:
   - Use GitHub connector/CLI when available
   - Otherwise use GitHub API

9. **Create and push tag automatically**

   Only after PR exists (new or existing):
   - Create annotated tag on current `HEAD`
   - Push tag to `origin`

   Tag message recommendation:
   - `Release <next-tag> (<change-name>, <release-type>)`

   If tag already exists remotely:
   - Refetch tags
   - Recompute next tag once
   - Retry once
   - If still collides, stop and report

10. **Display summary**

   Show:
   - Change name
   - Schema
   - Archive location
   - Spec sync status
   - Release type (`patch|minor|major`)
   - Created tag
   - PR URL
   - Any warnings

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

All artifacts complete. All tasks complete.
```

**Output On Success With Warnings**

```markdown
## Archive Complete (with warnings)

**Change:** <change-name>
**Schema:** <schema-name>
**Archived to:** openspec/changes/archive/YYYY-MM-DD-<name>/
**Specs:** Sync skipped
**Release impact:** <patch|minor|major>
**Tag:** <vX.Y.Z>
**Pull request:** <url>

**Warnings:**
- <warning 1>
- <warning 2>
```

**Output On Error (Archive Exists)**

```markdown
## Archive Failed

**Change:** <change-name>
**Target:** openspec/changes/archive/YYYY-MM-DD-<name>/

Target archive directory already exists.
```

**Guardrails**
- Always prompt for change selection if missing
- Use `openspec status --json` for artifact completion
- Do not block archive on warnings; block only on explicit user cancellation
- Preserve `.openspec.yaml` by moving the entire directory
- Release type must come from markdown (`proposal.md`) or explicit user choice
- Do not guess release type
- Do not create tag if PR creation fails
- Summary must include both PR URL and created tag
