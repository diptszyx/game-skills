# T21: three style boards v3 (trial, NOT user-approved), corrected by measuring real games

Game concept (read only): portrait mobile puzzle "Belly Slide". Colored penguins on an ice board; swipe one to belly-slide until it hits an obstacle or another penguin; 3 same-colour penguins touching huddle and leave. Open ice tiles, cracked tiles, holes, fish stop markers, seals as blockers. Do NOT copy any existing game art; use only the rules below.

Why v3: v2 boards (art/style_trial_v2/) were measured against two real top games on a phone. Real games have 2-3x LOWER edge/detail density, warmer lighter shadows, soft depth-blurred SCENIC backgrounds (not flat purple gradient), and a MASCOT hero on the home screen. v2 had a dense identical 6x6 penguin grid (looked like a template, and not a slide puzzle).

Goal: use image_gen to make THREE boards, each ONE PNG, as large as image_gen allows (try 2048 wide first), saved to /Users/tho/project/indie-game/game-skills/art/style_trial_v3/board_<N>_<name>.png.
Styles: 1 smooth painterly-3D mobile casual (soft rendered, gentle gradients, like premium casual games); 2 chunky 2D cartoon (thick rounded dark-tinted outline, flat blocks, one soft gradient per shape); 3 glossy candy toy 3D (bevelled, single specular highlight per object).

Hard rules in EVERY prompt:
1. Large simple shapes. LOW detail: no ice veins, snow fluff, scratches, noise, fur. Smooth surfaces. Fewer, bigger things; empty calm areas are good.
2. Backgrounds are a SOFT DEPTH-BLURRED SCENE (home: icy cove at warm dusk, pastel sky, distant snowy hills, a small igloo and pine trees, all blurred and low contrast; gameplay: same scene heavily blurred behind the board). NOT a flat gradient, NOT pale white.
3. ONE warm light from upper left. Shadows soft, tinted blue-violet, never black/grey. Darkest areas must stay mid-dark (no near-black).
4. Hierarchy: ONE big loud primary button per screen (red-orange or green, thick light rim). Everything else calmer.
5. Home screen has ONE big MASCOT: a single charismatic red penguin (big expressive eyes, tiny scarf), posed confidently, taking ~35% of screen height, the brightest and most detailed thing on the screen.
6. Gameplay board is a SLIDE PUZZLE: mostly open light-blue ice tiles with only 4-5 penguins (red, blue, yellow, big, readable), 1 fish marker, 1 seal blocker, 2 dark holes, 2 cracked tiles. Lots of EMPTY ice. Board in a thick rounded frame, about 60% of screen height.
7. Top bar identical in style to mobile casual games: pill with coin + count, pill with heart + "5", round settings gear. Bottom: one primary button + 3 small booster icons.
8. Text: bold white rounded font with dark outline, very few words, spelled correctly ("Belly Slide", "Play", "Level 12", "Win!", "Next").
9. All objects share one material and edge style.

Layout (identical on all three): THREE portrait phone screens side by side, no phone bezels needed: (a) HOME: logo "Belly Slide" top, mascot centre, big Play button "Level 12", bottom tab bar with 4 icons; (b) GAMEPLAY per rule 6; (c) WIN popup over the blurred board: ribbon "Win!", 3 gold stars, huddled penguin trio, primary "Next".

Work: one attempt per board, retry once only if clearly broken. Append exact prompt to art/style_trial_v3/PROMPTS.md. Edit only art/style_trial_v3/. No git. Cap 30 minutes. If image_gen unavailable say so and stop.
Report art/style_trial_v3/REPORT.md: file, pixel size, honest per-rule check (look at the image), minutes.
