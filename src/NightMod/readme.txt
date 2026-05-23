Night Mod - Day & Night Cycle
=============================

NightMod adds a configurable day-to-night cycle to Captain of Industry.
It does NOT add new weather types or change which weather the game picks.
Instead it dims the lighting of the game's original weather conditions on a
repeating cycle, and (optionally) makes solar panels follow that cycle.

How to install
--------------

1. Locate your COI data folder. By default it is at:
   %APPDATA%/Captain of Industry

2. Open or create the "Mods" directory inside it.

3. Extract the contents of this zip into the "Mods" directory.
   You should end up with:
   Mods/NightMod/manifest.json
   Mods/NightMod/NightMod.dll
   Mods/NightMod/config.json

4. Launch the game and select the mod when creating a new game, or add it to
   an existing save.

Configuration
-------------

Settings can be changed in the in-game mod settings UI (config.json):

  enabled               - master on/off switch for the cycle
  cycle_length_minutes  - real-time length of one full day->night->day cycle
  min_brightness        - how dark the deepest night gets (0 = darkest)
  affect_solar          - whether solar panel output follows the cycle
  vehicle_headlights    - whether vehicles cast headlight cones at night

The config options appear automatically in the in-game Settings menu
(Settings -> Mods), so they can be changed without editing this file.

5. In case of any errors, examine logs in:
   %APPDATA%/Captain of Industry/Logs
