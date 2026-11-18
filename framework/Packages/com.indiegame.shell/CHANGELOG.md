# Changelog

## Unreleased

- Bottom-tab screens (Check-in, Quests, Shop, Leaderboard) open as modals over Home through the shared `PopupHost`/`PopupView` (new `TabModals`), matching the original Hole It! UI: dim scrim, centred panel with the popup title zone and close X, tab bar left visible and clickable (the scrim stops above it), active tab highlighted, Home stays behind. Back/Escape closes one layer; a session popup above a tab popup closes first.
- New `ShellStyle.TabsAsModals` (default true; only effective with `EnableHomeChrome`). Set false to keep flat full-screen pages with a Back button.
- Swipe now switches the open tab modal to its neighbour (and past the first tab returns Home). Tab popups carry their own `TabSwipeHandler`; `SwipeForwarder` on list scrolls forwards drags the `ScrollRect` would swallow (also fixes swipe over lists in page mode).
- Home gear accessibility name is Settings (`popup.settings.title`) on Home and Pause only on the game screen.
- `QuestsView`, `ShopView`, `LeaderboardView`, `CheckInView` take an optional `PopupView modal` argument.

## 0.2.0 — 2026-11-18

- Add editor texture-import policy (ASTC 6x6 default, 4x4 detail exception, mipmaps a policy field, per-asset opt-out marker), applier, read-only audit and per-screen `SpriteAtlasBuilder` under `Editor/Textures`.
- Add shared quest pack `QuestPack.Standard(baseGold)`: five daily templates (`win1/3/5`, `play3/5`), three picked per day, localized in en and vi. Shell records `win` and `play` itself when the Win/Lose popup opens, so games no longer call `RecordEvent`. A game customizes by passing its own `DailyQuestsConfig` to `ShellAppConfig.Standard(..., quests:)` and adding `shell.quest.<id>` keys.
- Quest rows are sorted by target; the progress track sits inside the row card instead of straddling its bottom border.
- Derive popup title zones from usable panel top borders; border-less or insufficient borders retain the 104/8 layout.
- Centre one to three icon actions in an equal-size row; keep labelled actions and larger sets stacked.
- Support optional round and pill button sprites with white custom surfaces and procedural fallbacks.
- Hide lower popup views while another popup is above them, restoring them on close.
