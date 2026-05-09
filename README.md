# Planet Crafter Save Editor

A browser-based save editor for the game [Planet Crafter](https://store.steampowered.com/app/1284190/The_Planet_Crafter/). Move stuck players or items, edit inventory contents, and rename saves — without touching the original file.

> ⚠️ **Always keep a backup of your original save file before replacing it.** This editor downloads a new file with `-edited` in the name; copying it back over your original is up to you.

## What it does today

- Load a Planet Crafter save (drag-drop or file picker)
- Teleport players to the escape pod, another player, or custom coordinates
- Teleport world objects (containers, vehicles, items) to custom coordinates
- Edit inventory contents: add items by `gId`, remove, move between inventories, change count and growth values
- Rename the save's display name
- Review every pending change before downloading
- Discard all changes if an edit session goes wrong

## Run it locally

Requires .NET 10 SDK.

```bash
dotnet run --project src/PlanetCrafterSaveEditor
```

Then open the URL it prints (defaults to `http://localhost:5000`).

## Supported game version

Tested against Planet Crafter save format `2.007`. The editor will try to load other versions too — if it parses without complaint, the same edits should work; behave with the usual caution.

## Status

Passive development. Bug reports welcome.
