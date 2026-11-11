# Read-only review: how to build the shell SCREENS and POPUPS on what already exists

READ ONLY. Edit nothing, run no Unity, no git. Your final message is your answer (it is saved by the caller); do not try to create files. Stop reading after about 25 files and answer.
Context: framework/ (this repo) has a pure-C# Core (save, economy, lives, daily check-in, daily quests, settings, localization, notifications, config, service sockets) and Unity adapters (haptics, audio, safe area, loading). Still missing: the SCREENS (main, shop, daily check-in, daily quests, leaderboard, loading) and POPUPS (win, lose, revive, pause, leave) as Unity uGUI built by code, themable by a style spec (colors, sprites, fonts) so a new art style swaps without code edits. Plan to reuse /Users/tho/project/indie-game/game-kit/Packages/com.indiegame.ui (theme, modal, safe area, button style, builder, validator; read-only) and ideas from /Users/tho/project/indie-game/game-2/unity/Assets/Scripts/View (read-only; the user says game-2 is a rough draft, do not treat its look as a reference, only its structure).
Answer with evidence (file:line), under 120 lines:
1. What in game-kit ui can be reused as is, what is broken or too game-specific (verify by reading code and its tests, not by names).
2. Proposed minimal architecture: which classes, how a screen gets data from Core services (MVP? plain view + presenter), how popups stack, how a theme swap works, where text keys from Localization go. Fewest elements.
3. Concrete list of at most 8 jobs in dependency order, each with files, a measurable gate (EditMode/PlayMode test names or a screenshot rule), and a time estimate marked UNMEASURED unless from a recorded run (use REPORT_T1b.md, REPORT_T6.md, REPORT_A/B/C timings).
4. Top 5 risks, each with how to detect it early.
5. Anything in game-kit or game-2 that contradicts the plan above.
