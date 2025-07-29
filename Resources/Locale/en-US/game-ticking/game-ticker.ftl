game-ticker-restart-round = Restarting round...
game-ticker-start-round = The round is starting now...
game-ticker-start-round-cannot-start-game-mode-fallback = Failed to start {$failedGameMode} mode! Defaulting to {$fallbackMode}...
game-ticker-start-round-cannot-start-game-mode-restart = Failed to start {$failedGameMode} mode! Restarting round...
game-ticker-start-round-invalid-map = Selected map {$map} is inelligible for gamemode {$mode}. Gamemode may not function as intended...
game-ticker-unknown-role = Unknown
game-ticker-delay-start = Round start has been delayed for {$seconds} seconds.
game-ticker-pause-start = Round start has been paused.
game-ticker-pause-start-resumed = Round start countdown is now resumed.
game-ticker-player-join-game-message = Welcome to Sector Crescent. Keep your eyes open, your paranoia high and your allies close. You might just live through this.
game-ticker-get-info-text = Enter [color=white]Hullrot[/color]
                            The current round is: [color=white]#{$roundId}[/color]
                            The current player count is: [color=white]{$playerCount}[/color]
                            The current game mode is: [color=white]{$gmTitle}[/color]
                            >[color=yellow]{$desc}[/color]
game-ticker-get-info-preround-text = Enter [color=red]Hullrot[/color]
                            The current round is: [color=white]#{$roundId}[/color]
                            The current player count is: [color=white]{$playerCount}[/color] ([color=white]{$readyCount}[/color] {$readyCount ->
                                [one] is
                                *[other] are
                            } ready)
                            The current game mode is: [color=white]{$gmTitle}[/color]
                            >[color=yellow]{$desc}[/color]
game-ticker-no-map-selected = [color=yellow]Map not yet selected![/color]
game-ticker-player-no-jobs-available-when-joining = When attempting to join to the game, no jobs were available.
game-ticker-welcome-to-the-station = The Hadal Tide has rescinded. All personnel are to resume their normal duties. Anyone who suspects Hadal Contamination is to present to the medbay.

# Displayed in chat to admins when a player joins
player-join-message = Player {$name} joined.
player-first-join-message = Player {$name} joined for the first time.

# Displayed in chat to admins when a player leaves
player-leave-message = Player {$name} left.

latejoin-arrival-announcement = {$character} ({$job}) has arrived at the station!
latejoin-arrival-sender = Station
latejoin-arrivals-direction = A shuttle transferring you to your station will arrive shortly.
latejoin-arrivals-direction-time = A shuttle transferring you to your station will arrive in {$time}.
latejoin-arrivals-dumped-from-shuttle = A mysterious force prevents you from leaving with the arrivals shuttle.
latejoin-arrivals-teleport-to-spawn = A mysterious force teleports you off the arrivals shuttle. Have a safe shift!

preset-not-enough-ready-players = Can't start {$presetName}. Requires {$minimumPlayers} players but we have {$readyPlayersCount}.
preset-no-one-ready = Can't start {$presetName}. No players are ready.
