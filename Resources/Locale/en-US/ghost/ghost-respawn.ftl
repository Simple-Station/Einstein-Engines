ghost-respawn-minutes-left = Please wait {$time} {$time ->
    [one] minute
   *[other] minutes
} before trying to respawn.
ghost-respawn-seconds-left = Please wait {$time} {$time ->
    [one] second
   *[other] seconds
} before trying to respawn.

ghost-respawn-max-players = Cannot respawn right now. There should be fewer than {$players} players.
ghost-respawn-window-title = Respawn rules
ghost-respawn-window-rules-footer = By respawning, you [color=#ff7700]agree[/color] [color=#ff0000]not to use any knowledge gained as your previous character[/color]. Violation of this rule may constitute a server ban. Please read the server rules for more details.
ghost-respawn-same-character = You cannot respawn as the same character. Please select a different one in character preferences.

ghost-respawn-log-character-almost-same = Player {$player} { $try ->
    [true] joined
    *[false] tried to join
} the round after respawning with a similar name. Previous name: { $oldName }, current: { $newName }.
ghost-respawn-log-return-to-lobby = { $userName } returned to the lobby.
