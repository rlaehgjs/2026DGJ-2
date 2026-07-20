# Kitchen Entry and Refrigerator Inspection Design

Issue: #24

## Goal

Implement and verify the first temporary-object mission slice in `InteractionSandbox`:

`FindKitchen -> InspectRefrigerator -> FindFrontDoorKey`

The player enters a kitchen trigger, then looks at a refrigerator Cube and presses F. The interaction must go through the existing `PlayerInteraction` and `IInteractable` contract. No pickup, door, repair, generator, UI reintroduction, or MainGameScene integration is included.

## State and save design

`GameProgressState` will use this order:

1. `FindKitchen`
2. `InspectRefrigerator`
3. `FindFrontDoorKey`
4. Existing generator and later states, in their existing order

The existing `TryCompleteRefrigeratorInspection()` API remains public and will advance from `InspectRefrigerator` to `FindFrontDoorKey`. A new `TryCompleteKitchenArrival()` API is required because no current completion request represents kitchen entry. A new `TryCompleteFrontDoorKey()` may be added for the later pickup work, but this issue will not call it.

Existing version-1 saves store enum values from the older sequence. `GameSaveData.CurrentVersion` will become 2. Before restore, `SaveManager` will migrate version-1 progress values as follows:

| Version-1 state | Version-2 state |
| --- | --- |
| `FindKitchenKey` (0) | `FindKitchen` |
| `InspectRefrigerator` (1) | `InspectRefrigerator` |
| `FindGenerator` and every later state | The equivalent state shifted by one position |

Invalid legacy values fall back to `FindKitchen`. This preserves later mission progress while safely restarting the ambiguous former first objective. The migration is private to `SaveManager`; no caller uses PlayerPrefs directly.

## Components

### KitchenArrivalTrigger

A small trigger component receives an explicit `GameProgressManager` reference. On a PlayerInventory-containing collider entering the trigger, it requests `TryCompleteKitchenArrival()`. It does not modify progression fields directly, does not save directly, and does nothing when the current state is not `FindKitchen`.

### InspectInteractable

`InspectInteractable` implements `IInteractable` and receives an explicit `GameProgressManager` reference.

- `CanInteract` returns true only while the manager state is `InspectRefrigerator`.
- `Interact` calls `TryCompleteRefrigeratorInspection()`.
- It has no pickup, inventory consumption, door, or repair behavior.

The first slice has no one-shot visual event. Therefore restore never invokes an inspection effect: after restore, the manager state makes `CanInteract` false and a repeated F press cannot advance the mission again.

## Sandbox setup

`InteractionSandbox` will contain:

- `Player(Test)` with its existing input, interaction camera, and inventory
- a GameProgressManager instance starting at `FindKitchen`
- a SaveManager instance whose PlayerInventory and GameProgressManager references point to the sandbox instances
- a trigger Cube for kitchen arrival
- a non-trigger refrigerator Cube with a collider and `InspectInteractable`

The existing automatic pickup Cube and Heal_Test are not changed by this issue.

## Testing

Automated tests will cover:

1. Kitchen trigger advances only `FindKitchen` to `InspectRefrigerator`.
2. Refrigerator inspection advances only `InspectRefrigerator` to `FindFrontDoorKey`.
3. Inspection attempted in another state does not change progress.
4. Migrating version-1 save data maps states as documented.
5. Restoring at `FindFrontDoorKey` does not allow a second refrigerator completion.

Manual `InteractionSandbox` verification will cover F-key raycast distance and direction, kitchen entry, refrigerator inspection, save/restart restoration, and preservation of existing automatic pickup behavior.

## Explicit exclusions

- Front-door key pickup, doors, generator, wire, repair, drawers, shortcuts, freezer, and environment zones
- Persistent per-world-object state APIs
- ObjectiveUI implementation or binding. ObjectiveUI is absent from current `develop` after the UI rollback and must be integrated in a later UI task.
- MainGameScene placement and final models
