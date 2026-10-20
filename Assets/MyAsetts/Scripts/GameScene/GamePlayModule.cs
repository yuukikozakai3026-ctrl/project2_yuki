using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRBat;

/// <summary>
/// ゲームモジュール (GamePlayModule)
/// architect_10-20_2.md 準拠:
///   - 外部マネージャーとして BatGame (VRBat) および PitcherController を統括し、調停と時間管理を行う。
///   1. 初期化と統合モード起動:
///      BatGame を取得し integratedMode = true に設定、StartIntegratedRound() を呼び出す。
///   2. ピッチャーの位置合わせと投球スケジュールの実行:
///      ピッチャーの腕振り (BeginThrow) と VRBat の球種生成・投球 (SpawnManualPitch) を同期実行。
///   3. 時間終了・タイムアップ処理:
///      BatGame の phase == "results" を検知後、全ピッチャーの投球動作を停止 (StopPitching)。
///      タイムアップUIを1.5秒間表示し、余韻待機後に main() へ制御を戻す。
///   4. スコア取得:
///      GetCurrentScore() により最終スコアを返却。
/// </summary>
public class GamePlayModule : MonoBehaviour
{
    [SerializeField] List<PitcherController> pitchers = new List<PitcherController>();
    [SerializeField] BatGame battingGame;

    PitcherController pitcher;

    /// <summary>
    /// 現在の獲得スコアを取得
    /// </summary>
    public int GetCurrentScore() => battingGame ? battingGame.score : 0;

    public void SetBattingGame(BatGame game) { battingGame = game; }
    public void SetPitchers(IEnumerable<PitcherController> sources) { pitchers = new List<PitcherController>(sources); }

    void Start()
    {
        SelectPitcher();
    }

    void SelectPitcher()
    {
        pitcher = pitchers.Find(p => p && p.Lane == 0);
        if (!pitcher) pitcher = pitchers.Find(p => p);
        foreach (var p in pitchers) {
            if (p && p != pitcher) p.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ゲーム本編のメインルーチン
    /// </summary>
    public IEnumerator RunGameRoutine()
    {
        if (!battingGame) battingGame = FindFirstObjectByType<BatGame>();
        while (!battingGame || !battingGame.view || !battingGame.view.eye) yield return null;

        SelectPitcher();
        if (!pitcher) {
            Debug.LogError("[GamePlayModule] 投手が見つかりません");
            yield break;
        }

        foreach (var p in pitchers) {
            if (p && p != pitcher) p.gameObject.SetActive(false);
        }

        // 1. 統合モードの開始
        battingGame.integratedMode = true;
        battingGame.StartIntegratedRound();

        // ピッチャーのリリース位置を調整
        pitcher.AlignReleasePoint(battingGame.W(new Vector3(0, 1.71f, battingGame.tuning.launchDistance)));

        // 2. 投球ループの実行
        foreach (var plan in battingGame.pitchPlan) {
            // 次の投球予定時刻の直前（腕振りモーション開始タイミング）まで待機
            while (battingGame.phase == "play" && battingGame.now - battingGame.runAt < plan.launch - .275f) {
                yield return null;
            }

            if (battingGame.phase != "play") break;

            bool released = false;

            // 腕振りアニメーション実行
            pitcher.BeginThrow(point => {
                if (battingGame.phase == "play") {
                    battingGame.SpawnManualPitch(plan);
                    battingGame.Event("pitcher_release",
                        $"pitcher={pitcher.PitcherName};id={plan.id};x={point.x:F3};y={point.y:F3};z={point.z:F3}",
                        plan.bomb ? "bomb" : "ball");
                }
                released = true;
            }, () => battingGame.paused ? 0 : 1);

            // リリース完了またはゲーム状態変化まで待機
            while (!released && battingGame.phase == "play") yield return null;
        }

        // 3. ラウンド終了（phase == "results"）まで待機
        while (battingGame.phase != "results") yield return null;

        // 4. ピッチャー停止
        pitcher.StopPitching();

        // 5. タイムアップ表示（1.5秒）
        yield return StartCoroutine(ShowTimeUpRoutine());

        // 余韻待機
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 「タイムアップ！」の3Dパネル演出
    /// </summary>
    IEnumerator ShowTimeUpRoutine()
    {
        if (battingGame && battingGame.view && battingGame.view.eye) {
            var panel = battingGame.view.Panel(
                "TimeUp panel",
                battingGame.W(new Vector3(0, 1.65f, 2.0f)),
                new Vector2(.9f, .45f),
                battingGame.transform
            );
            var label = battingGame.view.Label(panel, "タイムアップ！", Vector2.zero, new Vector2(.85f, .35f), .10f);
            label.color = new Color(1f, .85f, .3f);

            yield return new WaitForSeconds(1.5f);

            if (panel) Destroy(panel.gameObject);
        } else {
            yield return new WaitForSeconds(1.5f);
        }
    }
}