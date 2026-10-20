using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameSceneの進行管理モジュール = main()
/// architect_10-20_2.md 準拠:
///   1. チュートリアルモジュールを呼び出し、完了を待つ。
///   2. ゲーム開始モジュールを呼び出し、3カウントダウンの完了を待つ。
///   3. ゲームモジュール (GamePlayModule) を呼び出し、ゲーム本編の完了を待つ。
///   4. ゲームモジュールから最終スコアを取得し、GameData.FinalScore に格納する。
///   5. シーン切り替えモジュールを実行し、HomeScene へシーンを切り替える。
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    [Header("裏処理モジュール群")]
    [SerializeField] private TutorialModule tutorialModule;
    [SerializeField] private CountdownModule countdownModule;
    [SerializeField] private GamePlayModule gamePlayModule;

    [Header("設定")]
    [SerializeField] private string homeSceneName = "HomeScene";

    private IEnumerator Start()
    {
        // 未アサイン時のフォールバック取得
        if (!tutorialModule) tutorialModule = FindFirstObjectByType<TutorialModule>();
        if (!countdownModule) countdownModule = FindFirstObjectByType<CountdownModule>();
        if (!gamePlayModule) gamePlayModule = FindFirstObjectByType<GamePlayModule>();

        // 1. チュートリアル呼び出し
        if (tutorialModule) {
            Debug.Log("[GameSceneManager] 1. チュートリアルを開始します。");
            yield return StartCoroutine(tutorialModule.RunTutorialRoutine());
        }

        // 2. カウントダウン呼び出し
        if (countdownModule) {
            Debug.Log("[GameSceneManager] 2. カウントダウンを開始します。");
            yield return StartCoroutine(countdownModule.RunCountdownRoutine());
        }

        // 3. ゲーム本編呼び出し
        if (gamePlayModule) {
            Debug.Log("[GameSceneManager] 3. ゲーム本編を開始します。");
            yield return StartCoroutine(gamePlayModule.RunGameRoutine());
        }

        // 4. スコア取得 & 5. スコア保存
        int score = gamePlayModule ? gamePlayModule.GetCurrentScore() : 0;
        GameData.FinalScore = score;
        Debug.Log($"[GameSceneManager] 4&5. 最終スコアを取得しGameDataに保存しました: FinalScore = {GameData.FinalScore}");

        // 6. シーン切り替え実行
        Debug.Log($"[GameSceneManager] 6. {homeSceneName} へシーンを切り替えます。");
        SceneManager.LoadScene(homeSceneName);
    }
}