character-age-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} be within [color=yellow]{$min}[/color] and [color=yellow]{$max}[/color] years old
character-species-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} a [color=green]{$species}[/color]
character-trait-requirement = You must {$inverted ->
    [true] not have
    *[other] have
} the trait [color=lightblue]{$traits}[/color]
character-backpack-type-requirement = You must {$inverted ->
    [true] not use
    *[other] use
} a [color=lightblue]{$type}[/color] as your bag
character-clothing-preference-requirement = You must {$inverted ->
    [true] not wear
    *[other] wear
} a [color=lightblue]{$type}[/color]

character-job-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} one of these jobs: {$jobs}
character-department-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} in one of these departments: {$departments}

character-timer-department-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes of [color={$departmentColor}]{$department}[/color] department playtime
character-timer-department-too-high = You require [color=yellow]{TOSTRING($time, "0")}[/color] fewer minutes in [color={$departmentColor}]{$department}[/color] department
character-timer-overall-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes of playtime
character-timer-overall-too-high = You require [color=yellow]{TOSTRING($time, "0")}[/color] fewer minutes of playtime
character-timer-role-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes with [color={$departmentColor}]{$job}[/color]
character-timer-role-too-high = You require[color=yellow] {TOSTRING($time, "0")}[/color] fewer minutes with [color={$departmentColor}]{$job}[/color]

character-trait-group-exclusion-requirement = You cannot have one of the following traits if you select this: {$traits}
character-loadout-group-exclusion-requirement = You cannot have one of the following loadouts if you select this: {$loadouts}
