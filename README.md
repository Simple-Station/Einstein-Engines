# Sector Crescent - Hullrot 

<p align="center"> <img alt="Sector Crescent" width="880" height="300" src="https://github.com/ilikeships/Sector-Crescent/blob/master/Resources/Textures/Logo/logo.png?raw=true" /></p>

This is the Main Repository of the Hullrot Server. Hullrot is a spin on classic SS14 prioritizing persistance, ship-combat , immersion and a more serious tone than other SS14 servers.

License-wise, for any code in the Hullrot Folder , it has been licensed to AGPL, with files that have alternative licenses being denoted so through comments at the top.

Most sprite assets in the Hullrot Folder are propietary, meaning do not redistribute , do not relicense , do not use and do not modify. Exemptions are explicity mentioned in the json of the RSI assets.

---

## Links
Hullrot Discord - https://discord.gg/e6n9n9xgHN
## Contributing
Before contributing , you are advised to consult either the Head Maintainer or the Project Lead over on the Hullrot Discord.

If you are new to programming in SS14 as a whole , you are advised to check out the #ss14-coding-crashcourse channel.

If you are experienced or wish to work on pre-approved content , we have a list available in the #dev-roadmaps channel.

We aren't as strict on Coding Standards as other SS14 servers , our review methodology takes into account how "performant" something needs to be, altough a few aspects should be kept in mind
- any code that is performance-significant is expected to be performant
- you are expected to write secure code
- you are expected to TEST your own code and ensure it works to a reasonable level
- do not leave behind old-code or code with no functionality
- you are expected to credit the source if you are porting content through a comment in the file(for code) or meta.json(for RSIs)
- do not mix unrelated balance changes with normal PR's.

### Build dependencies

> - Git
> - .NET SDK 9.0.101


### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/bat/buildAllRelease.bat` after making any changes to the source
> 4. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 5. Connect to localhost in the client and play

### Linux / MAC

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/sh/buildAllRelease.sh` after making any changes to the source
> 4. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 5. Connect to localhost in the client and play

## License

Please read the [LEGAL.md](./LEGAL.md) file for information on the licenses of the code and assets in this repository.
