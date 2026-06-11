using UnityEngine;
using Memori.UI;
using TMPro;

public class SteamLinkButton : MonoBehaviour
{
    [SerializeField] private MemoriButtonV2 button;
    [SerializeField] private TMP_Text wishlistText, wishlistCountText, steamText;
    [SerializeField] private Link links;

    private void Start()
    {
        #if DEMO
            wishlistText.gameObject.SetActive(true);
            wishlistCountText.gameObject.SetActive(true);
            steamText.gameObject.SetActive(false);
        #else
            wishlistText.gameObject.SetActive(false);
            wishlistCountText.gameObject.SetActive(false);
            steamText.gameObject.SetActive(true);
        #endif

        button.Button.onClick.AddListener(OpenLink);
    }

    private void OpenLink()
    {
        Application.OpenURL(links.Url);
    }
}
