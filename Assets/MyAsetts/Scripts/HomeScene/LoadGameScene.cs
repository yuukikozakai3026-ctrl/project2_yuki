using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// コライダー進入時に指定したシーンへ遷移するモジュール
/// </summary>
public class SceneTeleporter : MonoBehaviour
{
    // 移動先のシーン名（インスペクターで指定可能）
    [SerializeField] private string targetSceneName = "GameScene";

    // プレイヤーのタグ名（デフォルトは "Player"）
    [SerializeField] private string playerTag = "Player";

    // コライダーの範囲に入った瞬間に呼び出される
    private void OnTriggerEnter(Collider other)
    {
        // 侵入してきたオブジェクトがプレイヤーかどうかを判定
        if (other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag)))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
