ghost-respawn-time-left = Please wait {$time} {$time ->
    [one] minute
   *[other] minutes
} before trying to return to the round.

ghost-respawn-max-players = Cannot return to the round right now. There should be fewer players than {$players}.
ghost-respawn-window-title = Respawn rules
ghost-respawn-window-rules-footer = By respawning, you [color=#ff7700]agree[/color] [color=#ff0000]not to use any knowledge gained as your previous charactrer[/color]. Violation of this rule may constitute a server ban. Please, read the server rules for more details.
ghost-respawn-same-character = You cannot enter the round as the same character. Please, change it in the character preferences.

ghost-respawn-log-character-almost-same = Player {$player} { $try ->
    [true] joined
    *[false] tried to join
} the round after respawning, with a similar name. Previous name: { $oldName }, current: { $newName }.
ghost-respawn-log-return-to-lobby = { $userName } returned to the lobby.
