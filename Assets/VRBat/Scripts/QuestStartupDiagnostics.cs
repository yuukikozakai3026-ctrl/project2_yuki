using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRBat
{
    public static class QuestStartupDiagnostics
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeScene()
        {
            Debug.Log("[VR Startup] BeforeSceneLoad");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("[VR Startup] Scene loaded: " + scene.name);
        }
    }
}
