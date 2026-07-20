# Mission Progress Debug Logging Design

## Goal

Make the temporary kitchen mission flow observable from the Unity Console without
requiring a debugger breakpoint.

## Scope

- Log kitchen-trigger entry, including ignored entries caused by a missing progress
  manager or player inventory.
- Log every accepted and rejected kitchen-arrival or refrigerator-inspection
  transition with the expected and current mission states.
- Log refrigerator interaction attempts and whether they are currently allowed.
- Use the existing `Debug.Log` API only; do not add UI, save-data fields, or
  per-frame logging.

## Design

`KitchenArrivalTrigger` logs the physical trigger boundary: what entered and
whether it resolves to a player inventory. `InspectInteractable` logs the
interaction boundary: whether the refrigerator accepts the `F` interaction.

`GameProgressManager` logs the state-machine boundary. It records rejected
expected-state transitions and successful state changes, so the Console shows
both the cause and the actual before/after state.

All messages share the `[MissionProgress]` prefix to make them filterable in the
Unity Console.

## Verification

Editor tests will assert that kitchen entry and refrigerator inspection still
advance the existing state machine. Manual verification will confirm the Console
includes the trigger and state-transition messages in the current sandbox scene.
