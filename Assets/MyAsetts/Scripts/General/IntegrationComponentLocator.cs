using UnityEngine;

/// <summary>
/// XR Interaction Toolkitのバージョン差異に対応するためのコンポーネント取得補助クラス
/// </summary>
public static class IntegrationComponentLocator<T> where T : Component
{
    public static bool TryFindComponent(out T component)
    {
        component = Object.FindFirstObjectByType<T>();
        return component != null;
    }
}
