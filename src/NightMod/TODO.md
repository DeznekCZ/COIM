# NightMod - TODO

## Latitude-based sun & moon position (in progress)

Approach chosen: **simplified latitude model** (not a full ephemeris).

Done:
- `SolarSky.cs` - pure-math core. Computes sun elevation/azimuth from latitude,
  in-game day-of-year and time-of-day via the standard solar equations, and the
  moon as the same arc shifted by the lunar-month phase (drifts ~one step per
  in-game day, laps once per 30-day in-game month).
- `config.json` / `NightModSettings` - `latitude` and `longitude` options,
  defaulting to a northern-Africa zone, player-overridable.

Remaining (do once the project builds again - see headlights note below):
- Wire `SolarSky` into `NightCycleRenderer`: replace `CycleCurve.SunElevation` /
  `CycleCurve.MoonElevation` with `SolarSky.SunPosition` / `MoonPosition`.
  - Needs the in-game date (day-of-year, day-of-month) - inject `ICalendar` into
    the renderer, or expose the date from `NightCycleManager`.
  - Map the cycle position to `timeOfDay` (0..1, 0.5 = solar noon).
- Fade the moon out during daytime ("not very visible during the day") - scale
  moon visibility down when the sun is above the horizon.
- Calendar: 30-day months, 360-day year (already baked into `SolarSky`).

## HUD bar via Mafi UI - DONE (window content remains)

Done: `NightCycleWindow : Window` + `NightCycleWindowController : WindowController<>`.
The controller (a `[GlobalDependency]`) docks a horizontal day-night bar into the
status bar; the bar is recolored each frame to the current cycle color and, when
clicked, calls `ActivateSelf()` to open the window. The IMGUI `NightCycleHudBar`
is removed.

Remaining for the window:
- Host the mod settings panel: `new ModJsonConfigPanel(NightModSettings.Config)`
  inside `NightCycleWindow` (the panel type is public).
- Add a current date/time readout to the window.

--- original notes kept for reference ---
## HUD bar via Mafi UI (old notes)

The time-of-day bar must be a proper Mafi-UI component, not the IMGUI
`NightCycleHudBar` (a temporary placeholder). All pieces are now researched:

Docking - follow the proven pattern (see COIM_ProgramableNetwork's
`VariableHudDisplay`):
- New class, `[GlobalDependency(RegistrationMode.AsSelf)]`, ctor takes `UiContext`.
- `context.UiRoot.Schedule.Execute(...).Every(1000).Until(...)` to wait for the
  `StatusBarContainer` element, then build + dock the bar (navigate
  StatusBarContainer -> Row -> StatusBar -> Column, `.Add(bar)`), or fall back to
  `UiRoot.AddComponent` with `AbsolutePosition`.

Bar visual:
- A `PanelRow` / `Row` with a colored fill child; recolor + resize the fill each
  frame from `context.GameLoopEvents.RenderUpdate` so the colored background moves.

Click -> config window:
- BLOCKER: the game's `ModConfigWindow` is a `private` nested class - a mod cannot
  instantiate it.
- Workaround: build our own `Mafi.Unity...Window` subclass that hosts a
  `ModJsonConfigPanel(JsonConfig)` (that panel type IS public). The bar's
  `.OnClick(...)` opens our window. (Or, simplest stopgap: `OnClick` -> the
  game escape menu via `IUnityInputMgr.OpenGameMenu`.)

Finally: delete the IMGUI `NightCycleHudBar`.

## Horizon mask (fake night shadow) - DONE

`NightHorizonMask.cs` - a large invisible quad at sea level (Unity Y 0), sized to the
terrain plus a ~1 km margin per side, centered on the terrain. It is a pure
`ShadowCastingMode.ShadowsOnly` caster (never drawn, so it can never peek through the
ocean floor). The sun keeps casting shadows at every elevation; once the sun is below
the horizon its light hits this mask from beneath and the mask casts one map-wide
shadow, so night darkness is real occlusion and the old stray-fragment problem is gone.
`NightCycleRenderer` builds it from `TerrainManager.TerrainWidth/Height` and switches it
on only while the sun is below `MaskActivationElevation` (5 deg).

Known edge case: terrain dug below sea level (deep pits) sits below the mask and would
catch a little night light - accepted for now.

## Control-HUD options

Link options into the game's control HUD:
- Toggle to hide / show the day-night cycle live.
- Set a specific time of day (freeze the cycle).
- Choose the zone / location (latitude-longitude preset).
Defaults come from `config.json`. `sun_azimuth` is already a config option.

## Vehicle headlights

`NightHeadlights` is a work in progress - being completed in a separate session.
