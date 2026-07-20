# Mission Progress Inspector Design

## Goal

Make the temporary kitchen mission flow observable in the Unity Inspector without
requiring a debugger breakpoint.

## Scope

- Add a custom Inspector for `GameProgressManager`.
- Retain the existing editable `Initial State` field.
- Show `Current State` as a disabled, read-only field while playing.
- Show a clear unavailable value outside Play Mode, because no runtime state has
  been initialized then.
- Do not alter save data, gameplay state transitions, or runtime UI.

## Design

An editor-only `GameProgressManagerEditor` draws the standard serialized fields,
then draws `Current State` in a disabled control. It reads the public
`CurrentState` property directly from the inspected runtime object, so the value
updates whenever the Inspector repaints during Play Mode.

The runtime `GameProgressManager` remains unchanged. In particular, `CurrentState`
is not serialized: serializing it would blur the distinction between the configured
initial state and the active mission state.

## Verification

An editor test will verify the custom Inspector exposes the expected read-only
current-state property. Existing mission tests will continue to verify kitchen entry
and refrigerator inspection state transitions. Manual verification will confirm the
field changes from `FindKitchen` to `InspectRefrigerator` when the player enters the
kitchen trigger.
