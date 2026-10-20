#!/bin/zsh
set -eu
project_dir="$(cd "$(dirname "$0")/../.." && pwd)"
app_dir="$project_dir/Builds/Bat Carnival Tuner.app"
mkdir -p "$app_dir/Contents/MacOS"
cp "$project_dir/Tools/BatCarnivalTuner/Info.plist" "$app_dir/Contents/Info.plist"
xcrun swiftc -parse-as-library -O "$project_dir/Tools/BatCarnivalTuner/Tuner.swift" -o "$app_dir/Contents/MacOS/BatCarnivalTuner"
codesign --force --deep --sign - "$app_dir"
"$app_dir/Contents/MacOS/BatCarnivalTuner" --verify
