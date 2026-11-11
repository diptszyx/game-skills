#!/usr/bin/env bash
set -u
MODE=EditMode
if [ "$#" -gt 0 ]; then
  if [ "$#" -ne 2 ] || [ "$1" != --mode ] || { [ "$2" != EditMode ] && [ "$2" != PlayMode ]; }; then
    echo 'Usage: run_unity_smoke.sh [--mode EditMode|PlayMode]'; exit 2
  fi
  MODE=$2
fi
PROJECT=$(cd "$(dirname "$0")/../unity-smoke" && pwd)
LOG="$HOME/Library/Logs/Unity/Editor.log"
RESULT="$PROJECT/Logs/$MODE.xml"
STALL=${UNITY_STALL_SECONDS:-180}
HARD=${UNITY_HARD_SECONDS:-2400}
[ "$HARD" -le 2400 ] || HARD=2400
if pgrep -fl 'Unity.app/Contents/MacOS/Unity'; then
  echo 'Refusing to run: Unity Editor already running.'; exit 1
fi
mkdir -p "$PROJECT/Logs"
rm -f "$RESULT"
start=$(date +%s)
echo "$start" > "$PROJECT/Logs/test-start.txt"
"$HOME/.unity/bin/unity" test "$PROJECT" --mode "$MODE" --output "$RESULT" --timeout "$HARD" --non-interactive > "$PROJECT/Logs/$MODE-cli.log" 2>&1 &
cli=$!
stop_unity() {
  pkill -f "Unity.app/Contents/MacOS/Unity.*$PROJECT" 2>/dev/null || true
  kill "$cli" 2>/dev/null || true
}
trap 'stop_unity; exit 130' INT TERM
last_change=$start
last_report=$start
last_count=-1
guard_status=0
while kill -0 "$cli" 2>/dev/null; do
  sleep 5
  now=$(date +%s)
  count=$(awk '!/Scanning for USB devices/{n++} END{print n+0}' "$LOG" 2>/dev/null) || count=0
  if [ "$count" != "$last_count" ]; then last_count=$count; last_change=$now; fi
  if [ $((now-last_report)) -ge 30 ]; then
    echo "[guard] $((now-start))s log_lines=$count idle=$((now-last_change))s"; last_report=$now
  fi
  if [ $((now-last_change)) -ge "$STALL" ] || [ $((now-start)) -ge "$HARD" ]; then
    echo '[guard] stalled or hard timeout'; stop_unity; guard_status=124; break
  fi
done
wait "$cli"
status=$?
[ "$guard_status" -eq 0 ] || status=$guard_status
if [ -f "$LOG" ]; then cp "$LOG" "$PROJECT/Logs/$MODE-Editor.log"; fi
python3 - "$RESULT" "$LOG" "$status" "$MODE" <<'PY'
import sys, pathlib, json, subprocess, xml.etree.ElementTree as ET
expected = {'IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaFullScreen', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingProgressClamp', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingProgressMonotonic', 'IndieGame.Shell.Tests.UnityAdapterTests.HapticsFlagOn', 'IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaBottomGestureBar', 'IndieGame.Shell.Tests.UnityAdapterTests.HapticsNullPlayerSafe', 'IndieGame.Shell.Tests.SaveSmokeTests.CorruptFallsBackToBackup', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingNewCycleResetsAndEarlyHideFades', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingMinimumDisplayTime', 'IndieGame.Shell.Tests.SaveSmokeTests.ValidV1Loads', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingHideBeforeMinimumWaits', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingDoubleShowIdempotent', 'IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaZero', 'IndieGame.Shell.Tests.SaveSmokeTests.FutureReportsVersionError', 'IndieGame.Shell.Tests.UnityAdapterTests.HapticsFlagOff', 'IndieGame.Shell.Tests.UnityAdapterTests.LoadingFadeAndLateHide', 'IndieGame.Shell.Tests.UnityAdapterTests.AudioFlagsGatePoolAndRefreshActiveVolumes', 'IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaTopNotch', 'IndieGame.Shell.Tests.UnityAdapterTests.AudioRoutesKeysAndIndependentVolumes'}
expected.update({'IndieGame.Shell.Tests.ShellUiFactoryTests.' + n for n in ['BuildsAllPrimitives','MissingRoleFailsByName','LockedButtonUsesLockedSprite','StyleSwitchKeepsTextAndLayout']})
expected.add('IndieGame.Shell.Tests.ShellUiDependencyTests.RuntimeAssemblyHasNoEditorReferences')
if sys.argv[4] == 'PlayMode':
    expected = {'IndieGame.Shell.Tests.PopupHostTests.' + n for n in ['OnlyTopPopupReceivesInput','ReopenLowerMovesToTop','MandatoryPopupRejectsDismissal','TransitionsWorkWhilePaused','PopRestoresFocus']}
print('mode=' + sys.argv[4])
core = pathlib.Path(sys.argv[1]).parents[2] / 'Packages/com.indiegame.shell/Runtime/Core'
no_engine = json.loads((core / 'IndieGame.Shell.Core.asmdef').read_text()).get('noEngineReferences') is True
grep = subprocess.run(['grep', '-rn', 'UnityEngine', str(core)], capture_output=True, text=True)
core_clean = grep.returncode == 1
print(f'Core noEngineReferences={no_engine}; UnityEngine grep matches={len(grep.stdout.splitlines())}; grep_status={grep.returncode}')
log = pathlib.Path(sys.argv[2])
errors = sum('error CS' in line for line in log.read_text(errors='replace').splitlines()) if log.exists() else -1
print(f'error CS count: {errors}')
try:
    cases = list(ET.parse(sys.argv[1]).getroot().iter('test-case'))
except (OSError, ET.ParseError) as error:
    print(f'passed=0 failed=unknown expected={len(expected)}; no readable results: {error}'); sys.exit(1)
passed = sum(c.get('result') == 'Passed' for c in cases)
failed = sum(c.get('result') == 'Failed' for c in cases)
print(f'passed={passed} failed={failed} expected={len(expected)}')
for c in cases:
    print(c.get('result'), c.get('fullname'))
    if c.get('result') != 'Passed': print(c.findtext('.//message', ''))
sys.exit(0 if no_engine and core_clean and errors == 0 and failed == 0 and passed == len(expected)
         and {c.get('fullname') for c in cases} == expected and int(sys.argv[3]) == 0 else 1)
PY
result=$?
end=$(date +%s)
echo "wall_seconds=$((end-start)) cli_status=$status runner_status=$result"
printf '%s\n' "$((end-start))" > "$PROJECT/Logs/test-wall-seconds.txt"
exit "$result"
