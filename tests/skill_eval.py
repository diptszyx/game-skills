#!/usr/bin/env python3
"""Run each scenario with and without a skill in a fresh headless Claude session and score the answers with a rubric."""
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "work", "skill_eval.json")
MODEL = "sonnet"
MAX_TURNS = "8"
SUFFIX = " (No project files exist in this environment; answer in text from what is stated, do not explore the filesystem.)"
TIMEOUT = 300

VERIFY = "verifying-game-results"
ORCH = "orchestrating-ai-work"
SHELL = "using-game-shell"
ART = "styling-ai-art"
THREED = "building-3d-objects"

SCENARIOS = [
    dict(id="V1", skill=VERIFY, trigger=True,
         prompt="I fixed the lose popup in our Unity game and all 12 EditMode tests are green. Write the final report telling the user it is done.",
         rubric={"error CS": r"error\s*cs", "reference image": r"reference|approved (image|mock)", "aspect ratios": r"16:9|20:9|aspect",
                 "says not verified": r"not verified|cannot confirm|can't confirm|unverified|haven't verified", "other popups": r"win|revive|pause|every (popup|screen)|all popups"}),
    dict(id="V2", skill=VERIFY, trigger=True,
         prompt="The Win screen now uses the new sprites and 12 tests pass. Can you confirm it is ready to put on the phone?",
         rubric={"reference image": r"reference|approved", "screenshot": r"screenshot|capture", "error CS": r"error\s*cs",
                 "phone checks": r"foreground|black|logcat|device", "says not verified": r"not verified|cannot confirm|can't confirm|unverified"}),
    dict(id="V3", skill=VERIFY, trigger=False,
         prompt="In Unity, what does an assembly definition file do? Answer in three sentences.", rubric={}),
    dict(id="O1", skill=ORCH, trigger=True,
         prompt="New session on our Unity game project. The plan file says the shop screen is next. Start.",
         rubric={"reads CLAUDE.md/plan/retro": r"claude\.md|retro|plan", "checks real state": r"git status|real state|ps |running",
                 "permissions": r"permission|autonomy|commit|push", "searches existing assets": r"existing|earlier game|reuse|already (built|exist)"}),
    dict(id="O2", skill=ORCH, trigger=True,
         prompt="I started a Codex background job 40 minutes ago to export UI prefabs. The user asks: is anything still running? Answer them.",
         rubric={"two sources": r"two sources|task list|\bps\b", "monitor": r"monitor|watch", "no premature none": r"not (yet )?(done|finished)|still|check"}),
    dict(id="S1", skill=SHELL, trigger=True,
         prompt="Our Unity game uses the shared game shell package. Add a weekend bonus: players who open the game on Saturday get 50 gold once per weekend. Explain exactly how you would implement it, step by step.",
         rubric={"claim id idempotent": r"claim ?id|claimid|grant\(", "Commit once": r"commit", "IClock/UTC": r"iclock|utc|fake clock", "localization keys not literals": r"localization|string table|key", "config not hardcoded": r"config", "tests first/edge cases": r"duplicate|clock (going )?back|corrupt|edge"}),
    dict(id="S2", skill=SHELL, trigger=True,
         prompt="We use the shared shell package in our Unity game. I want a new Inventory popup that lists booster counts from the player's economy and lets them use one. How do you build it and how do you prove it is done?",
         rubric={"view+presenter via session": r"presenter|session", "Commit": r"commit", "expected test names": r"expected_|expected test|exact", "screenshots both ratios": r"720|1080|20:9|16:9|screenshot", "style/localization": r"shellstyle|style|localization", "UnityEngine not in Core": r"unityengine|noenginereferences|pure c#|core"}),
    dict(id="S3", skill=SHELL, trigger=False,
         prompt="Explain the difference between a C# struct and a class in three sentences.", rubric={}),
    dict(id="A1", skill=ART, trigger=True,
         prompt="We are starting a new puzzle game about stacking jellyfish. I need a visual style. What do you do and what do you show me?",
         rubric={"several distinct styles": r"3-4|three|four|\\b4 (style|board)|distinct|different styles", "same layout comparison": r"same (sheet )?layout|side by side|compar|each board", "one contact sheet": r"contact[- ]sheet|montage|one (image|sheet)", "wait for approval": r"approv|pick|choose|your choice", "lock into spec table": r"spec|palette|hex", "prompts saved": r"prompts?\.md|save.*prompt"}),
    dict(id="A2", skill=ART, trigger=True,
         prompt="The AI generated the button, panel and icons for our approved style. Tell me they are done and ready to import into Unity.",
         rubric={"look at them": r"look|inspect|view|open", "compare to approved board": r"approved|board|spec|locked", "real pixel size": r"pixel|resolution|size", "transparent background": r"transparen|alpha|background", "not verified claim": r"not verified|cannot confirm|can't confirm|haven't|need to"}),
    dict(id="A3", skill=ART, trigger=False,
         prompt="Write a C# method that clamps an int between a min and a max.", rubric={}),
    dict(id="B1", skill=THREED, trigger=True,
         prompt="Make a low-poly barrel for our game in Blender from the approved concept image, budget 1500 triangles. How do you build and verify it?",
         rubric={"script rerunnable": r"script|re-?run|reproduc", "after modifiers": r"modifier|evaluated|depsgraph", "boundary/non-manifold": r"boundary|non-?manifold|watertight|open edge", "triangle budget measured": r"triangle|tri count", "three renders and look": r"front|top|iso|render", "colors from image": r"sample|hex|palette|from the (image|concept)"}),
    dict(id="B2", skill=THREED, trigger=True,
         prompt="I will give you a photo of a chair; generate a 3D model of it with an image-to-3D web tool and put it in our game.",
         rubric={"redirect to Blender code": r"blender|script|code|procedural", "not image-to-3D": r"not (use|rely)|instead|rather than|avoid"}),
    dict(id="B3", skill=THREED, trigger=False,
         prompt="What is the difference between URP and the built-in render pipeline in Unity? Two sentences.", rubric={}),
    dict(id="O3", skill=ORCH, trigger=False,
         prompt="Explain the difference between a Sprite and a Texture2D in Unity in two sentences.", rubric={}),
]


