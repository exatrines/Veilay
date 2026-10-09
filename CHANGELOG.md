# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- Added `CONTRIBUTING.md`, synced from the shared exatrines contribution guide

### Changed

- Reorganized the README: centered plugin icon and Release / Changelog / License badges, a Contributing section, and the Japanese README moved to `docs/README.ja.md`
- Restored `LICENSE` to the official GNU AGPL v3 text

## [0.1.3] - 2026-08-31

### Added

- Enable Veilay also hides the NamePlate addon (live and replay)
- Native character config can hide fly text and pop-up text during duty replay

### Changed

- Nameplates are hidden with the NamePlate addon instead of RenderFlags; leftover Nameplate flags on models are cleared
- Native character-config parent switch is labeled Override Native Character Config (Duty Replay only)

## [0.1.2] - 2026-08-30

### Changed

- Character model hide uses Alpha 0 again, because `DrawObject.IsVisible` left weapons visible

## [0.1.1] - 2026-08-30

### Changed

- Character model hide now uses `DrawObject.IsVisible` instead of Alpha 0, so movement and actions stay usable while models are hidden (nameplates still use RenderFlags)

## [0.1.0] - 2026-08-29

### Added

- **Models** — Enable Veilay; Do nothing / Hide / Replace to dummy for Player, Pets, NPC, and Enemy; dummy size, role colors, and player labels (None / Role / Job). Entities stay so overlays can track them
- **Native** — hide selected native HUD Layout elements; during duty replay, optionally mute battle effects and target lines
- **Camera** — lock Center / Pos / Angle (field pick, self, target helpers); optional WASD block in duty replay and arrow block while locked; Cammy FreeCam notice
- Live and replay support for models, HUD, and camera; character-config overrides stay replay-only
- Auto-off for Enable Veilay, HUD checks, and Lock Camera on replay enter/end, live zone change, and logout (continuous replay keeps them on)
- Command `/veilay` to toggle the window
- Plugin icon and README (en / ja)

[Unreleased]: https://github.com/exatrines/Veilay/compare/v0.1.3...HEAD
[0.1.3]: https://github.com/exatrines/Veilay/compare/v0.1.2...v0.1.3
[0.1.2]: https://github.com/exatrines/Veilay/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/exatrines/Veilay/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/exatrines/Veilay/releases/tag/v0.1.0
