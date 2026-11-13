# Default UI font choice (2026-11-13)

Recommendation: **Baloo 2, ExtraBold (wght 800)**, static instance at `tools/fonts/Baloo2-ExtraBold.ttf` (licence: `tools/fonts/OFL.txt`).

Sources: google/fonts repo, `ofl/<family>/` raw files (https://github.com/google/fonts/tree/main/ofl), downloaded 2026-11-13 into `work/fonts/`. Licence for all: SIL Open Font License 1.1 (embedding in a commercial game allowed, font itself not sold alone; each `OFL.txt` is in `work/fonts/<name>/`, https://openfontlicense.org).

Coverage test: fontTools cmap vs 203 chars = every precomposed Vietnamese vowel (a ă â e ê i o ô ơ u ư y) x 6 tones (none, grave, acute, hook, tilde, dot), upper + lower, plus đ/Đ, consonants and UI punctuation/digits (. , ! ? : · - + x % / ( ) ' "). Script: not kept (throwaway); rerun by diffing `getBestCmap()`.

| Font (file tested) | Licence | VN coverage | Missing glyphs | Weights | File size | Chunkiness fit |
|---|---|---|---|---|---|---|
| **Baloo 2** `Baloo2[wght].ttf` | OFL 1.1 | 100% | none | variable 400-800 (static 800 made with fontTools instancer) | 683 KB var / 422 KB static | Best: heavy rounded terminals, tight spacing, cartoon feel; matches thick-outlined look |
| Nunito `Nunito[wght].ttf` | OFL 1.1 | 100% | none | variable 200-1000 (Black 900 made) | 277 KB var / 132 KB static | Good, rounder and softer but more "friendly UI" than chunky; wide, loose spacing |
| Be Vietnam Pro `BeVietnamPro-ExtraBold.ttf` | OFL 1.1 | 100% | none | 9 static (100-900), 800 tested | 140 KB | Clean geometric sans, not rounded; reads corporate, least fit |
| Fredoka (rejected) | OFL 1.1 | 49.8% | Ă ă Đ đ Ĩ ĩ Ũ ũ Ơ ơ Ư ư and all dotted/hooked/stacked tone forms | var 300-700 | 159 KB | Best look, but unusable for Vietnamese |
| Lilita One (rejected) | OFL 1.1 | 49.8% | same set as Fredoka | 400 only | 28 KB | Chunky but unusable for Vietnamese |
| Sniglet ExtraBold (rejected) | OFL 1.1 | 49.8% | same set | 800 | 74 KB | unusable |
| Chango (rejected) | OFL 1.1 | 50.7% | same set minus Ĩ ĩ | 400 | 41 KB | unusable |

Names do not predict coverage: Fredoka, Lilita One, Sniglet and Chango (the chunkiest) only cover Latin-1 plus a few tone marks, not Vietnamese.

## Recommendation reasoning
- Baloo 2 is the only chunky rounded display-style face of the set with full Vietnamese (the Baloo 2 family was built for Indic + Vietnamese).
- Sample render (white fill, dark outline 5 px, 36 px): `work/fonts/compare.png`. Stacked marks (ẫ, ể, ậ, ờ) stay legible at 36 px with outline; no collisions seen. 24 px not rendered; check on device.
- Fallback if Baloo 2 looks too tight on device: Nunito Black.

## Notes for the kit
- Unity TextMeshPro needs a static font: use `Baloo2-ExtraBold.ttf`, not the variable file. Static instance made by `fontTools.varLib.instancer` with wght=800 and updateFontNames; the copyright line in OFL.txt declares no Reserved Font Name, so the instance may keep the name.
- Baloo has tall ascent/descent metrics: expect extra line height; tune TMP line spacing / margins.
- TMP atlas must include the full Vietnamese set (all 134 composed vowel forms + Đđ); use a character list, not dynamic ASCII-only.
