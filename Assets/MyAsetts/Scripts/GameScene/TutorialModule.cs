using System.Collections;
using UnityEngine;
using VRBat;

/// <summary>
/// チュートリアルモジュール
/// architect_10-20_2.md 準拠:
///   1. チュートリアル案内UIを表示する。
///   2. プレイヤーの開始操作（プレイ開始ボタン）を受け取る。
///   3. 終了通知を行い完了する。
/// </summary>
public class TutorialModule : MonoBehaviour
{
    public void SetSkipTutorialForDebug(bool skip) { }

    public IEnumerator RunTutorialRoutine()
    {
        var game = FindFirstObjectByType<BatGame>();
        while (!game || !game.view || !game.view.eye) {
            yield return null;
            game = FindFirstObjectByType<BatGame>();
        }

        bool ready = false;
        var panel = game.view.Panel(
            "Player ready",
            game.W(new Vector3(0, 1.5f, 1.8f)),
            new Vector2(1.35f, .70f),
            game.transform
        );

        game.view.Label(panel, "バッティング", new Vector2(0, .23f), new Vector2(1.20f, .10f), .07f);

        string control = game.desktopInput
            ? "マウスで打点を移動\n左ボタンをためて離すとスイング"
            : "バットを振ってボールを打つ";

        game.view.Label(panel, control + "\n爆弾は避ける · 金色の帯で速度ボーナス",
            new Vector2(0, .025f), new Vector2(1.20f, .27f), .042f);

        game.view.Button(panel, "プレイ開始", new Vector2(0, -.22f), new Vector2(.55f, .13f), () => ready = true);

        while (!ready) yield return null;

        if (panel) Destroy(panel.gameObject);
    }
}