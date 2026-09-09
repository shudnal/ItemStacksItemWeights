# 1.1.1
* Updated for the Valheim 1.0.7 release.
* Completed the migration to the standalone ConditionalConfigSync dependency.
* Updated required dependencies to BepInExPack Valheim 5.4.2350 and ConditionalConfigSync 1.0.5.
* Updated the item creation hook for the release API.
* Fixed initial YAML application and debounced reloads after file creation, replacement, or deletion; invalid updates keep the last valid settings.
* Normalized configuration dictionaries consistently and validated stack/weight values to prevent invalid overrides and stack multiplier overflow.
* Preserved counts above 65,535 with one custom-data serializer that reads current and legacy full-stack metadata. The mod is now required on the server and all clients in multiplayer.
* Preserved rich-text formatting when hiding stack limits.

# 1.0.7
* fields in configuration file can now be safely omitted

# 1.0.6
* fixed actual mod assebly version

# 1.0.5
* support of HTML tags inside stack text (ZenUI compatibility and possibly other similar mods)

# 1.0.4
* keyboard shortcut to bypass stack size no longer server synced

# 1.0.3
* added new configs: compact stack size, compact amount size, shortcut to bypass changes

# 1.0.2
* fixed side incompatibility with mods using inhouse localization implementation (like PlantEverything and QSSSRT)

# 1.0.1
* fixed nasty issue with consecutive applying of multipliers on consecutive log off and log on

# 1.0.0
* Initial release