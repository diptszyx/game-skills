---
name: styling-ai-art
description: Workflow for choosing a visual style and generating 2D game art (buttons, panels, icons, logo, tiles, characters) with an AI image tool, then locking the style, checking the generated art by looking at it, and packaging the assets. Use when a new game needs a look, when asked for style options or concept boards, when writing image-generation prompts for game assets, and whenever AI-generated art is about to be called done, accepted, approved or ready to import into the game. Not for 3D modeling, UI layout code or gameplay.
---

# Styling and generating AI art

The person making the game approves looks; they do not hand-edit art. So the job is: offer distinct options, lock the choice into a spec, generate every asset to that spec, and check results against it by looking.

## Steps
1. **Write the brief from the game's concept only** (verbs, setting, mood, camera). Do not feed in art from older games unless the user marks it as a reference; a draft game's art is not a reference.
2. **Make 3-4 style boards, clearly different** (for example clay 3D, flat vector, hand-painted, low-poly). Same sheet layout on every board so they compare side by side: primary button normal + pressed, secondary button, modal panel with title bar, 3 icons, logo, tile set, main character in 3 poses and 3 colors, 2 props, one phone-portrait sample screen.
3. **Show ONE contact sheet** (all boards in a grid), not many images. Mark the boards "not approved". Wait for the user's pick; if they cannot be asked, proceed only with work that does not depend on the pick.
4. **Lock the pick into a spec table**: palette hex values (sampled from the board), outline rule, corner radius, shadow/lighting, font style, button states, icon grid size, forbidden things. Save the table next to the prompts.
5. **Generate the full set from the spec**, one asset family per prompt, with the spec table pasted in. Save every prompt used in `PROMPTS.md` beside the outputs.
6. **Check by looking**: open the montage, list defects per asset (blank panel bodies, both button states labeled the same, wrong pose count, muddy edges), compare to the locked board. Do not report "matches" from file existence.
7. **Package**: cut transparent backgrounds, name files by role, report real pixel sizes. Generators often return less than the size you asked for (a 2048 request came back 1254 px): report the real size and decide if it is enough.

Measured example: four 1254x1254 boards took about 6 minutes in one Codex run; two boards had empty modal bodies and the "pressed" button looked identical to normal. Details: `references/boards.md`.
