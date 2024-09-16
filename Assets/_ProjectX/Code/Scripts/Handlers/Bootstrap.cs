using Unity.Entities;
using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        DefaultWorldInitialization.Initialize("My World", false);
#endif
    }
}