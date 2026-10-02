#!/usr/bin/env bash
# Bygger spelet för Windows och Linux, och pushar till itch.io om man ger en version.
#
# Bara bygga:        ./scripts/publish_local.sh
# Bygga och pusha:   ./scripts/publish_local.sh 0.3.0
set -euo pipefail

ITCH_TARGET="marcmed/c64froggernostalgia"
cd "$(dirname "$0")/.."

rm -rf build
dotnet publish src/Frogger -c Release -r win-x64 -o build/win-x64
dotnet publish src/Frogger -c Release -r linux-x64 -o build/linux-x64

if [ $# -ge 1 ]; then
  VERSION="$1"
  butler push build/win-x64 "$ITCH_TARGET:windows" --userversion "$VERSION"
  butler push build/linux-x64 "$ITCH_TARGET:linux" --userversion "$VERSION"
else
  echo "Byggt i build/. Ange en version för att även pusha till itch.io."
fi
