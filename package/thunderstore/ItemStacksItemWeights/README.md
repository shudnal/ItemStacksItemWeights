# ItemStacksItemWeights

Control item stack sizes and weights. Dynamic server-to-client synchronization. Works globally or per item.

The mod creates a `shudnal.ItemStacksItemWeights.yml` file in the `BepInEx/config/shudnal.ItemStacksItemWeights` folder and populates it with default values on first launch.

You can edit this file to change item stack sizes and weights.

If you're using a dedicated server and want all clients to have synchronized stack sizes and weights, install the mod on the server and edit the file there.

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

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)

## Discord
[Join server](https://discord.gg/e3UtQB8GFK)