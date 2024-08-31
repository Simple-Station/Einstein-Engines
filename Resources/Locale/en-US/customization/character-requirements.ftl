## Job
character-job-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be one of these jobs: {$jobs}

character-department-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be in one of these departments: {$departments}

character-timer-department-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes of [color={$departmentColor}]{$department}[/color] department playtime
character-timer-department-too-high = You require [color=yellow]{TOSTRING($time, "0")}[/color] fewer minutes in [color={$departmentColor}]{$department}[/color] department

character-timer-overall-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes of playtime
character-timer-overall-too-high = You require [color=yellow]{TOSTRING($time, "0")}[/color] fewer minutes of playtime

character-timer-role-insufficient = You require [color=yellow]{TOSTRING($time, "0")}[/color] more minutes with [color={$departmentColor}]{$job}[/color]
character-timer-role-too-high = You require[color=yellow] {TOSTRING($time, "0")}[/color] fewer minutes with [color={$departmentColor}]{$job}[/color]


## Logic
character-logic-or-requirement-listprefix = {""}
    {""}[color=white]-[/color]{" "}
character-logic-or-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} fit at least one of these: {$options}

character-logic-xor-requirement-listprefix = {""}
    {""}[color=white]-[/color]{" "}
character-logic-xor-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} fit only one of these: {$options}


## Profile
character-age-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be within [color=yellow]{$min}[/color] and [color=yellow]{$max}[/color] years old

character-backpack-type-requirement = You must {$inverted ->
    [true] not use
    *[other] use
} a [color=brown]{$type}[/color] as your bag

character-clothing-preference-requirement = You must {$inverted ->
    [true] not wear
    *[other] wear
} a [color=white]{$type}[/color]

character-gender-requirement = You must {$inverted ->
    [true] not have
    *[other] have
} the pronouns [color=white]{$gender}[/color]

character-sex-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be [color=white]{$sex ->
    [None] unsexed
    *[other] {$sex}
}[/color]
character-species-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be a {$species}

character-height-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be{$min ->
    [-2147483648]{$max ->
        [2147483648]{""}
        *[other] shorter than [color={$color}]{$max}[/color]cm
    }
    *[other]{$max ->
        [2147483648] taller than [color={$color}]{$min}[/color]cm
        *[other] between [color={$color}]{$min}[/color] and [color={$color}]{$max}[/color]cm tall
    }
}

character-width-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be{$min ->
    [-2147483648]{$max ->
        [2147483648]{""}
        *[other] skinnier than [color={$color}]{$max}[/color]cm
    }
    *[other]{$max ->
        [2147483648] wider than [color={$color}]{$min}[/color]cm
        *[other] between [color={$color}]{$min}[/color] and [color={$color}]{$max}[/color]cm wide
    }
}

character-weight-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be{$min ->
    [-2147483648]{$max ->
        [2147483648]{""}
        *[other] lighter than [color={$color}]{$max}[/color]kg
    }
    *[other]{$max ->
        [2147483648] heavier than [color={$color}]{$min}[/color]kg
        *[other] between [color={$color}]{$min}[/color] and [color={$color}]{$max}[/color]kg
    }
}


character-trait-requirement = You must{$inverted ->
    [true] not have
    *[other] have
} one of these traits: {$traits}

character-loadout-requirement = You must{$inverted ->
    [true] not have
    *[other] have
} one of these loadouts: {$loadouts}


character-item-group-requirement = You must {$inverted ->
    [true] have {$max} or more
    *[other] have {$max} or less
} items from the group [color=white]{$group}[/color]


## Whitelist
character-whitelist-requirement = You must{$inverted ->
    [true]{" "}not
    *[other]{""}
} be whitelisted
