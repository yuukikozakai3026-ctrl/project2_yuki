using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// リザルトUI表示モジュール
/// architect_10-20_2.md 準拠:
///   - main() からスコアを受け取り、3D空間または画面上に最終スコアを表示
///   - 「完了（もう一度）」ボタンが押されたらコールバックで main() に通知
/// </summary>
public class ResultUIManager : MonoBehaviour
{
    [SerializeField] Font displayFont;
    GameObject resultPanel;
    bool closed;

    public void SetDisplayFont(Font font) { displayFont = font; }

    /// <summary>
    /// リザルト画面を表示し、プレイヤーが完了を押すまで待機するコルーチン
    /// </summary>
    /// <param name="score">最終獲得スコア</param>
    /// <param name="onClosed">完了ボタン押下時のコールバック</param>
    public IEnumerator ShowResultRoutine(int score, Action onClosed)
    {
        while (!Camera.main) yield return null;
        closed = false;
        resultPanel = HomeWorldUI.Panel(
            Camera.main,
            displayFont,
            "結果",
            $"獲得スコア  {score}",
            "完了",
            () => closed = true
        );

        while (!closed) yield return null;

        if (resultPanel) Destroy(resultPanel);
        resultPanel = null;

        onClosed?.Invoke();
    }
}
