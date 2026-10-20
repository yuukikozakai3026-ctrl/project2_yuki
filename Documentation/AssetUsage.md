# Asset usage and licences

Seven Kenney packs were freshly downloaded from their official pages. Per-file paths, SHA-256 hashes and reference matches are in `AssetAcquisition.json`. Each pack's CC0 `License.txt` remains under `Assets/Kenney/<pack>/License.txt`.

| Pack | A-game use |
| --- | --- |
| food-kit | Cake slice and strawberry fragments. |
| pirate-kit | Cannon-ball base mesh for ball, firework and toy bullet; chest and its animation. |
| factory-kit | Three compact launchers and the robot in the background tutorial demonstration. |
| particle-pack | Spark, star and smoke sprites for hits, bursts and finale. |
| impact-sounds | Bat impacts, metal, bell, soft and chest sounds. |
| interface-sounds | Menu, countdown and confirmation sounds. |
| ui-pack | World panel frame and selected-hand checkmark. |

The 210 m × 190 m night field, light towers, outfield, target trails and shockwaves are generated in Unity. The original synthesized stadium rhythm is reproducible with `Tools/generate_stadium_music.py`. Noto Sans JP provenance is in `FontAcquisition.json`; source-audio processing is in `AudioProcessing.json`. Assets for the abandoned B proposal may remain in the Unity project and provenance record, but are not placed in the active stage.

## 改訂A案で追加・再利用した表現（2026-09-29）

- 既存プロジェクトへ正式導入済みのKenneyのケーキ、苺、宝箱、球、粒子テクスチャ、打撃音を引き続き使用。旧プロジェクトからの追加コピーは行っていない。
- 新しい毒瓶、ブーメラン、真鍮の銃弾、魔法球は `PitchVisuals.cs` の独自プロシージャル形状。反応を読みやすくするために瓶のラベル・翼の先端模様・本物の色差を追加。
- `poison_jet.wav`、`boomerang_whoosh.wav`、`fuse_crackle.wav`、`magic_dissolve.wav` は今回作成した合成音。外部音源の転用なし。
- 噴流・火花・偽物の透過・遠方への返球は `RevisedEffects.cs`。ゲーム画面と音声付き録画で確認。HMD実機での見え方と音量は未確認。

## 打撃反応改訂（2026-09-29）

`HitReactions.cs` のスポンジ・ガラス・コイン・宝石・光の軌跡は独自生成形状。既に導入済みの同プロジェクトの粒子/苺/宝箱/打撃音を再利用し、外部素材の追加取得はない。`flight_air.wav` と `crowd_claps.wav` は `Tools/generate_reaction_audio.py` で生成したオリジナル合成音。実際の群衆の音声録音ではない。現在の通常投球では常設の発射装置・枠・支柱を表示しない。発射時だけ短い合図を表示する。使用条件・ライセンスの新規変更なし。

## 2026-09-29 演出増強
既存Kenney particle-pack の spark_01/star_01/light_01/trace_01 を再利用。StadiumSpectacle（発光スタンドと照明）、SpectacleEffects（まとめて描画する粒子と花火）、Resources/FestivalParticles.shader は本プロジェクト用の自作。第三者素材の追加取得なし。
