#!/usr/bin/env python3
import json
import os
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))


def run(script, payload):
    result = subprocess.run([sys.executable, os.path.join(HERE, script)], input=json.dumps(payload), capture_output=True, text=True)
    return result.returncode


def bash(command):
    return {"tool_name": "Bash", "tool_input": {"command": command}}


def edit(path):
    return {"tool_name": "Edit", "tool_input": {"file_path": path}}


def make_repo(files):
    directory = tempfile.mkdtemp()
    subprocess.run(["git", "init", "-q"], cwd=directory, check=True)
    for name in files:
        open(os.path.join(directory, name), "w").write("x")
    return directory


CASES = [
    ("guard_git.py", "blocks reset --hard", bash("git reset --hard HEAD~1"), 2),
    ("guard_git.py", "blocks pull --rebase", bash("git pull --rebase origin main"), 2),
    ("guard_git.py", "blocks push --force", bash("git push --force origin main"), 2),
    ("guard_git.py", "blocks push -f", bash("git push -f"), 2),
    ("guard_git.py", "allows reset --soft", bash("git reset --soft HEAD~1"), 0),
    ("guard_git.py", "allows pull", bash("git pull origin main"), 0),
    ("guard_git.py", "allows push", bash("git push -u origin main"), 0),
    ("guard_git.py", "allows non-git command mentioning force", bash("echo force and reset --hard"), 0),
    ("guard_paths.py", "blocks Edit in game-5", edit("/Users/tho/project/indie-game/game-5/docs/PILLARS.md"), 2),
    ("guard_paths.py", "blocks sed -i in game-3", bash("sed -i '' s/a/b/ /Users/tho/project/indie-game/game-3/docs/x.md"), 2),
    ("guard_paths.py", "blocks redirect into game-kit", bash("echo hi > /Users/tho/project/indie-game/game-kit/a.txt"), 2),
    ("guard_paths.py", "allows stderr merge when reading game-5", bash("grep foo /Users/tho/project/indie-game/game-5/a.cs 2>&1"), 0),
    ("guard_paths.py", "allows reading game-5", bash("cat /Users/tho/project/indie-game/game-5/CLAUDE.md"), 0),
    ("guard_paths.py", "allows Edit in this repo", edit("/Users/tho/project/indie-game/game-skills/docs/PLAN.md"), 0),
    ("guard_paths.py", "allows lookalike directory", edit("/Users/tho/project/indie-game/game-50/a.md"), 0),
]


def main():
    failures = 0
    for script, name, payload, expected in CASES:
        got = run(script, payload)
        status = "ok" if got == expected else "FAIL"
        failures += status == "FAIL"
        print(f"{status} {script} {name}: exit {got}, expected {expected}")

    code_repo = make_repo(["a.cs"])
    clean_repo = make_repo(["notes.md"])
    first = {"cwd": code_repo, "session_id": "t1", "stop_hook_active": False}
    stop_cases = [
        ("blocks when code changed", first, 2),
        ("allows same changes second time", first, 0),
        ("allows when stop_hook_active", {"cwd": code_repo, "session_id": "t2", "stop_hook_active": True}, 0),
        ("allows docs-only change", {"cwd": clean_repo, "session_id": "t3", "stop_hook_active": False}, 0),
        ("allows outside a git repo", {"cwd": tempfile.mkdtemp(), "session_id": "t4", "stop_hook_active": False}, 0),
    ]
    for name, payload, expected in stop_cases:
        got = run("stop_selfcheck.py", payload)
        status = "ok" if got == expected else "FAIL"
        failures += status == "FAIL"
        print(f"{status} stop_selfcheck.py {name}: exit {got}, expected {expected}")
    total = len(CASES) + len(stop_cases)
    print(f"{total - failures}/{total} passed")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
