using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Memori.SaveData;
using MoreMountains.Feedbacks;
using Memori.UI;
using Memori.Audio;

namespace TJ
{
    public class SquadDisplayCardPresenter : MonoBehaviour
    {
        [SerializeField] protected TMP_Text unitCountText;
        public TMP_Text UnitCountText => unitCountText;
        [SerializeField] protected Image unitImage, unitTypeIcon;
        public Image UnitImage => unitImage;
        public Image UnitTypeIcon => unitTypeIcon;
        [SerializeField] protected MMF_Player onHoverEnterFeedback, onHoverExitFeedback;
        public MMF_Player OnHoverEnterFeedback => onHoverEnterFeedback;
        public MMF_Player OnHoverExitFeedback => onHoverExitFeedback;
        [SerializeField] protected Button selectUnitTypeButton;
        public Button SelectUnitTypeButton => selectUnitTypeButton;

        [SerializeField] protected Image hoveredImage, selectedImage;
        public Image HoveredImage => hoveredImage;
        public Image SelectedImage => selectedImage;
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;
        
        [Header("Healthbar")]
        [SerializeField] protected Slider healthSlider;
        public Slider HealthSlider => healthSlider;

        [Header("Tiers and Levels")]
        [SerializeField] protected Image tierGradient;
        [SerializeField] protected Image tierGradient2;
        [SerializeField] protected GameObject prestigeFrame1, prestigeFrame2, prestigeFrame3;

        [Header("Backgrounds")]
        [SerializeField] protected Image ironLegionBackground;
        [SerializeField] protected Image  greenTideBackground, ravenhostBackground, taelindorBackground, sanguineCourtBackground, sakuraDynastyBackground, deepstoneHoldBackground, drakosaurBroodBackground;
        public void SetBackgroundImage(Race race)
        {
            ironLegionBackground.enabled = false;
            greenTideBackground.enabled = false;
            ravenhostBackground.enabled = false;
            taelindorBackground.enabled = false;
            sanguineCourtBackground.enabled = false;
            sakuraDynastyBackground.enabled = false;
            deepstoneHoldBackground.enabled = false;
            drakosaurBroodBackground.enabled = false;

            switch(race)
            {
                case Race.IronLegion:
                    ironLegionBackground.enabled = true;
                    break;
                case Race.Gruntkin:
                    greenTideBackground.enabled = true;
                    break;
                case Race.RavenHost:
                    ravenhostBackground.enabled = true;
                    break;
                case Race.TaelindorForest:
                    taelindorBackground.enabled = true;
                    break;
                case Race.SanguineCourt:
                    sanguineCourtBackground.enabled = true;
                    break;
                case Race.SakuraDynasty:
                    sakuraDynastyBackground.enabled = true;
                    break;
                case Race.DeepstoneHold:
                    deepstoneHoldBackground.enabled = true;
                    break;
                case Race.DrakosaurBrood:
                    drakosaurBroodBackground.enabled = true;
                    break;
            }
        }
        public void SetPrestigeFrames(int prestige)
        {
            switch(prestige)
            {
                case -1:
                    Destroy(prestigeFrame1);
                    Destroy(prestigeFrame2);
                    Destroy(prestigeFrame3);
                    break;
                case 0:
                    prestigeFrame1.SetActive(true);
                    Destroy(prestigeFrame2);
                    Destroy(prestigeFrame3);
                    break;
                case 1:
                    prestigeFrame2.SetActive(true);
                    Destroy(prestigeFrame1);
                    Destroy(prestigeFrame3);
                    break;
                case 2:
                    prestigeFrame3.SetActive(true);
                    Destroy(prestigeFrame1);
                    Destroy(prestigeFrame2);
                    break;
            }
        }
        public void SetTierVisuals(UnitRarity unitRarity)
        {
            Color gradientColor = ColorData.GetRarityTierColor(unitRarity);

            tierGradient.color = new Color(gradientColor.r, gradientColor.g, gradientColor.b, 100f / 255f);
            tierGradient2.color = new Color(gradientColor.r, gradientColor.g, gradientColor.b, 100f / 255f);
        }
    }
}