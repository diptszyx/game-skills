---
name: sprite-animation
description: How to make a 2D game character or prop look alive in Unity (breathing, blinking, swaying, waving) using separate layered parts plus bone or Transform tweens, driven by data profiles. Use when asked to animate a mascot, character, prop or home-screen scene, when a flat image "needs movement", when choosing amplitude or period, or when AI-generated parts must line up. Not for gameplay movement code, 3D rigs, or sprite import settings (see sprite-import-atlas).
---

# Making a 2D character alive

## Rules
1. **Animate parts, not one flat image.** Patching the finished picture changed 0.04% of pixels per frame and the owner saw no difference. Split the character into separate layers (body, head, arm segments, eyes) and move those. (`game-2/docs/HOME_ALIVE_PLAN.md:24`; layered plan `:41`)
2. **Parts move by a bone chain or Transform tweens.** Pivot at the joint, parent along the chain, drive rotation / scale / offset. For a 2-bone arm a plain Transform tween on separate renderers is enough to start; the Unity 2D Animation package adds mesh deform if joints gap. Comparison and versions: `game-5/docs/BEAR_RIG_RESEARCH.md`. Neither needs a paid Spine runtime.
3. **Behaviour is data, not code.** Each moving thing declares a profile with amplitude, period, phase and target part. Existing profiles: `sway`, `rigid_bob`, `flipbook`, `lamp_pendulum`, `glow_pulse`. A new character adds data only; add code only for a new profile. (`game-2/docs/ASSET_PIPELINE.md:52,57`)
4. **Use a flipbook only when the shape changes** (flame, steam, flag). Rigid parts use bob or sway, since flipbooks multiply generation cost and drift. (`game-2/docs/ASSET_PIPELINE.md:50`)
5. **Measure target numbers from a reference before choosing amplitude.** Take the reference clip or approved image, measure the displacement of the furthest point from the pivot in screen pixels, and copy that. Do not guess degrees. Visibility floor on a 260 ppi phone: far-point travel of at least 1.5 to 2 px; a static tilt does not count as motion. A lamp swing of 0.24 degrees (0.3 px) was invisible and was the mistake. (`game-2/docs/HOME_ALIVE_PLAN.md:175`)
6. **Cut parts from one approved drawing; do not generate them separately.** Separately generated AI parts disagree at the seams (shoulder, neck, ear): different outline weight, shading and position. If parts must be regenerated, give the approved whole as the source image and check the seams at rest and at the maximum pose, on light and dark backgrounds. (risk noted for parts: `game-2/docs/HOME_ALIVE_PLAN.md:41` L2 criterion of at most 1% differing pixels when reassembled)
7. **Rest pose must equal the approved still.** Reassemble the parts at zero offset and diff against the whole image; the plan's gate is at most 1% differing pixels. (`game-2/docs/HOME_ALIVE_PLAN.md:41`)
8. **Cover joints.** Rigid rotation opens a gap at elbows and shoulders; overlap the segments, round their ends, or hide the joint under another part. Test at the largest amplitude.
9. **Use unscaled time** if the character animates in popups while the game is paused (`building-game-ui` rule 5).
10. **Verify motion on the phone, not by reading numbers.** Capture two frames at the extremes of each profile, compute the pixel difference, then watch it on the Galaxy A07. (`game-2/docs/HOME_ALIVE_PLAN.md:24`)

## Minimal bear example (2-bone arm)
Parts: `body`, `head`, `arm_upper`, `arm_lower`, `eyes_open`, `eyes_closed`. Profiles: `body` bob (scale Y +-1.5%, period 3.5 s), `arm_upper` sway, `arm_lower` sway with a phase lag, `eyes` blink (closed 0.12 s every 3-5 s random). Amplitudes are placeholders until rule 5 is done.

## Checklist
- [ ] Layers exist as separate files; manifest lists part, pivot and parent.
- [ ] Rest pose diff vs approved still at most 1%.
- [ ] Every profile has a measured reference number and a source note.
- [ ] Largest pose shows no gap or seam on light and dark backgrounds.
- [ ] Frame-pair pixel diff is clearly above zero (rule 1) and far-point travel is at least 1.5 px.
- [ ] New behaviour added as data; code touched only for a new profile.
- [ ] Watched on the A07 and judged by the owner for feel (I cannot judge that).
