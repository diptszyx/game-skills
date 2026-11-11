#!/usr/bin/env python3
import hashlib
import json
import os
import subprocess
import sys

WATCHED = (".cs", ".png", ".jpg", ".shader", ".prefab", ".unity", ".py", ".sh", ".blend", ".json")
MARKER_DIR = os.path.join(os.environ.get("TMPDIR", "/tmp"), "game-skills-selfcheck")
CHECKLIST = (
    "Before finishing, answer each item with evidence (a command result or a file path) or write 'not verified':\n"
    "1. error CS count in the editor log is 0 and old test results were deleted before the run.\n"
    "2. The new logic has a test and the regression gate passed.\n"
    "3. Screens changed: captured at 16:9 and 20:9 and compared with the approved reference image.\n"
    "4. Every popup of the flow was captured, not only Home and gameplay.\n"
    "5. No number was invented: each has a source or the label 'set by me'.\n"
    "6. Unity ran once at the end if work was split across agents.\n"
    "7. Background jobs checked with two sources before saying nothing runs.\n"
    "Past mistakes to rule out: reported done without comparing to the reference; green tests with a wrong screen; "
    "'unknown' without searching by name and id; mistake found but no retro written.\n"
    "Then give the final report: done, not done, unknown."
)


def changed_files(cwd):
    result = subprocess.run(["git", "status", "--porcelain"], cwd=cwd, capture_output=True, text=True)
    if result.returncode != 0:
        return []
    names = [line[3:].strip() for line in result.stdout.splitlines() if len(line) > 3]
    return [name for name in names if name.endswith(WATCHED)]


def main():
    data = json.load(sys.stdin)
    if data.get("stop_hook_active"):
        return 0
    cwd = data.get("cwd") or os.getcwd()
    files = changed_files(cwd)
    if not files:
        return 0
    digest = hashlib.sha1(("\n".join(sorted(files)) + data.get("session_id", "")).encode()).hexdigest()
    os.makedirs(MARKER_DIR, exist_ok=True)
    marker = os.path.join(MARKER_DIR, digest)
    if os.path.exists(marker):
        return 0
    open(marker, "w").close()
    print(CHECKLIST, file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main())
