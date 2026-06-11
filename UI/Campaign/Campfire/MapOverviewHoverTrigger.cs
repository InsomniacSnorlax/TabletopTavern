using TJ.Map;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.Campfire
{
    public class MapOverviewHoverTrigger : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private MapOverviewPanel mapOverviewPanel;
        [SerializeField] private MapSceneUIManager mapSceneUIManager;

        public void OnPointerEnter(PointerEventData eventData)
        {
            mapOverviewPanel.Open(
                mapSceneUIManager.MapSceneManager.MapLayers,
                CampaignManager.Instance.CampaignSaveManager.SaveData,
                mapSceneUIManager.LayerNodeSelected);
        }
    }
}
