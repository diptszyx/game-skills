# Changelog

## 0.2.0 — 2026-11-18

- Add editor texture-import policy (ASTC 6x6 default, 4x4 detail exception, mipmaps a policy field, per-asset opt-out marker), applier, read-only audit and per-screen `SpriteAtlasBuilder` under `Editor/Textures`.
- Add shared quest pack `QuestPack.Standard(baseGold)`: five daily templates (`win1/3/5`, `play3/5`), three picked per day, localized in en and vi. Shell records `win` and `play` itself when the Win/Lose popup opens, so games no longer call `RecordEvent`. A game customizes by passing its own `DailyQuestsConfig` to `ShellAppConfig.Standard(..., quests:)` and adding `shell.quest.<id>` keys.
- Quest rows are sorted by target; the progress track sits inside the row card instead of straddling its bottom border.
- Derive popup title zones from usable panel top borders; border-less or insufficient borders retain the 104/8 layout.
- Centre one to three icon actions in an equal-size row; keep labelled actions and larger sets stacked.
- Support optional round and pill button sprites with white custom surfaces and procedural fallbacks.
- Hide lower popup views while another popup is above them, restoring them on close.
