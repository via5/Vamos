## Introduction
Vamos is a BepInEx plugin. It adds functionality to Virt-a-Mate that would otherwise be impossible from a regular script.

## Installation
- Download [the last BepInEx 5.x release](https://github.com/BepInEx/BepInEx/releases) (currently [5.4.21](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip)).
- Extract the zip into the VaM folder. There should be a file `winhttp.dll` alongside `VaM.exe` and a new folder `BepInEx`.
- Run VaM once to initialize BepInEx.
- Close VaM.
- Download [the latest Vamos DLL](https://github.com/via5/Vamos/releases).
- Extract the DLL into `VaM/BepInEx/plugins/`.
- Run VaM. Vamos should be running.

## Configuration
There's a configuration file `VaM/BepInEx/config/via5.vamos.cfg`. It can be opened in Notepad. Each feature has an associated section and an entry `enabled = true`. Change `true` to `false` to disable the feature and restart VaM.

## Features

### Drop files
`.var` files can now be dropped on the VaM window. Vamos will load the scene from the `.json` file from `Saves/scene/`, if any. If the `.var` file contains multiple scenes, Vamos will display an error message and do nothing. The scene will need to be loaded manually from VaM.

### Auto UI scale
Vamos will detect when the VaM window changes monitors and will adjust the UI scaling based on the Windows font scaling (such as 100%, 125%, etc.).
