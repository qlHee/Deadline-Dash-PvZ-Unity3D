using UnityEngine;

public class UnlockResetOnExit : MonoBehaviour
{
    static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        ResetUnlocks();

        GameObject go = new GameObject("UnlockResetOnExit");
        DontDestroyOnLoad(go);
        go.AddComponent<UnlockResetOnExit>();
    }

    void OnApplicationQuit()
    {
        ResetUnlocks();
    }

    static void ResetUnlocks()
    {
        SharedUnlockIO.Save(new SharedUnlockData());
    }
}