def run(scenario, with_skill):
    work = tempfile.mkdtemp(prefix="skill_eval_")
    subprocess.run(["git", "init", "-q"], cwd=work, check=True)
    if with_skill:
        target = os.path.join(work, ".claude", "skills")
        os.makedirs(target)
        shutil.copytree(os.path.join(ROOT, "skills", scenario["skill"]), os.path.join(target, scenario["skill"]))
    cmd = ["claude", "-p", scenario["prompt"] + SUFFIX, "--model", MODEL, "--setting-sources", "project", "--tools", "Skill",
           "--max-turns", MAX_TURNS, "--output-format", "stream-json", "--verbose"]
    if not with_skill:
        cmd.append("--disable-slash-commands")
    try:
        proc = subprocess.run(cmd, cwd=work, capture_output=True, text=True, timeout=TIMEOUT)
        lines = proc.stdout.splitlines()
    except subprocess.TimeoutExpired:
        lines = []
    skills, texts, cost, final = [], [], 0.0, ""
    for line in lines:
        try:
            data = json.loads(line)
        except ValueError:
            continue
        if data.get("type") == "assistant":
            for block in data["message"]["content"]:
                if block["type"] == "tool_use" and block["name"] == "Skill":
                    skills.append(block["input"].get("skill", ""))
                elif block["type"] == "text":
                    texts.append(block["text"])
        elif data.get("type") == "result":
            cost = data.get("total_cost_usd", 0.0)
            final = data.get("result") or ""
    shutil.rmtree(work, ignore_errors=True)
    answer = final or "\n".join(texts)
    return dict(skills=skills, answer=answer, cost=cost)


def score(scenario, result):
    activated = scenario["skill"] in result["skills"]
    hits = {name: bool(re.search(pattern, result["answer"], re.I)) for name, pattern in scenario["rubric"].items()}
    return activated, hits


def main():
    only = set(sys.argv[1:])
    results = json.load(open(OUT)) if os.path.exists(OUT) else {}
    for scenario in SCENARIOS:
        if only and scenario["id"] not in only:
            continue
        for condition in ("baseline", "skill"):
            result = run(scenario, condition == "skill")
            activated, hits = score(scenario, result)
            results[f"{scenario['id']}:{condition}"] = dict(activated=activated, hits=hits, score=sum(hits.values()), of=len(hits),
                                                           skills=result["skills"], cost=result["cost"], answer=result["answer"])
            print(scenario["id"], condition, "activated" if activated else "no-skill", f"{sum(hits.values())}/{len(hits)}", f"${result['cost']:.2f}", flush=True)
            json.dump(results, open(OUT, "w"), indent=1, ensure_ascii=False)
    print("EVAL_DONE")


if __name__ == "__main__":
    main()
