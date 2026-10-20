/// <summary>
/// シーンを跨いでゲームデータを保持する静的クラス
/// architect_10-20_2.md 準拠:
///   - シーンを跨いだデータの受け渡しに使用する静的クラス
///   - FinalScore = -1: 初回起動時（スタート画面を表示）
///   - FinalScore >= 0: GameScene終了後（リザルト画面を表示）
///   - Reset(): FinalScore = -1 に初期化
/// </summary>
public static class GameData
{
    /// <summary>
    /// 最終獲得スコア（-1: 初回起動 / 0以上: ゲーム終了後のリザルトスコア）
    /// </summary>
    public static int FinalScore = -1;

    /// <summary>
    /// リザルト表示対象のスコアが存在するかどうか（互換用プロパティ）
    /// </summary>
    public static bool HasResult => FinalScore >= 0;

    /// <summary>
    /// スコア状態を初期状態（-1）にリセットする
    /// </summary>
    public static void Reset()
    {
        FinalScore = -1;
    }
}
