# Standing instructions for Codex in this repo (read every session)

Claude briefs you per job (`briefs/*.md`). This file holds rules that apply to EVERY job, so briefs do not repeat them. If a brief conflicts with this file, the brief wins for that job only.

## Always
- Edit only the files the brief lists. No git commands. Never commit.
- Report honestly: what you checked, what you could not check, defects you saw. Do not claim "done" for something you did not open or run.
- Cap time as the brief says; if blocked, stop and say why.

## Image jobs (image_gen)
- Before generating, read `art/style_lock/STYLE_BRIEF.md` and open the nearest exemplar in `art/style_lock/exemplars/`. Use the STYLE_BRIEF text as the style system prompt in every image request, plus the asset line from the brief, AND attach the nearest exemplar as a reference image (`referenced_image_paths`; verified accepted 2026-11-12, the one-piece test matched the reference far better than text-only).
- `image_gen` writes each PNG to `~/.codex/generated_images/<your thread id>/exec-*.png`. Save with `cp` of the NEWEST file inside YOUR OWN thread folder only (`ls -t ~/.codex/generated_images/<your thread id>/ | head -1`), never the newest across all folders (another job's image can be copied by mistake). NEVER base64 or pipe image data through a command (empty files).
- After saving, OPEN the saved target file and check it shows the subject the brief asked for (a past job saved wrong images and reported them as fine). Run the four self-check questions at the end of STYLE_BRIEF. A "no" means regenerate once, then report the defect.
- If `image_gen` returns no image, retry the same piece once, then continue with the next and say so in the report.
- Pieces for cut-out go on ONE flat solid #FF00FF background, centred, wide margin.
- Append the exact prompt used to the job's `PROMPTS.md` right after each piece, not at the end.
- Write one piece, verify, then the next. Never produce a batch before the first piece is verified.

## Unity jobs
- Count `error CS` in `~/Library/Logs/Unity/Editor.log` first; it must be 0 before reading test results. One Unity instance per project.
