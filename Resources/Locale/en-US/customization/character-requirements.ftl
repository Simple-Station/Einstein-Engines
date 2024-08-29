# Job
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


# Profile
character-age-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} be within [color=yellow]{$min}[/color] and [color=yellow]{$max}[/color] years old
character-species-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} a {$species}
character-trait-requirement = You must {$inverted ->
    [true] not have
    *[other] have
} one of these traits: {$traits}
character-loadout-requirement = You must {$inverted ->
    [true] not have
    *[other] have
} one of these loadouts: {$loadouts}
character-backpack-type-requirement = You must {$inverted ->
    [true] not use
    *[other] use
} a [color=brown]{$type}[/color] as your bag
character-clothing-preference-requirement = You must {$inverted ->
    [true] not wear
    *[other] wear
} a [color=white]{$type}[/color]


# Whitelist
character-whitelist-requirement = You must {$inverted ->
    [true] not be
    *[other] be
} whitelisted
