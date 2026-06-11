using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Memori.Audio;
using Memori.UI;

namespace TJ.MainMenu
{
    public class PlayPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected Image highlightImage, iconToScale, selectionHighlightImage;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private GameObject particleEffect;
        [SerializeField] private Button button;
        public Button Button => button;
        [SerializeField] private bool scaleOnHover = true;
        PlayPanel _playPanel;
        GameObject _sectionPanel;
        private void Awake()
        {
            if (highlightImage != null) highlightImage.enabled = false;
            if (particleEffect != null) particleEffect.SetActive(false);
        }
        public void SetUp(PlayPanel playPanel, GameObject sectionPanel)
        {
            _playPanel = playPanel;
            _sectionPanel = sectionPanel;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_sectionPanel != null) _sectionPanel.SetActive(true);
            if (scaleOnHover) MemoriUI.BloomItemScale(transform, 1.025f, 0.1f);
            if (buttonText != null) MemoriUI.BloomItemScaleTemp(buttonText.transform, 1.1f, 0.1f);
            if (iconToScale != null) MemoriUI.BloomItemScaleTemp(iconToScale.transform, 1.1f, 0.1f);
            if (highlightImage != null) highlightImage.enabled = true;
            if (particleEffect != null) particleEffect.SetActive(true);
            if (_sectionPanel != null) _playPanel.OnPlayPanelButtonHover(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_sectionPanel == null) ShutDown();
        }
        public void ShutDown()
        {
            if (highlightImage != null) highlightImage.enabled = false;
            if (particleEffect != null) particleEffect.SetActive(false);
            if (scaleOnHover) MemoriUI.BloomItemScale(transform, 1f, 0.1f);
            if (_sectionPanel != null) _sectionPanel.SetActive(false);
        }
}
}
