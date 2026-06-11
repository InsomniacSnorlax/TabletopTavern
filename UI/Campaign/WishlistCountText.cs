using UnityEngine;
using TMPro;

namespace TJ
{
    [RequireComponent(typeof(TMP_Text))]
    public class WishlistCountText : MonoBehaviour
    {
        private void Start()
        {
            UpdateWishlistCount();
        }

        private void UpdateWishlistCount()
        {
            GetComponent<TMP_Text>().text = TabletopTavernConstants.WISHLIST_COUNT;
        }
    }
}