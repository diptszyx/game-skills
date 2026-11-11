---
name: orchestrating-ai-work
description: Rules for running a game project with Claude as orchestrator and Codex or sub-agents as workers. Use at the start of any work session on a game, when delegating code, Unity, Blender or image jobs, when starting or watching background jobs, when a mistake is found (write the retro now), or before saying "nothing is running" or "done". Not for answering a one-off question.
---

# Orchestrating AI work on a game

These rules come from repeated mistakes in two earlier games (retro ids in brackets, see `references/lessons.md`). A rule that must hold every time belongs in a hook, not here; hooks live in `hooks/` of this repo.

## 1. Start of session (do in this order)
1. Read the project `CLAUDE.md`, then the current plan and the project retro file. Run a command to check the real state (git status, running processes) instead of trusting the plan text.
2. Find the standing permissions (project memory, plan). If commit/push autonomy is unknown, ask once and write the answer to memory. Without a standing grant, ask before commit and push. The user repeated "do not stop to ask small things" four times in game-3 [C8].
3. Look for existing assets, data and decisions before building: project docs, earlier games, approved art. Say where you looked [C2, R1].

## 2. Never say "unknown" before searching twice
Search by name and by id/GUID, and list every exported file before concluding data does not exist [R1, C6]. Label each fact: measured, from a source (give it), approved by the user, or set by you. Numbers you invented must say so; do not tune by eye and ship [C1].

## 3. Delegating a job
A brief has: one goal, files allowed (everything else forbidden), what to read first, the check command with expected result, what not to do, a time cap, `AMBIG` for anything the docs do not settle, and a report template. One goal per job; two jobs never edit the same file or build the same project [O04].
- Big feature or architecture change: propose concretely, have Codex critique read-only using project data, check each finding yourself (right / wrong / unchecked), only then write it into the plan. Codex has been wrong; do not forward its text unchecked.
- If you can do it in under 5 minutes, do it yourself.
- When splitting work across agents, agents only write code. Unity runs once at the end, by one executor, on the merged tree [R5, R8].
- Do not run full sweeps (all levels, all screens). Test a small fixed sample; ask first if the estimate exceeds 20 minutes [C5].

## 4. Background jobs
- Start a Monitor the moment a job starts: liveness, progress file, and an end-of-process signal that also covers crash and hang. Monitors expire (30 min max): re-arm and say when no watch exists.
- Before saying "nothing is running" check two sources (task list and `ps`), because tools can disagree [C4]. "Waiting for its own background work" is not done [R6].
- Use explicit `timeout` on long foreground commands so they are not pushed to the background silently.
- Time and cost numbers need a recorded run. Label the rest "estimate" or "cap".

## 5. When you find a mistake of your own
Open the project retro file and write four lines now, before fixing: what went wrong, the consequence, what should have been done, status [R0]. A lesson that is true for every game also goes to the shared lessons file. Memory is a shortcut, not the record: it is stored per working directory and was lost between games.

## 6. Before reporting
Say what is done, not done and unknown, each with evidence. Interim images are labelled drafts. Use the `verifying-game-results` skill for anything visible, playable or built.
