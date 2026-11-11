---
name: verifying-game-results
description: Checklist for proving a game change works before reporting it done. Use after changing code, UI, art, shaders, levels or builds in a Unity game, before saying done, finished, fixed or ready, when a test run looks green, when screenshots or phone checks are needed, and when comparing a screen to an approved reference image. Not for pure Q&A or planning.
---

# Verifying game results

"Tests pass" has been green while the screen was wrong: white balls, Home drawn over gameplay, a lose panel showing placeholder "Level 10" [R2, R9, C7]. Tests only check what you assumed. Prove each item below with evidence (a command result or a file path), not with the word "checked".

## Checklist
1. **Compile is clean.** Count `error CS` in the Unity editor log; it must be 0 before you read test numbers. Delete old result files first so a stale green report cannot pass. Do not chain `| tail && git commit` (tail returns 0).
2. **Logic tests** for the new rule pass, and the project regression gate passes in full for shared code changes. Run only related tests while iterating.
3. **Gate can fail.** Break something on purpose once and confirm the gate catches it. A gate never seen failing proves little.
4. **Play it like a player.** Drive the feature through real input (taps, drags, buttons), including edges: screen edge, two fingers, release mid-gesture, cold start from saved data, out of lives.
5. **Look at it.** Screenshot key moments including mid-animation, at two aspect ratios (16:9 and 20:9) and in every language. Open the images. Compare the whole screen against the approved reference image, not just numbers; measurements only filter errors. There is no original game to copy: the reference is an image the user approved.
6. **Every screen and popup of the flow** has a reference and a capture (win, lose, revive, pause, leave, shop...), not only Home and gameplay [R9]. Scan the whole family of prefabs/sprites after a visual bug; do not fix one sample and stop.
7. **Performance** when adding something heavy: milliseconds per frame (average and p95) on the real phone. Editor numbers are labelled Editor.
8. **On the phone:** confirm the right app is in the foreground, the screenshot is not black, and the Unity log has no new errors before using the result. Ask first if the user may be using the phone.
9. **Report:** list tested items with results, image paths, and what only the user can judge (feel, sound, fun). Name each layer done and not done. Say "Editor", "device" or "not checked" explicitly.

## If a check cannot run
Say which one and why, and keep the claim at "not verified". Do not substitute a weaker check silently.
