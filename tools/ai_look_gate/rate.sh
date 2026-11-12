#!/bin/bash
# usage: rate.sh <blind_dir> <out_dir>   (blind rater: Claude, sees one image at a time)
set -u
BLIND="$1"; OUT="$2"; mkdir -p "$OUT"
RUB="$(cd "$(dirname "$0")" && pwd)/rubric.md"
rate_one() {
  f="$1"; id=$(basename "$f" .png)
  cd "$OUT" || exit 1
  claude -p --setting-sources project --disable-slash-commands --tools Read --permission-mode bypassPermissions --max-turns 6 --output-format json \
    "$(cat "$RUB")

Image path: $f" > "$OUT/$id.raw.json" 2>"$OUT/$id.err"
}
export -f rate_one; export RUB OUT
ls "$BLIND"/img_*.png | xargs -P 3 -I{} bash -c 'rate_one {}'
