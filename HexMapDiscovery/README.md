# HexMapDiscovery

Increases map exploration radius while actively piloting a ship in Valheim. Default multiplier is 3, but I prefer 5.

## Features

* Increases map exploration radius while steering a ship
* Configurable exploration radius multiplier
* Works with vanilla ships
* Compatible with ValheimRAFT

---

## Configuration

Configuration file location:

```text
BepInEx/config/com.hex.mapdiscovery.cfg
```

Default configuration:

```ini
[General]

## Enable or disable the mod
# Setting type: Boolean
# Default value: true
Enabled = true

[Exploration]

## Multiplier for the exploration radius while piloting a ship
# Setting type: Single
# Default value: 3
# Acceptable value range: From 1 to 10
ExplorationRadiusMultiplier = 3
```

---

## Installation

### Thunderstore

Install with your preferred mod manager.

### Manual Installation

1. Install BepInEx for Valheim.
2. Extract `HexMapDiscovery.dll` into:

```text
BepInEx/plugins/
```

3. Launch the game.

---

## Multiplayer

* Have not tested in multiplayer.

---

## Compatibility

* Modifies exploration radius through a Harmony patch on `Minimap.Explore`.
* Compatible with vanilla ships.
* Compatible with ValheimRAFT.
* Should be compatible with most mods that do not modify map exploration behavior.

---

## Tested With

* Valheim
* ValheimRAFT
