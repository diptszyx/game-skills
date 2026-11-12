# T25 kit v3 run report

Run stopped at the brief's 40-minute cap. Only `art/kit_v3/btn_primary.png` was retried in this run and opened after saving. The remaining 19 assets were not processed in this run; do not treat them as passing the T25 check. Existing prior records in `PROMPTS.md` document defects for the pre-run files.

## Current verified output

| File | Pixel size | 1. Plum outline? | 2. Any large glossy highlight or gradient? | 3. Penguin face/proportions? | 4. Palette only? | Defects |
|---|---:|---|---|---|---|---|
| `btn_primary.png` | 2048 x 682 | Yes | No | Yes (not a penguin; N/A) | Yes | Large highlight and broad smooth shading remain after the one allowed retry. |

## Not processed in this run

`btn_secondary.png`, `btn_locked.png`, `panel_popup.png`, `btn_close.png`, `pill_currency.png`, `pill_lives.png`, `tab_bar.png`, `list_row.png`, `toggle.png`, `day_cell.png`, `badge.png`, `icons.png`, `tiles.png`, `penguin_red_idle.png`, `penguin_red_slide.png`, `penguins_blue_yellow.png`, `fish_and_seal.png`, `logo.png`, `bg_home.png`, `bg_play.png`.

The 13 existing assets that were inspected through the prior T24/T25 records include reported style/palette defects; their detailed prior prompt and self-check records remain in `PROMPTS.md`. Seven of the 20 listed pieces did not exist at run start. No claim is made that unprocessed assets pass.
