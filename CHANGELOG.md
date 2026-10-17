# Changelog

Notable changes to Faddi. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/).

## [1.0.0] - 2026-10-17

First public release.

### Added

- Whole-drive scan in about a minute, with a disk strip that splits the drive into games and programs, developer leftovers, caches, old and big things, Windows and everything else.
- Space map colored by how long since each folder changed.
- Cleanup sorted by safety, with developer junk detection: `node_modules`, Python venvs, Unity, Unreal, .NET, Gradle, Flutter and Rust build output, and package caches.
- Largest files, file types, duplicate files, and installed programs with their real size.
- RAM optimizer: on demand or automatic (when usage passes a limit or on a timer, only while the CPU is idle), standby cache clearing as admin, optional clipboard clearing.
- RAM monitor that logs sudden jumps and the programs behind them.
- System info: CPU, RAM, GPU, disks with their health, network, displays, battery and startup apps.
- Tray icon, start with Windows, one copy at a time.
- English, Arabic, Spanish, French, Portuguese, Russian, Turkish and Simplified Chinese.

[1.0.0]: https://github.com/sox29-ui/Faddi/releases/tag/v1.0.0
