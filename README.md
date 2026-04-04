<!--
SPDX-FileCopyrightText: 2017 PJB3005 <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2019 Ivan <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2019 Silver <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2020 Injazz <43905364+Injazz@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 RedlineTriad <39059512+RedlineTriad@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
SPDX-FileCopyrightText: 2023 iglov <iglov@avalon.land>
SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
SPDX-FileCopyrightText: 2024 Rares Popa <2606875+rarepops@users.noreply.github.com>
SPDX-FileCopyrightText: 2024 router <messagebus@vk.com>
SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>

SPDX-License-Identifier: AGPL-3.0-or-later
-->

<p align="center"> <img alt="Space Station 14" width="880" height="300" src="https://github.com/BohdanNovikov0207/Orehum-Project/blob/master/Resources/Textures/Logo/logo.png" /></p>

This is a fork from the primary repo for Space Station 14 called Orehum Project. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server this is the content pack for Orehum Project.

If you want to host or create content for SS14, go to the [Space Station 14 repository](https://github.com/space-wizards/space-station-14) as it contains both RobustToolbox and the content pack for development of new content packs and is the base for your fork.

## Links

[Orehum Project Discord Server](https://discord.gg/nUz6nH6RqW) | [Goob Station Wiki](https://wiki.orehum-project.ru)

## Documentation/Wiki

The Goob Station [docs site](https://docs.goobstation.com/) has documentation on GS14's content, engine, game design, and more. It also has lots of resources for new contributors to the project.

## Contributing

We are happy to accept contributions from anybody. Get in [Development Discord Server](https://discord.gg/nUz6nH6RqW) if you want to help.
While following the [Space Station 14 contribution guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html) is not mandatory for Goob Station, we recommend reviewing them for best practices.

We are currently accepting translations of the game on our main repository.
If you would like to translate the game into another language check the #contributor-general channel in our Discord.

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Einstein Engines is not the same and many things may not apply.
We provide some scripts shown below to make the job easier.

### Build dependencies

> - Git
> - .NET SDK 9.0.101


### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/bat/buildAllDebug.bat` after making any changes to the source
> 4. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 5. Connect to localhost in the client and play

### Linux

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/sh/buildAllDebug.sh` after making any changes to the source
> 4. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 5. Connect to localhost in the client and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux

## License

All code in this codebase is released under the AGPL-3.0-or-later license. Each file includes REUSE Specification headers or separate .license files that specify a dual license option. This dual licensing is provided to simplify the process for projects that are not using AGPL, allowing them to adopt the relevant portions of the code under an alternative license. You can review the complete texts of these licenses in the LICENSES/ directory.

Most media assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
