# Session Handoff

Updated: 2026-07-20

## Current repository state

- Repository: `C:\Users\vkccl\Desktop\Github\2026DGJ-2`
- Current branch: `feature-kitchen_refrigerator_interaction-24`
- Base: latest `develop` at `1a030f9` (`Merge pull request #23 from rlaehgjs/revert/ui-pr-18`)
- GitHub issue: #24, `Feat: 주방 진입 및 냉장고 조사 미션 진행`
  - https://github.com/rlaehgjs/2026DGJ-2/issues/24
- No implementation code or scene changes have been made for #24.
- No commit has been made on this branch.

## Uncommitted documents

- `docs/superpowers/specs/2026-07-20-kitchen-refrigerator-interaction-design.md`
  - Design proposal for the first mission slice.
- This handoff file.

Do not commit either document until the user explicitly approves it.

## Approved issue scope

Implement only this temporary-object mission slice in `InteractionSandbox`:

`FindKitchen -> InspectRefrigerator -> FindFrontDoorKey`

Included:

- Kitchen entry trigger
- Refrigerator Cube inspected using F through `PlayerInteraction` and `IInteractable`
- Progress-state updates and save-version handling
- InteractionSandbox setup and focused automated/manual tests

Excluded:

- Front-door key pickup, doors, generator, repair, drawers, shortcuts, freezer, environment zones
- MainGameScene and final models
- ObjectiveUI: it is currently absent from `develop` because UI PR #18 was reverted by merged PR #23. UI reintroduction must be a separate later integration.

## Key implementation facts discovered

- `GameInputReader.InteractPressed` emits F input.
- `PlayerInteraction` performs a central-camera Raycast and calls `IInteractable.CanInteract` then `Interact`.
- `PickupInteractable` is trigger-only automatic pickup. Do not add inspect/repair/door responsibilities to it.
- No class currently implements `IInteractable`.
- Existing progress starts `FindKitchenKey -> InspectRefrigerator -> FindGenerator`; the target flow needs `FindKitchen -> InspectRefrigerator -> FindFrontDoorKey -> FindGenerator`.
- `TryCompleteRefrigeratorInspection()` already exists and must remain public, but its next state needs to become `FindFrontDoorKey`.
- A new, minimal `TryCompleteKitchenArrival()` API is needed.
- `GameSaveData` stores enum values numerically. The design proposes version 2 migration:
  - old `FindKitchenKey` (0) -> new `FindKitchen`
  - old `InspectRefrigerator` (1) -> unchanged
  - old `FindGenerator` and later -> equivalent state shifted by one
- `SaveManager` has no persistent world-object API; that work is out of scope for #24.
- `InteractionSandbox` currently has only Plane, automatic pickup Cube, Heal_Test, and a SaveManager with no PlayerInventory reference. It lacks Player(Test) and GameProgressManager.
- `MainGameScene` currently overrides GameProgressManager initial state to enum value 2. MainGameScene is out of scope for this issue.

## Required gate before implementation

The user asked to review the design document directly before implementation. Wait for explicit approval of:

`docs/superpowers/specs/2026-07-20-kitchen-refrigerator-interaction-design.md`

After approval, create the detailed implementation plan, then implement and test only issue #24 scope.
