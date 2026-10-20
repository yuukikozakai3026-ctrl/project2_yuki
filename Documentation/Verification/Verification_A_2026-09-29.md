# A案「バット・カーニバル」検証記録（2026-09-29）

> この記録は改訂前のA案です。最新は [改訂A案の実装・検証](Verification_Revised_A_2026-09-29.md) を参照してください。

## 配布物

- 編集可能な Unity 6000.5.2f1 プロジェクト: `/Volumes/UnityAPFSWorkspace/Projects/VRBatCarnivalWorkshop`。実体は Transcend 内の `Unity/UnityAPFSWorkspace.sparsebundle`。
- macOS PC自動デモ: `Builds/VRBat.app`。Transcend 内の `VRBatCarnivalWorkshop/Play.command` で起動。
- Windows 64-bit OpenXR ビルド: `/Volumes/Transcend/Unity/VRBatCarnivalWorkshop/BatCarnival_WindowsVR.zip`。ZIP展開検査済み。SHA-256: `fb89acecfc9885f9065ab746d32d9630027f6cf80a6ea2253ab0206ed7a72d00`。
- 発射距離変更後の Mac/Windows の Unity BuildReport はともに `Succeeded`。ログは `Logs/build-launch-distance-v3.log` と `Logs/build-windows-launch-distance.log`。両ビルドとも幾何チェックを通過。

## 実行検証

| ケース | 結果 | 証拠 |
| --- | --- | --- |
| A本番2周連続 | 各周490点。種類別成功数14/4/4/3/3。終了と再初期化を確認 | [FinalA](FinalA/) のCSV・26枚の画像 |
| 発射距離変更後のMacビルドでA本番1周 | 490点、種類別成功数14/4/4/3/3、平均58.14 FPS。メニューから結果まで14枚撮影。実行ログに例外なし | [LaunchDistanceFull](LaunchDistanceFull/) のCSV・画像・ログ |
| 導入で2回失敗 | 背景ロボット実演が入り、再試行後に進行 | [VisualReleaseFailureV4](VisualReleaseFailureV4/) のCSV・画像・ログ |
| 発射距離変更後の1.35mの目線 | 発射口と球、打席、発射機の脚と床の接地、花火を撮影。画面の崩れなし | [LaunchDistanceHeight135](LaunchDistanceHeight135/) の画像・ログ |
| 左利き、低い目線、連続見逃し | 左反転・1.35m・同種2回見逃し後の追加ヒントを記録。430点で完走 | [LeftHeightMiss](LeftHeightMiss/) のCSV |
| 目線高の範囲外 | 1.20m入力時に±1cm調整UIが表示され、重なりがないことを撮影 | [HeightOutOfRange](HeightOutOfRange/A-0-menu-menu.png) |

中央の球の発射 Z 位置を 3.69m から 7.38m、左右を 3.49m から 6.98m に変更した。発射機も球と同じ位置に移し、奥の金属標的とネット、打球の戻り先を合わせた。発射ログは中央 `z=7.38`、左右 `z=6.98` を記録している。近距離の減速区間、当たり判定、時間進行、得点は維持した。旧版での2周連続・失敗・左利きの検証結果は変更前の参考結果であり、変更後は上記の1周と低視点導入を再検証した。

## 最終画面の確認箇所

- [メニューと広い球場](LaunchDistanceFull/A-0-menu-menu.png)
- [中央の発射口から現れる球](LaunchDistanceFull/A-0-intro-launch-origin.png)
- [種類別の弾の輪郭](LaunchDistanceFull/A-0-play-bullet-incoming.png)
- [宝箱の輪郭と見え方](LaunchDistanceFull/A-0-play-chest-incoming.png)
- [得点と打球の短い残光](LaunchDistanceFull/A-0-play-band-hit.png)
- [ケーキの破片](LaunchDistanceFull/A-0-play-cake-fragments.png)
- [遠方ネットから上がる花火](LaunchDistanceFull/A-0-play-firework-far.png)
- [宝箱の開封](LaunchDistanceFull/A-0-play-chest-open.png)
- [結果画面](LaunchDistanceFull/A-0-results-results.png)
- [失敗後の背景ロボット実演](VisualReleaseFailureV4/A-0-intro-background-demo.png)
- [低い目線での発射口](LaunchDistanceHeight135/A-0-intro-launch-origin.png)
- [低い目線での打席と花火](LaunchDistanceHeight135/A-0-countdown-firework-far.png)

視覚改善として、種類別の標的輪郭、接近時の輪郭縮小、発射機の支柱と明るい正面、打点から離れた得点表示、短い打球残光、横に広がるケーキ破片と断面、空中に収まる花火、宝箱の影の整理、手前の芝の連続性を確認した。画面演出の評価エージェントは距離変更後の通常14枚と発射直後の画像を厳しく確認し、発射位置、球・花火・標的・ネット・スコア板の構図と視認性にP1/P2の残件なしと判定した。静止画では接近の時間感覚を評価できない。説明文の加筆は行っていない。

## 未検証

VRヘッドセットと実コントローラでの到達範囲、装着感、入力・振動、Windowsでの実行性能は未検証。静止画の監査は動きの間やVR実機での見え方の保証ではない。
