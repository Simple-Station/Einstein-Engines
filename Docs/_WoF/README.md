# Wings of Fire: Kingdoms of Pyrrhia: vertical slice, step 1

Built on Einstein Engines (`Simple-Station/Einstein-Engines` @ `e37dbff4`, engine 267.3.0, .NET 9).
Everything new lives in `_WoF` folders. Upstream edits are marked `// WoF start` / `// WoF end`.

## What's in this drop

| Area | Files | Notes |
|---|---|---|
| Dragon flight | `Content.Shared/_WoF/Flight/*`, `Content.Client/_WoF/Flight/*` | Altitude bands built on EE's harpy `FlightComponent` |
| Upstream edit | `Content.Shared/Flight/Events.cs`, `SharedFlightSystem.cs` | Adds and raises `FlightToggledEvent` (5 lines) |
| Tribes | `Resources/Prototypes/_WoF/Species`, `Entities/Mobs/Species`, `Body`, `Markings`, `Datasets` | RainWing, NightWing, MudWing |
| Jobs | `Resources/Prototypes/_WoF/Roles` | 12 slice jobs, 6 departments, playtime trackers |
| Test map | `Resources/Prototypes/_WoF/Maps/wof_test_court.yml` | Pebble station with only kingdom jobs |
| Text | `Resources/Locale/en-US/_WoF/*.ftl` | |

## How flight works

- **Taking off.** Use EE's existing **Fly** action. The dragon goes to **Low** altitude.
- **Climb and Descend actions.** Climb moves Low to **High**. Descend moves High to Low, and from Low it lands.
- **Low altitude.** Flies over tables, crates, railings and half walls. Walls and doors still block it, so flight never gets around security.
- **High altitude.** 35% faster, but you can't pick things up or use anything that isn't on you.
- **Falling.** Flight can end without a landing: stunned, knocked down, asleep, crit or dead, or the wing is lost or disabled. Falling from High does 20 Blunt and from Low does 4.
- **Wing injuries.** Flight is granted by the winged right arm (`WoFRightArmDragon`). If the arm is severed or disabled by damage, flight goes away. The existing limb surgery system gives broken-wing gameplay for free.
- **Stamina.** Normal flying costs no stamina (`staminaDrainRate: 0`).
- **Tribe handling.** Each tribe flies differently through its weightless movement values (see the comments in `Entities/Mobs/Species/dragons.yml`):
  - RainWing: agile and precise
  - NightWing: fast, glides well
  - MudWing: heavy, slow to turn
- **Visuals.** The client lifts the sprite with altitude. Ground shadow, banking and wind effects are still to do.
- **Bug fix.** EE's base flight system doesn't land you when you go crit or die. The dragon system handles that, so a downed dragon falls.

## Placeholders to replace

- **Sprites.** Bodies are reptilian sprites with bat-wing and lizard markings. Original dragon art is the biggest art task. Don't trace official Wings of Fire art.
- **Gear, icons and access.** Borrowed from station jobs (CaptainGear, JobIconChef, `Kitchen` access and so on).
- **Map.** The test map is Pebble station.
- **Action icons.** Climb and Descend use the harpy flight icons.

## Trying it

1. Copy the files into your EE checkout, or apply the patch. Then build as usual (`dotnet build`).
2. In `server_config.toml`, under `[game]`, set `map_pool = "WoFSliceMapPool"`.
3. Make a RainWing, NightWing or MudWing in the character editor and pick a kingdom job.
4. Run the integration tests. Prototype loading and the YAML linter will catch anything the offline check missed.

## Not verified yet

- **Not compiled.** This was written without a .NET SDK. Every prototype reference was checked against the full EE prototype set (65 prototypes, 0 missing IDs), and the C# was checked against EE and engine source. Still, build and run the tests before merging.
- **Client prediction.** Altitude changes run on the server only. Expect a small snap when you take off over a table under lag. Shared prediction is the next step.

## Next steps (from the GDD's Prototype 1 and 2)

1. Build and playtest the flight feel, then tune the per-tribe numbers.
2. Ground shadow and a smooth altitude transition (client).
3. Tribe abilities for the slice:
   - RainWing camouflage and venom
   - NightWing dark vision
   - MudWing mud-bath healing and fire resistance
4. A farm-to-kitchen supply chain, using the existing botany and kitchen systems re-themed.
5. Rainforest Court map with canopy platforms.
6. Quality-of-life ports from Wolfgate (see `QOL_PORTS.md`).
