# UI Style Literal Inventory (Task 5.1)

## Scope
- `src/FamilyFinances.Web/Components/**/*.razor`
- `src/FamilyFinances.Web/Components/**/*.razor.css`

## Classification

### Tokenize
- Loading containers with `min-height` literals moved to shared classes (`ff-loading-shell`, `ff-loading-shell-sm`).
- Spinner size literals moved to shared classes (`ff-spinner-lg`, `ff-spinner-md`) backed by token values.
- Static click affordance (`cursor: pointer`) moved to shared class (`ff-clickable`).
- Static modal overlay color moved to shared class (`ff-modal-overlay`).
- Static list scroll constraints moved to shared class (`ff-scroll-y-300`).
- Static input minimum width moved to shared class (`ff-input-min-w-200`).
- Static report/history/transactions/accounts table width literals moved to width utility classes (`ff-w-*`, `ff-w-*p`).
- Static compact progress container dimensions moved to class (`ff-progress-compact`).
- Static tiny heading font size moved to class (`ff-text-xxs`).

### Keep-as-dynamic
- `AccountGroupTotalsPage.razor`: progress bar width kept dynamic via CSS custom property assignment `style="--ff-progress-width:@(percentage)%"` + class `ff-progress-fill`.
- `AnnualCompositionChart.razor`: slice color kept dynamic via CSS custom property assignment `style="--ff-slice-color:{slice.ColorHex}"` + class `composition-dot`.

### Delete
- No direct deletions required after token/class migration.

## Result Snapshot
- Remaining inline `style=` usages in Razor: 2 (both dynamic custom-property assignments above).
