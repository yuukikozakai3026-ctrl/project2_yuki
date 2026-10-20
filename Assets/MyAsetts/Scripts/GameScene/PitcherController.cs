using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// ピッチャー管理モジュール
/// architect_10-20_2.md 準拠:
///   - 各ピッチャーオブジェクトにアタッチされ、腕の振り（アニメーション）とリリース制御を担当する。
///   - 投球モーション開始 (BeginThrow):
///     腕のボーンを回転させてワインドアップから振り下ろしアニメーションを実行。
///     手が最前点（リリース点）に達した瞬間、手のワールド座標 (releasePoint) を算出してコールバック通知。
///   - 停止処理 (StopPitching):
///     投球コルーチンおよび腕振りモーションを即座に停止し、基本姿勢へ戻る。
/// </summary>
public class PitcherController : MonoBehaviour
{
    [SerializeField] string pitcherName = "Pitcher";
    [SerializeField, Range(-1, 1)] int lane;

    Coroutine throwMotion;
    Transform throwingArm;
    Transform throwingForearm;
    Quaternion neutralThrowRotation;

    public int Lane => lane;
    public string PitcherName => pitcherName;

    void Start()
    {
        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = false;

        PoseArm("B-upperArm.L", "B-forearm.L", new Vector3(.22f, -.85f, -.26f));
        throwingArm = PoseArm("B-upperArm.R", "B-forearm.R", new Vector3(-.22f, -.78f, -.35f));
        throwingForearm = GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(x => x.name == "B-forearm.R");

        if (throwingArm) neutralThrowRotation = throwingArm.rotation;
    }

    Transform PoseArm(string upperName, string forearmName, Vector3 desired)
    {
        var bones = GetComponentsInChildren<Transform>(true);
        var upper = bones.FirstOrDefault(x => x.name == upperName);
        var forearm = bones.FirstOrDefault(x => x.name == forearmName);
        if (!upper || !forearm) return null;

        var current = forearm.position - upper.position;
        if (current.sqrMagnitude > .0001f)
            upper.rotation = Quaternion.FromToRotation(current.normalized, desired.normalized) * upper.rotation;

        return upper;
    }

    public void Configure(string name, int selectedLane)
    {
        pitcherName = name;
        lane = Mathf.Clamp(selectedLane, -1, 1);
    }

    /// <summary>
    /// 腕振り投球モーションを実行する
    /// </summary>
    /// <param name="release">リリース位置到達時のコールバック（ワールド座標を渡す）</param>
    /// <param name="playbackRate">スローモーション等の再生速度レート取得デリゲート</param>
    public void BeginThrow(Action<Vector3> release, Func<float> playbackRate = null)
    {
        if (throwMotion != null) StopCoroutine(throwMotion);
        if (throwingArm) throwingArm.rotation = neutralThrowRotation;
        throwMotion = StartCoroutine(AnimateThrow(release, playbackRate));
    }

    IEnumerator AnimateThrow(Action<Vector3> release, Func<float> playbackRate)
    {
        if (!throwingArm || !throwingForearm) {
            release?.Invoke(transform.position + new Vector3(0, 1.45f, -.7f));
            throwMotion = null;
            yield break;
        }

        var direction = (throwingForearm.position - throwingArm.position).normalized;
        var peak = Quaternion.FromToRotation(direction, new Vector3(-.15f, .56f, -.81f).normalized)
                   * neutralThrowRotation;

        float elapsed = 0;
        bool released = false;

        while (elapsed < .40f) {
            float rate = playbackRate?.Invoke() ?? 1;
            if (rate <= 0) { yield return null; continue; }
            elapsed += Time.unscaledDeltaTime * rate;

            float arc = elapsed <= .275f
                ? Mathf.Sin(Mathf.Clamp01(elapsed / .275f) * Mathf.PI * .5f)
                : Mathf.Cos(Mathf.Clamp01((elapsed - .275f) / .125f) * Mathf.PI * .5f);

            throwingArm.rotation = Quaternion.Slerp(neutralThrowRotation, peak, arc);

            if (!released && elapsed >= .275f) {
                released = true;
                Vector3 arm = throwingForearm.position - throwingArm.position;
                Vector3 hand = throwingForearm.position + arm.normalized * .23f;
                hand.y = Mathf.Clamp(hand.y, transform.position.y + 1.28f, transform.position.y + 1.85f);
                hand.z = Mathf.Min(hand.z, transform.position.z - .4f);
                release?.Invoke(hand);
            }
            yield return null;
        }

        throwingArm.rotation = neutralThrowRotation;
        throwMotion = null;
    }

    /// <summary>
    /// 想定リリース点のワールド座標を算出
    /// </summary>
    public Vector3 EstimateReleasePoint()
    {
        if (!throwingArm || !throwingForearm) return transform.position + new Vector3(0, 1.45f, -.7f);
        var arm = throwingForearm.position - throwingArm.position;
        var direction = Quaternion.FromToRotation(arm.normalized, new Vector3(-.15f, .56f, -.81f).normalized) * arm;
        var hand = throwingArm.position + direction + direction.normalized * .23f;
        hand.y = Mathf.Clamp(hand.y, transform.position.y + 1.28f, transform.position.y + 1.85f);
        hand.z = Mathf.Min(hand.z, transform.position.z - .4f);
        return hand;
    }

    /// <summary>
    /// ピッチャーのリリース位置を目標座標に合わせる
    /// </summary>
    public void AlignReleasePoint(Vector3 desired)
    {
        transform.position += desired - EstimateReleasePoint();
    }

    /// <summary>
    /// 投球動作を即座に停止し基本姿勢へ戻す
    /// </summary>
    public void StopPitching()
    {
        if (throwMotion != null) StopCoroutine(throwMotion);
        throwMotion = null;
        if (throwingArm) throwingArm.rotation = neutralThrowRotation;
    }
}
