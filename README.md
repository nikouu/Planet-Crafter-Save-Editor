# Planet Crafter Save Editor

A browser-based save editor for the game [Planet Crafter](https://store.steampowered.com/app/1284190/The_Planet_Crafter/). Move stuck players or items, edit inventory contents, and rename saves — without touching the original file.

> ⚠️ **Always keep a backup of your original save file before replacing it.** This editor downloads a new file with `-edited` in the name; copying it back over your original is up to you.

👉[Edit your save here!](https://nikouu.github.io/Planet-Crafter-Save-Editor/)👈

<img width="1170" height="1174" alt="image" src="https://github.com/user-attachments/assets/d38fefc5-f5db-4c17-b2e4-09b574520aba" />

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

## An experiment

Read more about it here: [Vibe Coding in Mid 2026](http://www.nikouusitalo.com/blog/vibe-coding-in-mid-2026/).
