# ItemStacksItemWeights

Control item stack sizes and weights. Dynamic server-to-client synchronization. Works globally or per item.

The mod creates a `shudnal.ItemStacksItemWeights.yml` file in the `BepInEx/config/shudnal.ItemStacksItemWeights` folder and populates it with default values on first launch.

You can edit this file to change item stack sizes and weights.

In multiplayer, install this mod and Conditional Config Sync on the server and all clients. Edit the stack and weight configuration on the server.

After each file update, all settings are synchronized automatically and item properties are updated accordingly.

## File structure

The configuration file is a simple YAML structure consisting of four main sections:

``` yaml
WeightMultiplier:
  Global: 1
WeightAmount:
  ItemName1: 2
StackMultiplier:
  Global: 1
StackSize:
  ItemName1: 50
  ItemName2: 100
```

### Sections

- `WeightMultiplier` -- Defines a *multiplier* applied to the base weight of items.
- `WeightAmount` -- Defines an *absolute weight value* for items. This value will override weight multiplier value for the item.
- `StackMultiplier` -- Defines a *multiplier* applied to base stack sizes.
- `StackSize` -- Defines an *absolute stack size* for items. This value will override stack multiplier value for the item.

### Keys
- `Global` -- applies to all items unless overridden.
- `<PrefabName>` -- applies only to a specific item.

## Automatically generated documentation

When you start a world, an `Items stacks and weights.txt` file will be generated in the `BepInEx/config/shudnal.ItemStacksItemWeights` directory.

This file contains all items available in your current game session. Use it to find the exact item names.

You can regenerate this file manually at any time using the `iwisdocs` console command.

## Installation (manual)
extract ItemStacksItemWeights.dll to your `BepInEx\plugins\` folder.

## Configurating
The best way to handle configs is [Configuration Manager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/).

Or [Official BepInEx Configuration Manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).

## Large stacks and save compatibility

Valheim 1.0.7 stores the built-in stack count in a 16-bit field. This mod preserves counts above 65,535 in the item's custom data and restores them when loading the item. Keep this mod installed on every peer that can save or load these items. Removing it while oversized stacks exist can reduce their saved counts.

The loader also accepts legacy `StackCount` metadata. New saves use the canonical `FullStack` key. Split oversized stacks into stacks of at most 65,535 before removing the mod.

Keep configured stack limits large enough for existing stacks: the game's inventory loader still enforces the active limit. This update cannot recover counts that were already truncated in an earlier save. Back up the world and characters before updating.

## Dependencies

- [BepInExPack Valheim 5.4.2350](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- [ConditionalConfigSync 1.0.5](https://thunderstore.io/c/valheim/p/shudnal/ConditionalConfigSync/)

Install ConditionalConfigSync as a separate dependency; do not copy its DLLs into this mod's package.

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)

## Discord
[Join server](https://discord.gg/e3UtQB8GFK)
