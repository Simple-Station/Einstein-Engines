[33mcommit 3cbb9a73e82a9b2a91c1b139ddf54d6358339d5b[m[33m ([m[1;36mHEAD -> [m[1;32mNukeSmallRevork[m[33m, [m[1;31morigin/NukeSmallRevork[m[33m)[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Thu May 23 22:11:53 2024 +0300

    final?

[33mcommit 11e8a6a344412beeb8dabe4cbc7c893648d603b4[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Wed May 22 23:53:22 2024 +0300

    Add Multipl Pinpoint

[33mcommit 4411b8e53329a212ce91fda6f4328aef74b8b257[m
Merge: 00483fed42 156dfb61a8
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Tue May 21 14:25:37 2024 +0300

    Merge branch 'master' into NukeSmallRevork

[33mcommit 00483fed427c11dfb82a2ec1eb1fd42da1c3151c[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Tue May 21 14:12:21 2024 +0300

    Commit!

[33mcommit 156dfb61a88293d99024b784d580fec1ef5a3225[m[33m ([m[1;31morigin/master[m[33m, [m[1;31morigin/HEAD[m[33m, [m[1;32mupstream[m[33m)[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 20 15:57:56 2024 -0700

    Mirror: Code cleanup: Purge obsoleted SharedPhysicsSystem methods (#384)
    
    ## Mirror of PR #26287: [Code cleanup: Purge obsoleted
    SharedPhysicsSystem
    methods](https://github.com/space-wizards/space-station-14/pull/26287)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `964c6d54caae45b205a326143f56d6458a1bbc8a`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/85356?v=4" width="16"/><a
    href="https://github.com/Tayrtahn"> Tayrtahn</a> at 2024-03-20 13:37:25
    UTC
    
    ---
    
    PR changed 16 files with 43 additions and 43 deletions.
    
    The PR had the following labels:
    - Status: Needs Review
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > <!-- Please read these guidelines before opening your PR:
    https://docs.spacestation14.io/en/getting-started/pr-guideline -->
    > <!-- The text between the arrows are comments - they will not be
    visible on your PR. -->
    >
    > Requires https://github.com/space-wizards/RobustToolbox/pull/4979
    >
    > ## About the PR
    > <!-- What did you change in this PR? -->
    > Cleans up some obsolete method calls.
    >
    > ## Why / Balance / Technical
    > <!-- Why was it changed? Link any discussions or issues here. Please
    discuss how this would affect game balance. -->
    > Cleaning up obsolete Dirty calls in RT required changing the
    signatures of some public methods in SharedPhysicsSystem. This updates
    the calls to those methods here in Content to use the new signatures
    passing in UIDs.
    >
    >
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit feb7db5db8edad714fcf89a0555ac2a0bd622bb9[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 20 15:57:19 2024 -0700

    Mirror: Gives borg industrial welder (#370)
    
    ## Mirror of PR #26332: [Gives borg industrial
    welder](https://github.com/space-wizards/space-station-14/pull/26332)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `1dff97901d5c4ea3f23cf29a9e6f1c2edebdbc27`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/164462467?v=4"
    width="16"/><a href="https://github.com/SoulFN"> SoulFN</a> at
    2024-03-22 09:48:46 UTC
    
    ---
    
    PR changed 1 files with 1 additions and 1 deletions.
    
    The PR had the following labels:
    - No C#
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > <!-- Please read these guidelines before opening your PR:
    https://docs.spacestation14.io/en/getting-started/pr-guideline -->
    > <!-- The text between the arrows are comments - they will not be
    visible on your PR. -->
    >
    > ## About the PR
    > Changes basic welder in borg tool module to industrial welder
    > <!-- What did you change in this PR? -->
    >
    > ## Why / Balance
    > Basic welder sucks
    > <!-- Why was it changed? Link any discussions or issues here. Please
    discuss how this would affect game balance. -->
    >
    > ## Technical details
    > No
    > <!-- If this is a code change, summarize at high level how your new
    code works. This makes it easier to review. -->
    >
    > ## Media
    > <!--
    > PRs which make ingame changes (adding clothing, items, new features,
    etc) are required to have media attached that showcase the changes.
    > Small fixes/refactors are exempt.
    > Any media may be used in SS14 progress reports, with clear credit
    given.
    >
    > If you're unsure whether your PR will require media, ask a maintainer.
    >
    > Check the box below to confirm that you have in fact seen this (put an
    X in the brackets, like [X]):
    > -->
    >
    > - This PR does not require an ingame showcase
    >
    > ## Breaking changes
    > No
    > <!--
    > List any breaking changes, including namespace, public
    class/method/field changes, prototype renames; and provide instructions
    for fixing them. This will be pasted in #codebase-changes.
    > -->
    >
    > **Changelog**
    > :cl:
    > - tweak: The borg tool module now has an industrial welding tool.
    > <!--
    > Make players aware of new features and changes that could affect how
    they play the game by adding a Changelog entry. Please read the
    Changelog guidelines located at:
    https://docs.spacestation14.io/en/getting-started/pr-guideline#changelog
    > -->
    >
    > <!--
    > Make sure to take this Changelog template out of the comment block in
    order for it to show up.
    >
    > -->
    >
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit e5314b25a1042b431323d46f21e10302da5de19a[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 20 15:56:51 2024 -0700

    Mirror: Removed Cannabis from thief objectives (#369)
    
    ## Mirror of PR #26412: [Removed Cannabis from thief
    objectives](https://github.com/space-wizards/space-station-14/pull/26412)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `b44015bd554eb97a2d4762d7b585c71e43865571`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/57235581?v=4"
    width="16"/><a href="https://github.com/ChaseFlorom"> ChaseFlorom</a> at
    2024-03-24 21:40:14 UTC
    
    ---
    
    PR changed 3 files with 4 additions and 26 deletions.
    
    The PR had the following labels:
    - No C#
    - Status: Needs Review
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > <!-- Please read these guidelines before opening your PR:
    https://docs.spacestation14.io/en/getting-started/pr-guideline -->
    > <!-- The text between the arrows are comments - they will not be
    visible on your PR. -->
    >
    > ## About the PR
    > <!-- What did you change in this PR? -->
    >
    > Removed the cannabis objective from the thief objectives.
    >
    > ## Why / Balance
    > <!-- Why was it changed? Link any discussions or issues here. Please
    discuss how this would affect game balance. -->
    >
    > 20-30 cannabis leaves is neither fun for the thief, or for the
    botanist that they're asking to grow it. It doesn't fit with the heart
    of the thief objective system, as the only real way to accomplish it is
    to ask the botanist to grow it for you, or grow it yourself. Neither of
    which is stealing.
    >
    > Resolve [#26321]
    >
    > ## Technical details
    > <!-- If this is a code change, summarize at high level how your new
    code works. This makes it easier to review. -->
    > Removed the proper .yml code.
    > ## Media
    > <!--
    > PRs which make ingame changes (adding clothing, items, new features,
    etc) are required to have media attached that showcase the changes.
    > Small fixes/refactors are exempt.
    > Any media may be used in SS14 progress reports, with clear credit
    given.
    >
    > If you're unsure whether your PR will require media, ask a maintainer.
    >
    > Check the box below to confirm that you have in fact seen this (put an
    X in the brackets, like [X]):
    > -->
    >
    > - [x] I have added screenshots/videos to this PR showcasing its
    changes ingame, **or** this PR does not require an ingame showcase
    >
    > ## Breaking changes
    > <!--
    > List any breaking changes, including namespace, public
    class/method/field changes, prototype renames; and provide instructions
    for fixing them. This will be pasted in #codebase-changes.
    > -->
    >
    > **Changelog**
    > <!--
    > Make players aware of new features and changes that could affect how
    they play the game by adding a Changelog entry. Please read the
    Changelog guidelines located at:
    https://docs.spacestation14.io/en/getting-started/pr-guideline#changelog
    > -->
    > :cl:
    > - remove: removed cannabis thief objective.
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit eb7a08980e85c492789d899b2e18520002233f27[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 20 15:56:20 2024 -0700

    Mirror: Store keybind priority (#365)
    
    ## Mirror of PR #26356: [Store keybind
    priority](https://github.com/space-wizards/space-station-14/pull/26356)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `f7a1ffd0aab25d70ac185753d596469fc1e87480`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/81056464?v=4"
    width="16"/><a href="https://github.com/wrexbe"> wrexbe</a> at
    2024-03-23 02:49:07 UTC
    
    ---
    
    PR changed 2 files with 2 additions and 1 deletions.
    
    The PR had the following labels:
    - Changes: UI
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > Changed it so the priority isn't lost when you set a binding in the
    UI.
    > Also added a priority to MoveStoredItem so it doesn't conflict with
    Use.
    > Fixes https://github.com/space-wizards/space-station-14/issues/26142
    > Does not fix old keybinds files, so they will need to reset it, and
    rebind it.
    >
    > A better solution might be to change it so the keybinds are always in
    the order they appear in the default keybinds folder, to prevent the
    ordering from changing unpredictably based on what the user overrides.
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit 28223bc2eaa170df7074599354f3ab50760b8255[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Mon May 20 18:28:03 2024 +0300

    Fuuuuck

[33mcommit fbc039aef07e5fca0e47b776cefbc604d1fc66f9[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Mon May 20 17:54:06 2024 +0300

    just commit

[33mcommit 6e0ffe81bc2a1be3e9d0453b8597a8f92df41dc1[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Sun May 19 23:33:00 2024 -0700

    Mirror: Partial atmos refactor (#312)
    
    ## Mirror of PR #22521: [Partial atmos
    refactor](https://github.com/space-wizards/space-station-14/pull/22521)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `18a35e7e83b2b71ee84b054d44d9ed5e595dd618`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/60421075?v=4"
    width="16"/><a href="https://github.com/ElectroJr"> ElectroJr</a> at
    2023-12-15 03:45:42 UTC
    
    ---
    
    PR changed 43 files with 891 additions and 635 deletions.
    
    The PR had the following labels:
    - Status: Needs Review
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > This PR reworks how some parts of atmos code work. Originally it was
    just meant to be a performance and bugfix PR, but it has ballooned in
    scope. I'm not sure about some of my changes largely because I'm not
    sure if some things were an oversight or an intentional decision for
    some reason.
    >
    > List of changes:
    > - The `MolesArchived float[]` field is now read-only
    > - It simply gets zeroed whenever the `GasMixture` is set to null
    instead of constantly reallocating
    > - Airtight query information is now cached in `TileAtmosphere`
    > - This means that it should only iterate over anchored entities once
    per update.
    > - Previously an invalidated atmos tile would cause
    `ProcessRevalidate()` to query airtight entities on the same tile six
    times by calling a combination of `GridIsTileAirBlocked()`,
    `NeedsVacuumFixing()`, and `GridIsTileAirBlocked()`. So this should help
    significantly reduce component lookups & entity enumeration.
    > - This does change some behaviour. In particular blocked directions
    are now only updated if the tile was invalidated prior to the current
    atmos-update, and will only ever be updated once per atmos-update.
    > - AFAIK this only has an effect if the invalid tile processing is
    deferred over multiple ticks, and I don't think it should cause any
    issues?
    > - Fixes a potential bug, where tiles might not dispose of their
    excited group if their direction flags changed.
    > - `MapAtmosphereComponent.Mixture` is now always immutable and no
    longer nullable
    > - I'm not sure why the mixture was nullable before? AFAICT the
    component is meaningless if its null?
    > - Space "gas" was always immutable, but there was nothing that
    required planet atmospheres to be immutable. Requiring that it be
    immutable gets rid of the constant gas mixture cloning.
    > - I don't know if there was a reason for why they weren't immutable to
    begin with.
    > - Fixes lungs removing too much air from a gas mixture, resulting in
    negative moles.
    > - `GasMixture.Moles` is now `[Access]` restricted to the atmosphere
    system.
    > - This is to prevent people from improperly modifying the gas mixtures
    (e.g., lungs), or accidentally modifying immutable mixtures.
    > - Fixes an issue where non-grid atmosphere tiles would fail to update
    their adjacent tiles, resulting in null reference exception spam
    >   - Fixes #21732
    >   - Fixes #21210 (probably)
    > - Disconnected atmosphere tiles, i.e., tiles that aren't on or
    adjacent to a grid tile, will now get removed from the tile set.
    Previously the tile set would just always increase, with tiles never
    getting removed.
    > - Removes various redundant component and tile-definition queries.
    > - Removes some method events in favour of just using methods.
    > - Map-exposded tiles now get updated when a map's atmosphere changes
    (or the grid moves across maps).
    > - Adds a `setmapatmos` command for adding map-wide atmospheres.
    > - Fixed (non-planet) map atmospheres rendering over grids.
    >
    > ## Media
    >
    > This PR also includes changes to the atmos debug overlay, though I've
    also split that off into a separate PR to make reviewing easier
    (#22520).
    >
    > Below is a video showing that atmos still seems to work, and that
    trimming of disconnected tiles works:
    >
    >
    https://github.com/space-wizards/space-station-14/assets/60421075/4da46992-19e6-4354-8ecd-3cd67be4d0ed
    >
    > For comparison, here is a video showing how current master works
    (disconnected tiles never get removed):
    >
    >
    https://github.com/space-wizards/space-station-14/assets/60421075/54590777-e11c-41dc-b49d-fd7e53bfeed7
    >
    > :cl:
    > - fix: Fixed a bug where partially airtight entities (e.g., thin
    windows or doors) could let air leak out into space.
    >
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit c62f777aee0c5e28df70953f77de950009e42c13[m
Author: github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
Date:   Sat May 18 19:19:09 2024 -0700

    Update Credits (#412)
    
    This is an automated Pull Request. This PR updates the GitHub
    contributors in the credits section.
    
    Co-authored-by: SimpleStation Changelogs <SimpleStation14@users.noreply.github.com>

[33mcommit a866a4872001f265731978629af50df2b655b99d[m
Author: DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
Date:   Sat May 18 15:38:06 2024 -0700

    Temporary CI/CD Fix (#411)
    
    # Description
    
    Ports
    https://github.com/DeltaV-Station/Delta-v/commit/7e3ba621d325db4d838c4d0fb675b5173f3111df
    until we get the real fix.
    Should allow tests to run, though it still fails most of the time
    according to Null.
    
    Co-authored-by: Null <56081759+NullWanderer@users.noreply.github.com>

[33mcommit 790fe7d5782a5f3c1ba996b72000d6330578b57a[m
Author: DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
Date:   Sat May 18 15:37:54 2024 -0700

    Build/Run Scripts from Parkstation (#6)
    
    # Description
    
    Adds several scripts to make building and running the projects easier
    outside of an IDE.

[33mcommit 2d9de78e78f73aecbc56c447b76b9476f2ff72a4[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Sat May 18 19:22:45 2024 +0300

    Add Nuke Sindicat Code

[33mcommit 8a85541fa2967e60f279ff9834f2f0a3dba8808f[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Sat May 18 08:54:14 2024 +0300

    Add Nuke sindicat code

[33mcommit 7dc6850a1415c6ba62057fa3bdea57893c0b179b[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Fri May 17 16:24:05 2024 +0300

    Add Nuke Sindicat Bomb

[33mcommit f4afece0933beaaa3550365aab78fda1c44c4ca6[m[33m ([m[1;32mRuLoc[m[33m)[m
Author: Spatison <137375981+Spatison@users.noreply.github.com>
Date:   Thu May 16 21:20:50 2024 +0300

    Add RuLoc

[33mcommit 25397147474d173cc8251fbef0c45c4ac75807e6[m
Author: DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
Date:   Wed May 15 16:10:36 2024 -0700

    Update close-master-pr Workflow's Denial Message (#347)
    
    # Description
    
    Didn't intend for this branch to be made on this repo.
    Improves the "close PRs from master" workflow's denial message.

[33mcommit c4f90e56161df940e845874e1980338f8c3851fc[m
Author: DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
Date:   Wed May 15 16:08:59 2024 -0700

    Add an Audio Automatic Label (#408)
    
    # Description
    
    Realized while going through issues that we don't have a label for audio
    file changes, I've added one.

[33mcommit 2079364f4225744b9950ff24ddfda118e1e65c45[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 13 11:24:04 2024 -0700

    Mirror: Hide empty marking categories in the markings picker (#324)
    
    ## Mirror of PR #26377: [Hide empty marking categories in the markings
    picker](https://github.com/space-wizards/space-station-14/pull/26377)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `4790ccba19c1f63f21a2aa57fe4525587c119d9c`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/10968691?v=4"
    width="16"/><a href="https://github.com/DrSmugleaf"> DrSmugleaf</a> at
    2024-03-24 03:49:01 UTC
    
    ---
    
    PR changed 1 files with 33 additions and 11 deletions.
    
    The PR had the following labels:
    - Changes: UI
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > ## About the PR
    > Updates when something about the character is changed.
    > This helps not gaslight me about which markings a species has
    available, and also slime person players are faced with the hard truth
    that they barely have any.
    >
    > ## Media
    >
    https://github.com/space-wizards/space-station-14/assets/10968691/439a4b39-a7c6-4ab1-839f-5436ba54b817
    >
    > - [x] I have added screenshots/videos to this PR showcasing its
    changes ingame, **or** this PR does not require an ingame showcase
    >
    > **Changelog**
    > :cl:
    > - tweak: Empty marking categories are now hidden in the markings
    picker.
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit a20e842992215fbac9609534d910f3054e3b18cc[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 13 06:58:10 2024 -0700

    Mirror: Fix EFCore obsoletion warnings in Content.Server.Database (#283)
    
    ## Mirror of PR #26285: [Fix EFCore obsoletion warnings in
    Content.Server.Database](https://github.com/space-wizards/space-station-14/pull/26285)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `fc76996dc55b19b313b621b2fb9025b9198e4b7d`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/8107459?v=4" width="16"/><a
    href="https://github.com/PJB3005"> PJB3005</a> at 2024-03-20 11:47:51
    UTC
    
    ---
    
    PR changed 1 files with 2 additions and 2 deletions.
    
    The PR had the following labels:
    - Status: Needs Review
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    >
    
    
    </details>
    
    Co-authored-by: SimpleStation14 <Unknown>

[33mcommit 2e226b341391943a062ecdbd3493ffbc57cb7f48[m
Author: SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
Date:   Mon May 13 06:55:58 2024 -0700

    Mirror: makes closets/lockers better (#257)
    
    ## Mirror of PR #24942: [makes closets/lockers
    better](https://github.com/space-wizards/space-station-14/pull/24942)
    from <img src="https://avatars.githubusercontent.com/u/10567778?v=4"
    alt="space-wizards" width="22"/>
    [space-wizards](https://github.com/space-wizards)/[space-station-14](https://github.com/space-wizards/space-station-14)
    
    ###### `e94fba5f702820588ad358a1824d67920fd399f8`
    
    PR opened by <img
    src="https://avatars.githubusercontent.com/u/79580518?v=4"
    width="16"/><a href="https://github.com/potato1234x"> potato1234x</a> at
    2024-02-04 09:07:17 UTC
    
    ---
    
    PR changed 9 files with 121 additions and 0 deletions.
    
    The PR had the following labels:
    - No C#
    - Changes: Sprites
    
    
    ---
    
    <details open="true"><summary><h1>Original Body</h1></summary>
    
    > <!-- Please read these guidelines before opening your PR:
    https://docs.spacestation14.io/en/getting-started/pr-guideline -->
    > <!-- The text between the arrows are comments - they will not be
    visible on your PR. -->
    >
    > ## About the PR
    > <!-- What did you change in this PR? -->
    >
    > Makes wall lockers craftable
    > Makes secure lockers craftable
    > Makes secure lockers deconstructible
    >
    > ## Why / Balance
    > <!-- Why was it changed? Link any discussions or issues here. Please
    discuss how this would affect game balance. -->
    >
    > every time i try to deconstruct a wall with a wall locker on it or
    need to remove a secure locker i have to beat i