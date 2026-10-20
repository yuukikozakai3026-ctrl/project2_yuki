using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// HomeSceneのゲーム進行管理モジュール = main()
/// architect_10-20_2.md 準拠:
///   1. シーン起動時、GameData.FinalScore を参照する。
///      - FinalScore >= 0（ゲーム終了後）の場合:
///        スコアを渡し、リザルトUI表示モジュールを呼び出す。
///        リザルトUIが閉じた通知（完了ボタン）を受け取ったら、GameData.Reset() を実行してスタート画面へ進む。
///      - FinalScore == -1（初回起動時）の場合:
///        スタート画面（タイトル・開始ボタン等）を表示する。
///   2. プレイヤーの開始操作を受け取ったら、GameData.Reset() して GameScene へシーン遷移する。
/// </summary>
public class HomeManager : MonoBehaviour
{
    [SerializeField] ResultUIManager resultUI;
    [SerializeField] Font displayFont;
    GameObject startPanel;

    public void SetDisplayFont(Font font) { displayFont = font; }

    IEnumerator Start()
    {
        Debug.Log("[VR Startup] HomeManager.Start scene=" + gameObject.scene.name);

        if (Application.platform == RuntimePlatform.WindowsPlayer) {
            var settings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            if (settings && settings.Manager && !settings.Manager.isInitializationComplete) {
                yield return settings.Manager.InitializeLoader();
                if (settings.Manager.activeLoader) settings.Manager.StartSubsystems();
            }
        }

        if (!GetComponent<HomePointer>()) gameObject.AddComponent<HomePointer>();
        if (!resultUI) resultUI = GetComponent<ResultUIManager>();

        var cameraWaitStarted = Time.realtimeSinceStartup;
        while (!Camera.main) {
            if (Time.realtimeSinceStartup - cameraWaitStarted > 5f) {
                Debug.LogError("[VR Startup] Home has no active MainCamera");
                cameraWaitStarted = Time.realtimeSinceStartup;
            }
            yield return null;
        }
        Debug.Log("[VR Startup] Home camera ready: " + Camera.main.name);

        // architect_10-20_2.md 判定仕様:
        // FinalScore >= 0: ゲーム本編終了後のリザルト表示
        // FinalScore == -1: 初回起動時のスタート画面表示
        if (GameData.FinalScore >= 0) {
            Debug.Log($"[HomeManager] リザルト表示を開始します。FinalScore = {GameData.FinalScore}");
            if (resultUI) {
                yield return StartCoroutine(resultUI.ShowResultRoutine(GameData.FinalScore, OnResultClosed));
            } else {
                OnResultClosed();
            }
        } else {
            Debug.Log("[HomeManager] 初回起動のため、スタート画面を表示します。");
            InitializeHomeScreen();
        }
    }

    /// <summary>
    /// スタート画面（タイトル・開始ボタン等）を生成・表示する
    /// </summary>
    void InitializeHomeScreen()
    {
        if (startPanel) Destroy(startPanel);
        startPanel = HomeWorldUI.Panel(
            Camera.main,
            displayFont,
            "バット・カーニバル",
            "ボールを打とう · 爆弾は避けよう",
            "開始",
            StartGame
        );
        Debug.Log("[VR Startup] Home presentation ready");
    }

    /// <summary>
    /// 「開始」ボタン押下時の処理: スコアをリセットしてGameSceneへ遷移
    /// </summary>
    void StartGame()
    {
        Debug.Log("[VR Startup] Loading GameScene");
        if (startPanel) Destroy(startPanel);
        GameData.Reset();
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// リザルトUIの「完了」ボタン押下時の処理: スコアをリセットしてスタート画面へ復帰
    /// </summary>
    void OnResultClosed()
    {
        Debug.Log("[HomeManager] リザルト画面が閉じられました。スタート画面へ復帰します。");
        GameData.Reset();
        InitializeHomeScreen();
    }
}
