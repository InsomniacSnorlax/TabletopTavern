using UnityEngine;
using TMPro;
using QuickOutline;

namespace TJ.Shop
{
    [RequireComponent(typeof(Canvas))]
    public class ShopPriceCanvas : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI priceText;
        private Outline outline;
        private Canvas canvas;
        GoldManager goldManager;
        private void Start()
        {
            canvas = GetComponent<Canvas>();
            canvas.worldCamera = CampaignManager.Instance.MapCamera.ShopCamera;
            goldManager = CampaignManager.Instance.GoldManager;
            goldManager.OnGoldAmountChanged += UpdateAffordability;
        }
        public void SetUp(string price)
        {
            outline = GetComponentInChildren<Outline>();
            priceText.text = price;
            bool canAfford = CampaignManager.Instance.GoldManager.CheckIfCanAfford(int.Parse(priceText.text));
            Color color = ColorData.GetColorBasedOnAffordability(canAfford);
            priceText.color = color;
            outline.OutlineColor = color;
        }
        void Update()
        {
            //face the camera
            Vector3 cameraPosition = canvas.worldCamera.transform.position;
            Vector3 canvasPosition = canvas.transform.position;
            Vector3 direction = cameraPosition - canvasPosition;
            direction.y = 0; // Keep the canvas upright
            direction.Normalize();
            canvas.transform.rotation = Quaternion.LookRotation(direction);
        }
        public void UpdateAffordability(int _goldAmount)
        {
            bool canAfford = CampaignManager.Instance.GoldManager.CheckIfCanAfford(int.Parse(priceText.text));
            Color color = ColorData.GetColorBasedOnAffordability(canAfford);
            priceText.color = color;
            outline.OutlineColor = color;
        }
        public void OnDestroy()
        {
            if(goldManager != null) {
                goldManager.OnGoldAmountChanged -= UpdateAffordability;
            }
        }
    }
}
