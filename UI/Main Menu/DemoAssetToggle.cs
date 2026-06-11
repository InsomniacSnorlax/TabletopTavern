using UnityEngine;

public class DemoAssetToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] demoAssets;
    [SerializeField] private GameObject[] fullGameAssets;

    private void Awake()
    {
#if DEMO
        SetActive(demoAssets, true);
        SetActive(fullGameAssets, false);
#else
        SetActive(demoAssets, false);
        SetActive(fullGameAssets, true);
#endif
    }

    private void SetActive(GameObject[] objects, bool active)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
