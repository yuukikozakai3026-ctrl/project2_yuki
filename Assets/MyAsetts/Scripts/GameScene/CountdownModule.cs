using System.Collections;
using UnityEngine;
using VRBat;

/// <summary>
/// ゲーム開始カウントダウンモジュール
/// architect_10-20_2.md 準拠:
///   - ゲーム開始のカウントダウン（3カウント）を行う。
///   - カウントダウン演出（数字UI・ランプ表示）を表示。
///   - カウント終了したら main() に伝えて完了。
/// </summary>
public class CountdownModule : MonoBehaviour
{
    public IEnumerator RunCountdownRoutine()
    {
        var game = FindFirstObjectByType<BatGame>();
        while (!game || !game.view || !game.view.eye) {
            yield return null;
            game = FindFirstObjectByType<BatGame>();
        }

        var panel = game.view.Panel(
            "Game countdown",
            game.W(new Vector3(0, 1.65f, 2.15f)),
            new Vector2(.65f, .5f),
            game.transform
        );

        var label = game.view.Label(panel, "3", Vector2.zero, new Vector2(.55f, .38f), .18f);

        for (int n = 3; n >= 1; n--) {
            label.text = n.ToString();
            game.view.Lamps(n);
            yield return new WaitForSeconds(1f);
        }

        label.text = "開始";
        yield return new WaitForSeconds(.4f);

        if (panel) Destroy(panel.gameObject);
    }
}
