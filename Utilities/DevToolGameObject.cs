using Memori.SaveData;
using Memori.Utilities;
using UnityEngine;

[RequireComponent(typeof(MemoriCanvasGroup))]
public class DevToolGameObject : MonoBehaviour
{
    public bool lockUnlessDevToolUser = false;
    MemoriCanvasGroup memoriCanvasGroup;
    private void Start()
    {
        memoriCanvasGroup = GetComponent<MemoriCanvasGroup>();
        bool isDev = SaveDataHandler.IsDevToolUser();
        bool shouldEnable = lockUnlessDevToolUser ? !isDev : isDev;
        if (shouldEnable)
            memoriCanvasGroup.CGEnable();
        else
            memoriCanvasGroup.CGDisable();
    }
}
