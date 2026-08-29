# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] - 2026-08-29

### Added

- **Models** — Enable Veilay; Do nothing / Hide / Replace to dummy for Player, Pets, NPC, and Enemy; dummy size, role colors, and player labels (None / Role / Job). Entities stay so overlays can track them
- **Native** — hide selected native HUD Layout elements; during duty replay, optionally mute battle effects and target lines
- **Camera** — lock Center / Pos / Angle (field pick, self, target helpers); optional WASD block in duty replay and arrow block while locked; Cammy FreeCam notice
- Live and replay support for models, HUD, and camera; character-config overrides stay replay-only
- Auto-off for Enable Veilay, HUD checks, and Lock Camera on replay enter/end, live zone change, and logout (continuous replay keeps them on)
- Command `/veilay` to toggle the window
- Plugin icon and README (en / ja)

[Unreleased]: https://github.com/exatrines/Veilay/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/exatrines/Veilay/releases/tag/v0.1.0
