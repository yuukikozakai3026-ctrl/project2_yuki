using UnityEngine;

/// <summary>
/// ゲーム全体の共通設定パラメータを一元管理するクラス
/// architect_10-20_2.md 準拠
/// </summary>
public static class GameSettings
{
    // ボールの種類数（0 〜 BALL_TYPE_MAX_INDEX）
    public const int BALL_TYPE_MAX_INDEX = 7;

    // 初回投球までの時間（秒、小数OK）
    public const float FIRST_PITCH_MIN_TIME = 2.0f;
    public const float FIRST_PITCH_MAX_TIME = 5.0f;

    // 2球目以降の投球間隔（秒、整数指定）
    public const float NEXT_PITCH_MIN_TIME = 7.0f;
    public const float NEXT_PITCH_MAX_TIME = 10.0f;

    // ゲーム本編の制限時間（秒）
    public const float GAME_TIME_LIMIT = 60.0f;
}
