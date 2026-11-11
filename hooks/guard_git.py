#!/usr/bin/env python3
import json
import re
import sys

FORBIDDEN = [
    (r"\bgit\b[^|;&]*\breset\b[^|;&]*--hard", "git reset --hard is forbidden; use git reset --soft"),
    (r"\bgit\b[^|;&]*\bpull\b[^|;&]*(--rebase|\s-r\b)", "git pull --rebase is forbidden; use git pull (merge)"),
    (r"\bgit\b[^|;&]*\bpush\b[^|;&]*(--force\b|--force-with-lease|\s-f\b)", "force push is forbidden"),
]


def check(command):
    for pattern, message in FORBIDDEN:
        if re.search(pattern, command):
            return message
    return None


def main():
    data = json.load(sys.stdin)
    if data.get("tool_name") != "Bash":
        return 0
    message = check(data.get("tool_input", {}).get("command", ""))
    if message:
        print(f"BLOCKED: {message}", file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
