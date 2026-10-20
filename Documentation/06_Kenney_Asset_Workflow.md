# Kenney素材の取得・導入指示

## 1. 添付カタログをそのまま使う

ユーザーのZIPは素材本体ではなく検索カタログと運用手順である。`catalog/user_supplied_catalog.zip`は元の添付物。展開して使いやすくしたJSON・検索スクリプト・指示書も同梱した。READMEのファイル名だけ英語表記へ揃えた。

新規プロジェクトの実際の構成を見てカタログの配置を決める。既存のAGENTS等には`AGENTS_SNIPPET.md`を重複なく追記し、上書きしない。旧プロジェクトの絶対パスを引き継がない。`search_assets.py`とJSONは同じ場所へ置くか、`--catalog`でそのプロジェクトの実際のJSONを指定する。

今回の指示書で素材の選択は済んでいる。まず指定素材を取り込み、一般的な候補探しからやり直さない。ファイルが配布更新で消えた場合だけ、短い日本語/英語の名詞で上位3件を確認、0件なら同義語を一回、その後Kenney公式サイトを確認する。新しい候補を見つけてもゲームの見た目を変更してよいという意味ではない。

## 2. 新規取得と確認の手順

1. 以下の公式ページを開き、そのページの現在の通常ダウンロード経路から新しく取得する。過去のプロジェクト、以前の展開物、本指示書作成時の研究用ZIPを流用しない。
2. 無料取得を使う。購入・寄付・有料パックへの切替は今回の依頼に含まれない。
3. ZIPのSHA-256と取得日、配布物のLicense.txtを記録する。assets/asset_manifest.jsonのハッシュは**2026-09-28に著者が内容確認した版**の照合値。新規プロジェクトの取得済み証明ではない。
4. ハッシュが違っても即危険扱いはせず、指定ファイル名、形、ノード構成、依存関係、ライセンスに変更があるか確認する。等価なら変更内容を記録して採用。見た目やルールに関わる変更なら該当素材の代替案を示し、その他の実装は進める。
5. 指定ファイルと依存ファイルだけを取り込む。7パックの全素材・全形式・デモシーンをUnityへ一括導入しない。
6. モデルを正規化して指定寸法へ合わせ、A/Bの実際のカメラから表示を確認する。粒子は暗/明背景で透過縁を確認する。音は実際に再生する。
7. 新規プロジェクト用の導入記録へ実際の相対パス、対象シーン、変更した材質/寸法、確認結果を保存する。`local.status`は取得までdownloaded、導入・動作を確認して初めてimported。著者の確認を転記してimportedにしない。

同梱カタログ34件の`local`は元のunknownを保持している。今回採用したが元カタログにないパックは、プロジェクト側で追加するか別導入記録にリンクする。既存カタログ全文をAGENTSや会話へ貼らない。

## 3. 採用パックと実ファイル

各ファイルのSHA-256、パックZIPハッシュ、確認時のダウンロードURLは`assets/asset_manifest.json`。以下の公式ページを恒久的な起点にする。一時ZIP URLを固定ダウンロード元としてコードへ埋め込まない。

