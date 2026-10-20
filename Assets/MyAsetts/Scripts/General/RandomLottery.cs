using UnityEngine;

/// <summary>
/// 汎用乱数抽選を行う静的モジュール
/// architect_10-20_2.md 準拠:
///   - 最小値・最大値・小数許可フラグを受け取り乱数を生成
///   - 返り値は float 固定（整数抽選の場合は端数を丸めてキャスト利用）
/// </summary>
public static class RandomLottery
{
    /// <summary>
    /// 指定範囲内で数値を抽選する（返り値はfloat固定）
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <param name="isDecimal">true: 小数OK / false: 整数のみ</param>
    public static float GetRandomValue(float min, float max, bool isDecimal)
    {
        if (isDecimal)
        {
            // 小数を含むランダム（maxを含む）
            return Random.Range(min, max);
        }
        else
        {
            // 整数のみのランダム
            // Random.Range(int, int) は最大値が含まれない仕様のため +1 する
            int minInt = Mathf.FloorToInt(min);
            int maxInt = Mathf.FloorToInt(max);
            return (float)Random.Range(minInt, maxInt + 1);
        }
    }
}
