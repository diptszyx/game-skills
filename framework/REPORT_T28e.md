# T28e: home screen, check-in day cells, ChromePresenter (written by Claude, 2026-11-12)

Codex implemented this job (brief `briefs/T28e_home.md`) but its process died before writing a report. This file records what Claude verified independently. Codex's own claims are not repeated here unless re-checked.

## Verified by Claude (independent runs, same tree)
- Shell: EditMode 68/68, PlayMode 34/34, `error CS` 0 (`framework/tests/run_unity_smoke.sh`).
- Pilot `game-2-v2` on ShellApp, legacy Main path (EnableHomeChrome false): EditMode 325/325, PlayMode 4/4, `error CS` 0 (`game-2-v2/Tools/run_gate.py`).
- Screenshots opened: `unity-smoke/Logs/shots/T28e_{Main,CheckIn}_{720x1600,1080x2340}_{en,vi}.png`.

## What the screenshots show
- Main: logo, mascot, one large Play button, top bar (gold, hearts with countdown), bottom tab bar with 5 tabs and a badge on Check in. en and vi render with full diacritics inside their frames.
- CheckIn: five day cells (one Today, four Locked) and a Claim button.

## Defects seen, not yet fixed
1. CheckIn screen title is thin cream text on a pastel background: almost unreadable.
2. CheckIn reward line ("Gold 25, Hearts 1") is small and sits under Day 1 only.
3. Day 4 and Day 5 cells sit left-aligned on a second row.
4. "Leaderboard" tab label touches the cell edges.
5. Top-bar countdown text is small.

## Not checked
- Real device, real Back key, `game-2-v2` migrated to the home composition (pilot still uses the legacy Main).
- Whether bg_home or bg_play is the better ScreenBackground was decided by Codex (bg_home in the chunky demo style); Claude did not compare them.
- No mutation (deliberate-break) test was run on the new ChromePresenter badge logic or the tab routing.
