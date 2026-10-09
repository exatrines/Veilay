<p align="center">
  <img src="Veilay/Data/plugin-icon.png" alt="Veilay icon" width="128" height="128">
</p>

<h1 align="center">Veilay</h1>

<p align="center">
  English | <a href="docs/README.ja.md">日本語</a>
</p>

<p align="center">
  <a href="https://github.com/exatrines/Veilay/releases/latest">
    <img src="https://img.shields.io/github/v/release/exatrines/Veilay?label=Release&amp;labelColor=F280B6&amp;color=FFFFFF&amp;style=flat&amp;sort=date&amp;display_name=tag" alt="Release">
  </a>
  <a href="CHANGELOG.md">
    <img src="https://img.shields.io/badge/Changelog-view-FFFFFF?labelColor=F280B6&amp;style=flat" alt="Changelog">
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/License-AGPL--3.0--or--later-FFFFFF?labelColor=F280B6&amp;style=flat" alt="AGPL-3.0-or-later">
  </a>
</p>

<p align="center">
  <img src="docs/screenshots/veilay-overlay-730x380.png" alt="Hidden models with Splatoon overlays on the arena">
</p>

Veilay is a Dalamud plugin that hides character models locally, clears selected HUD elements, and can lock the camera. It is mainly for sharing drawing-plugin workflows such as Splatoon while character model visuals stay hidden.

You can hide or replace player, pet, NPC, and enemy models. Entities are not removed, so Splatoon and similar tools can still track positions. You can also hide native HUD elements, turn off battle effects, and place a locked camera.

Locked camera conflicts with Cammy FreeCam—use care when both are loaded.

## Install

1. Run `/xlsettings` and open the **Experimental** tab
2. Add this URL under **Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. Run `/xlplugins` and install **Veilay**

## Features

- **Models** — Enable Veilay; hide or replace Player / Pets / NPC / Enemy models (entities stay) and hide nameplates
- **Native** — hide selected HUD Layout elements; mute battle effects, target lines, fly text, and pop-up text during duty replay
- **Camera** — lock Center / Pos / Angle (avoid using with Cammy FreeCam)

## Commands

| Command | Description |
| --- | --- |
| `/veilay` | Toggle the Veilay window |

## For developers

1. Build: `dotnet build Veilay.sln -c Release -p:Platform=x64`
2. Point Dalamud’s **dev plugin** path at `Veilay/bin/Release/`
3. Enable **Veilay** in the plugin installer (dev)

## Contributing

Contributions are always welcome! Please see the [contribution guide](CONTRIBUTING.md).
