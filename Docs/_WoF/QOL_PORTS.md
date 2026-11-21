# Quality-of-life ports from Wolfgate

Source: `VanguardControl/Wolfgate` @ `65c86b39` (Nov 20 2026), a Monolith/Frontier fork. Its fork-specific code
lives in `Content.*/_WG/<Feature>`, and its changelog is `Resources/Changelog/Wolfgate.yml`.

## Before porting

- **License.** Wolfgate code added after Frontier commit `2fca06ea` is AGPL-3.0, the same as Einstein Engines, so ports are allowed. Keep the author credit and license header, and note the source in the changelog.
  - Code in `_Starlight` folders needs Starlight attribution.
  - Goonstation-derived assets are CC-BY-NC-SA. They're fine for a non-commercial server, but keep track of them.
- **Engine gap.** Wolfgate runs engine 277.x on .NET 10, and EE is on 267.3 on .NET 9. Ports are rewrites against EE's APIs, not copy-paste. Smaller UI and chat features move easily. Medical (Wolfmed) and anything that touches ships does not.
- **EE already has some of these:**
  - offering items to another player (`OfferItem`)
  - a custom species name field
  - height and width sliders
  - typing indicators
  - Shitmed limb surgery
- **Audience.** Wings of Fire readers skew young.
  - Wolfgate's adult content stays out: genitals, the anatomy tab, adult descriptions, undergarment consent, street drugs.
  - Its executions feature is a judgment call next to the GDD's gore goal.
  - Settle the server's age policy before opening.

## Port first (cheap, big RP payoff)

| Feature | Wolfgate module | Why it fits WoF |
|---|---|---|
| Subtle emotes: 1.5-tile range, hidden from ghosts, own channel with `%` | `Server/_WG/Subtle` | Court intrigue and quiet conversations |
| Subtle OOC with `&` | Changelog Oct 28 (look in chat code) | OOC between neighbors without breaking the scene |
| Emote shorthand (`*bow` becomes "Clay bows.") | `Server/_WG/Emotes` | Fast RP. Add dragon emotes: wing flare, tail lash, hiss |
| Basic formatting in speech and emotes | `Server/_WG/ChatFormatting` | |
| Examine card: photo, status, visible wounds, worn items, description; notice when someone looks you over | `_WG/ExamineCard` | Replaces chat spam and works with wound gore |
| Character headshots | `_WG/Headshot` | Shows on the examine card and in court records |
| Character editor: drag-rotate and zoom preview, floor tile, unsaved-changes prompt, 4096-character descriptions | `Client/_WG/CharacterPreview` | Dragon players spend a long time on looks |
| Per-character voice: message sound, typing bubble, chat font, scream and laugh | `_WG/Voice`, `TypingBubble`, `ChatFont`, `EmoteVoices` | Tribe-flavored voices |
| Show or hide markings mid-round | `_WG/MarkingToggles` | Useful later for RainWing camouflage |
| New-player tag under 8 hours, welcome popup, hide option | `_WG/Newbie` | Cuts newbie-hunting |
| Ahelp Resolve button, no repeated bwoink after a staff reply | `_WG/AHelpResolve` | Admin sanity |
| Save chat log button | Changelog Nov 15 | |
| Search bar in Settings | `Client/_WG/Options` | |
| Screentips: name of what's under the cursor | `Client/_WG/Screentips` | Helps people learn a re-themed map |
| Pixel shift, tilt, and turn without moving | `Client/_WG/PixelShift`, `Input` | RP posing. Perching dragons |
| Keys to swap straight to left or right hand, and to cancel your own action | `Client/_WG/Input`, `ItemSwitch` | |
| Craft x2, x5, x10, All | Changelog Sep 24 | Farmers and cooks batch-craft |
| Jukebox and boombox volume slider | `Client/_WG/JukeboxVolume` | |

## Port second (bigger, still strong fits)

| Feature | Wolfgate module | WoF use |
|---|---|---|
| Contract boards with per-faction offers and staged quests | `Server/_WG/Contracts` | The GDD's expedition and job board |
| NPC traders across a counter, cash or ID, receipts | `_WG/Traders` | Market stalls and the trade quarter |
| Paintings plus an Art Catalogue kept between rounds | `_WG/Paintings` | Scribes, scrolls, royal portraits |
| Instrument menu overhaul and MIDI library | Changelog Oct 21 | Bards, and the Harpy singer code |
| Staff appear in person as DM-NPCs, admin observer kit | `_WG/EventStaff`, `AdminObserver` | Queen's envoys and story events run by staff |
| Admin tools: drag a ghost into a body, spawn as outfit, offer a mob to ghosts | `_WG/Administration` | Running kingdom events |
| Ghost orbit menu, event notice chips, warp list sorted by place | `_WG/Ghost` | |
| Weather, day and night | `_WG/WolfWeather` | Rainforest storms, desert heat |
| Roofs | `Client/_WG/Roofs` | Pairs with high-altitude flight over buildings |
| Mapping mode: private sessions, map library, export | `_WG/MappingMode` | Community map building for three kingdoms |

## Compare before porting

- **Wolfmed.** Per-part wounds, pain, infection, arterial bleeds, brain-death and CPR. It overlaps with EE's Shitmed. Wolfmed is deeper and very good for the gore goal, but it's a large rewrite. Prototype wing fractures on Shitmed first, then decide.

## Skip

- Anything to do with ships, FTL, radar, factions or space economy.
- Adult and anatomy features.
- Street drugs.
- Doom and MAME arcades.
