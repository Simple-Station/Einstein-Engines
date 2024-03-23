# Einstein Engines

<p align="center"><img src="https://raw.githubusercontent.com/Simple-Station/Einstein-Engines/master/Resources/Textures/Logo/splashlogo.png" width="512px" /></p>

---

Einstein Engines is a hard fork of [Space Station 14](https://github.com/space-wizards/space-station-14) built around the ideals and design inspirations of the Baystation family of servers from Space Station 13 with a focus on having modular code that anyone can use to make the RP server of their dreams.
Our founding organization is based on a democratic system whereby our mutual contributors and downstreams have a say in what code goes into their own upstream.
If you are a representative of a former downstream of Delta-V, we would like to invite you to contact us for an opportunity to represent your fork in this new upstream.

Space Station 14 is inspired heavily by Space Station 13 and runs on [Robust Toolbox](https://github.com/space-wizards/Robust-Toolbox), a homegrown engine written in C#.

As a hard fork, any code sourced from a different upstream cannot ever be merged directly here, and must instead be ported.
All code present in this repository is subject to change as desired by the council of maintainers.

## Official Server Policy

**No official servers will ever be made for Einstein-Engines**.

In order to prevent a potential conflict of interest, we will never open any server directly using the Einstein Engines codebase itself.
Any server claiming to be an official representation of this fork is not endorsed in any way by this organization.
We however would like to invite anyone wishing to create a server to make a fork of Einstein Engines.

## Links

[Website](https://simplestation.org) | [Discord](https://discord.gg/X4QEXxUrsJ) | [Steam(SSMV Launcher)](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/) | [Steam(WizDen Launcher)](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Standalone](https://spacestationmultiverse.com/downloads/)

## Contributing

We are happy to accept contributions from anybody, come join our Discord if you want to help.
We've got a [list of issues](https://github.com/Simple-Station/Einstein-Engines/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help in Discord either!

We are currently accepting translations of the game on our main repository.
If you would like to translate the game into another language check the #contributor-general channel in our Discord.

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Einstein Engines is not the same and many things may not apply.
We provide some scripts shown below to make the job easier.

### Build dependencies

> - Git
> - DOTNET SDK 7.0 or higher
> - python 3.7 or higher


### Windows

> 1. Clone this repository
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal
> 3. Run the `Scripts/bat/run1buildDebug.bat`
> 4. Run the `Scripts/bat/run2configDev.bat` if you need other configurations run other config scripts
> 5. Run both the `Scripts/bat/run3server.bat` and `Scripts/bat/run4client.bat`
> 6. Connect to localhost and play

### Linux

> 1. Clone this repository
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal
> 3. Run the `Scripts/sh/run1buildDebug.sh`
> 4. Run the `Scripts/sh/run2configDev.sh` if you need other configurations run other config scripts
> 5. Run both the `Scripts/sh/run3server.bat` and `scripts/sh/run4client.sh`
> 6. Connect to localhost and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux

## License

Content contributed to this repository after commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the GNU Affero General Public License version 3.0 unless otherwise stated.
See [LICENSE-AGPLv3](https://github.com/Simple-Station/Einstein-Engines/blob/master/LICENSE-AGPLv3.txt).

Content contributed to this repository before commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the MIT license unless otherwise stated.
See [LICENSE-MIT](https://github.com/Simple-Station/Einstein-Engines/blob/master/LICENSE-MIT.txt).

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file.
[Example](https://github.com/Simple-Station/Einstein-Engines/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