| パック/公式URL | 取り込むファイル | 使用場所 |
| --- | --- | --- |
| [Food Kit](https://kenney.nl/assets/food-kit) | Models/GLB format/cake.glb、plate.glb、strawberry.glb | A一切れ、B皿付きホール、飛散するイチゴ |
| [Pirate Kit](https://kenney.nl/assets/pirate-kit) | Models/GLB format/chest.glb、cannon-ball.glb | 両案の箱、球形対象の母体 |
| [Factory Kit](https://kenney.nl/assets/factory-kit) | Models/GLB format/floor.glb、machine.glb、machine-connection-hole.glb、robot-arm-a.glb、pipe-large.glb | 床、発射機、背景設備、見本機構、煙突、供給管 |
| [Particle Pack](https://kenney.nl/assets/particle-pack) | PNG (Transparent)/circle_01.png、spark_01.png、star_01.png、smoke_01.png、trace_01.png、light_01.png | 打撃、報酬、花火、煙、短い軌跡 |
| [Impact Sounds](https://kenney.nl/assets/impact-sounds) | 04の指定Audio/*.ogg 10ファイル | 打撃、皿、爆発の素材音 |
| [Interface Sounds](https://kenney.nl/assets/interface-sounds) | 04の指定Audio/*.ogg 6ファイル | 予告、報酬、巻き取り、メニュー |
| [UI Pack](https://kenney.nl/assets/ui-pack) | PNG/Blue/Default/button_rectangle_flat.png、icon_checkmark.png | 板の枠と選択表示 |

3Dの3パックはそれぞれの**`Models/GLB format/Textures/colormap.png`**も必要。GLBだから外部画像なしで完結すると仮定しない。同名テクスチャを共通フォルダへまとめて上書きすると色が壊れるため、パック別に維持する。インポーターが画像を別形式へ変換しても、元ファイルと対応表を残す。

素材の使用率を上げるために不要な物を増やさない。見える主要な食べ物・宝箱・球形物・背景・粒子・音・UIへKenneyを使う。正確な操作形状が必要なバット、穴付き台、受け金具、カウント灯、金貨/宝石は寸法指定で補う。この分担も仕様として固定。

## 4. 実配布物で確認した点

- Food Kitのcakeは同名`slice`が6個。Aはscene rootのindex 0を抜き出せる。文字列検索で全sliceを削除/採用しない。
- 全cakeの外接寸法は約0.6392×0.273077×0.6392（元単位）。node 0は約0.3196×0.273077×0.3196。
- plateの外接寸法は約0.891774×0.09×0.891774。深皿`plate-dinner.glb`とは異なる。
- chestには`chest`と子の`lid`があり、`open`、`close`、`open-close`のアニメーションを収録。閉状態は約1.275×1.1475×1.275。ヒンジを無視して中心回転しない。
- cannon-ballは粗い球メッシュで、外接寸法は約0.637208×0.670000×0.551838。指定直径へ軸ごとの正規化をする。
- Factoryのfloorは厚さほぼ0の平面。床の衝突面は別の水平平面で用意し、Yの極小寸法で割ってスケールを求めない。
- 今回の7配布物それぞれのLicense.txtにCC0表記があることを確認した。コピーはreferences/licenses内。実装時も新しく取得した版を確認する。

`references/verified_models.png`は元のメッシュから作った静的な確認図であり、完成ゲームの照明・画質の見本ではない。描画確認だけでUnity動作確認済みとはしない。

## 5. 日本語フォント

KenneyのUI画像は日本語の文字を提供するものではない。字体はGoogle Fonts配布の[Noto Sans JP](https://github.com/google/fonts/tree/main/ofl/notosansjp)に固定する。

公式ディレクトリのMETADATA.pbで`NotoSansJP[wght].ttf`とweight軸100〜900を確認し、OFL.txtを読んだ。制作時は同ディレクトリからフォント本体とOFL.txtを取得し、ハッシュを導入記録へ追加する。配布元のURLと取得日を保持する。今回フォント本体のゲームへの導入は未実施。

フォントの変換/アトラス化方式はCodexが選ぶ。表示文字は日本語、数字、記号を欠落なく含める。テキストを画像化して文字化けを隠さない。版により可変フォントが直接使えない場合は500/700の静的インスタンスへ変換する。

## 6. 最終的な素材記録

プロジェクト側にAssetUsageの一覧を残す。1行に「対象ID、パック、公式ページ、元ファイル、プロジェクト内相対パス、加工内容、確認日、確認結果」を持たせる。元ライセンスは一緒に保存する。モデルを読み込めなかった、音を試聴できなかったなどの項目を空欄で済ませず未確認として残す。
