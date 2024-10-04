ghost-respawn-time-left = Before the opportunity to return to the round { $time } 
    { $time ->
        [one] minute
       *[other] minutes
    }
ghost-respawn-max-players = The function is not available, there should be fewer players on the server { $players }.
ghost-respawn-window-title = Rules for returning to the round
ghost-respawn-window-rules-footer = By using this feature, you [color=#ff7700]agree[/color] [color=#ff0000]not to transfer[/color] the knowledge of your past character to a new one. For violation of the clause specified here, [color=#ff0000]a ban in the amount of 3 days or more follows[/color].
ghost-respawn-same-character = You cannot enter the round for the same character. Change it in the character settings.

ghost-respawn-log-character-almost-same = Player { $player } { $try ->
    [true] join
    *[false] tried to join
} in the round after the respawn with a similar name. Past name: { $oldName }, current: { $newName }.
ghost-respawn-log-return-to-lobby = { $userName } returned to the lobby.