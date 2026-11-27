#!/usr/bin/env bash
set -euo pipefail

# Pinned public mirror commit of Kenney Mini Dungeon 2.0.
# The original Kenney License.txt is downloaded alongside the model files.
readonly SOURCE_COMMIT="fc2cd355a8e7c1d8e625fd650abf64f50a1fddaa"
readonly SOURCE_ROOT="https://raw.githubusercontent.com/eturner58/game-assets/${SOURCE_COMMIT}/kenney/3D%20assets/Mini%20Dungeon"
readonly OUTPUT_ROOT="${1:-Unity/Assets/NemesisContent/Imported/KenneyMiniDungeon}"
readonly MODEL_ROOT="${OUTPUT_ROOT}/Models/GLB format"
readonly TEXTURE_ROOT="${MODEL_ROOT}/Textures"

mkdir -p "${MODEL_ROOT}" "${TEXTURE_ROOT}"

models=(
  banner barrel chair character-human character-orc chest coin column dirt
  floor-detail floor gate key pot potion rocks shield-rectangle shield-round
  stairs stones table trap wall-half wall-narrow wall-opening wall
  weapon-spear weapon-sword wood-structure wood-support
)

for model in "${models[@]}"; do
  curl --location --fail --retry 3 --retry-delay 2 --connect-timeout 20 --max-time 120 \
    "${SOURCE_ROOT}/Models/GLB%20format/${model}.glb" \
    --output "${MODEL_ROOT}/${model}.glb"
  test -s "${MODEL_ROOT}/${model}.glb"
done

curl --location --fail --retry 3 --retry-delay 2 --connect-timeout 20 --max-time 60 \
  "${SOURCE_ROOT}/Models/GLB%20format/Textures/colormap.png" \
  --output "${TEXTURE_ROOT}/colormap.png"
curl --location --fail --retry 3 --retry-delay 2 --connect-timeout 20 --max-time 60 \
  "${SOURCE_ROOT}/License.txt" \
  --output "${OUTPUT_ROOT}/License.txt"

actual_count="$(find "${MODEL_ROOT}" -maxdepth 1 -type f -iname '*.glb' | wc -l | tr -d ' ')"
if [ "${actual_count}" -ne "${#models[@]}" ]; then
  echo "::error::Expected ${#models[@]} GLB files, found ${actual_count}."
  exit 1
fi

cat > "${OUTPUT_ROOT}/ASSET_SOURCE.md" <<EOF
# Kenney Mini Dungeon 2.0

- Creator: Kenney (https://kenney.nl)
- Original pack page: https://kenney.nl/assets/mini-dungeon
- License: Creative Commons Zero 1.0 Universal (CC0)
- Original creator license file: License.txt (included beside this note)
- Public mirror used by CI: https://github.com/eturner58/game-assets
- Mirror commit pinned by the download script: ${SOURCE_COMMIT}

The original creator identifies this pack as CC0. The included License.txt states that the content may be used for personal, educational and commercial purposes, with credit optional. The pack contains 30 GLB models and its shared colormap texture.

Optional credit: "Mini Dungeon by Kenney (kenney.nl) — CC0".
EOF

echo "Imported Kenney Mini Dungeon 2.0: ${actual_count} GLB models and the shared colormap."
