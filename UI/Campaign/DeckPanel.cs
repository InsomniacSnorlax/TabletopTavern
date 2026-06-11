// using TJ.Map;
// using UnityEngine;
// using Memori.Utilities;
// using System.Collections.Generic;
// using UnityEngine.UI;

// namespace TJ.Map
// {
// [RequireComponent(typeof(MemoriCanvasGroup))]
// public class DeckPanel : MonoBehaviour
// {
//     [SerializeField] private Transform cardParentTransform;
//     [SerializeField] private GearDeckCard gearDeckCardPrefab;
//     [SerializeField] private Button closeButton;
//     [SerializeField] private GearCard selectedGearCard;
//     [SerializeField] private Animator deckTitleAnimator;

//     MemoriCanvasGroup memoriCanvasGroup;
//     List<Gear> gearList = new();

//     CampaignSaveManager campaignSaveManager;
//     MapSceneUIManager mapSceneUIManager;

//     private void Awake()
//     {
//         memoriCanvasGroup = GetComponent<MemoriCanvasGroup>();
//         closeButton.onClick.AddListener(CloseDeckPanel);
//     }
//     public void SetUp(CampaignSaveManager _campaignSaveManager, MapSceneUIManager _mapSceneUIManager)
//     {
//         campaignSaveManager = _campaignSaveManager;
//         mapSceneUIManager = _mapSceneUIManager;
//     }
//     public void LoadDeckPanel()
//     {
//         deckTitleAnimator.SetBool("Active", true);
//         List<string> gearNameList = campaignSaveManager.GetGear();
//         gearList = new ();
//         foreach (string gearName in gearNameList) {
//             Gear gear = GearData.GetGear(gearName);
//             gearList.Add(gear);
//         }

//         for(int i = 0; i < cardParentTransform.childCount; i++) {
//             Destroy(cardParentTransform.GetChild(i).gameObject);
//         }

//         foreach (Gear gear in gearList) {
//             GearDeckCard gearDeckCard = Instantiate(gearDeckCardPrefab, cardParentTransform);
//             gearDeckCard.LoadGearCard(gear, this);
//         }
//         memoriCanvasGroup.CGEnable();

//         if(selectedGearCard.gameObject.activeSelf) selectedGearCard.gameObject.SetActive(false);
//     }
//     public void CloseDeckPanel()
//     {
//         deckTitleAnimator.SetBool("Active", false);
//         memoriCanvasGroup.FadeOut();
//     }
//     public void ShowGearCardInfo(Gear _gear)
//     {
//         if(!selectedGearCard.gameObject.activeSelf) selectedGearCard.gameObject.SetActive(true);

//         selectedGearCard.LoadGearCardDeck(_gear);
//     }
//     public void HideGearCardInfo()
//     {
//         selectedGearCard.gameObject.SetActive(false);
//     }
// }
// }