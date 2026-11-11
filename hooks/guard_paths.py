#!/usr/bin/env python3
import json
import os
import re
import sys

CONFIG = os.path.join(os.path.dirname(os.path.abspath(__file__)), "protected_paths.txt")
WRITE_VERBS = r"(\bsed\s+-i|\brm\b|\bmv\b|\bcp\b|\btee\b|\btouch\b|\bmkdir\b|\bgit\s+(commit|add|checkout|restore|stash|push)\b|(?<!\d)>>?(?!&))"


def protected_roots():
    with open(CONFIG, encoding="utf-8") as handle:
        return [line.strip().rstrip("/") for line in handle if line.strip()]


def is_protected(path, roots):
    path = os.path.abspath(os.path.expanduser(path))
    return any(path == root or path.startswith(root + "/") for root in roots)


def check(data, roots):
    tool = data.get("tool_name")
    args = data.get("tool_input", {})
    if tool in ("Edit", "Write", "NotebookEdit"):
        path = args.get("file_path") or args.get("notebook_path") or ""
        if path and is_protected(path, roots):
            return f"{path} belongs to another session; this repo must not edit it"
    if tool == "Bash":
        command = args.get("command", "")
        if re.search(WRITE_VERBS, command):
            for root in roots:
                if root in command:
                    return f"command writes under {root}, which belongs to another session"
    return None


def main():
    data = json.load(sys.stdin)
    message = check(data, protected_roots())
    if message:
        print(f"BLOCKED: {message}", file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
