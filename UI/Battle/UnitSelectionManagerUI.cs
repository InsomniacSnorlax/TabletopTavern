using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionManagerUI : MonoBehaviour 
{
    [SerializeField] private RectTransform selectionAreaRectTransform;
    [SerializeField] private Color addToSelectionColor, removeFromSelectionColor;
    [SerializeField] private Image selectionAreaImage;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        BattleInputManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        BattleInputManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update() {
        if (selectionAreaRectTransform.gameObject.activeSelf) {
            UpdateVisual();
        }
    }

    private void UnitSelectionManager_OnSelectionAreaStart(object sender, System.EventArgs e) {
        selectionAreaRectTransform.gameObject.SetActive(true);
        // Debug.Log($"Selection area started");
        UpdateVisual();
    }

    private void UnitSelectionManager_OnSelectionAreaEnd(object sender, System.EventArgs e) {
        selectionAreaRectTransform.gameObject.SetActive(false);
        // Debug.Log($"Selection area ended");
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = BattleInputManager.Instance.GetSelectionAreaRect();

        float canvasScale = canvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
        selectionAreaImage.color = addToSelectionColor; //BattleInputManager.Instance.RemovingFromSelectedUnits ? removeFromSelectionColor : 

        // Get the screen bounds for the selection area to stay within screen limits
        float minX = 10;
        float maxX = Screen.width / canvasScale - 10;
        float minY = 10;
        float maxY = Screen.height / canvasScale - 10;

        // Clamp the selection area position to ensure it does not go off-screen
        Vector2 anchoredPosition = selectionAreaRectTransform.anchoredPosition;

        // Handle X-axis clamping (left and right boundaries)
        if (anchoredPosition.x < minX)
        {
            selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRectTransform.sizeDelta.x + (anchoredPosition.x - minX), selectionAreaRectTransform.sizeDelta.y);
            anchoredPosition.x = minX;
        }
        if (anchoredPosition.x + selectionAreaRectTransform.sizeDelta.x > maxX)
        {
            selectionAreaRectTransform.sizeDelta = new Vector2(maxX - anchoredPosition.x, selectionAreaRectTransform.sizeDelta.y);
        }

        // Handle Y-axis clamping (top and bottom boundaries)
        if (anchoredPosition.y < minY)
        {
            selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRectTransform.sizeDelta.x, selectionAreaRectTransform.sizeDelta.y + (anchoredPosition.y - minY));
            anchoredPosition.y = minY;
        }
        if (anchoredPosition.y + selectionAreaRectTransform.sizeDelta.y > maxY)
        {
            selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRectTransform.sizeDelta.x, maxY - anchoredPosition.y);
        }

        // Apply the clamped position and size back to the RectTransform
        selectionAreaRectTransform.anchoredPosition = anchoredPosition;
        selectionAreaRectTransform.sizeDelta = selectionAreaRectTransform.sizeDelta;
    }
    private void OnDestroy() 
    {
        if(BattleInputManager.Instance != null)
        {
            BattleInputManager.Instance.OnSelectionAreaStart -= UnitSelectionManager_OnSelectionAreaStart;
            BattleInputManager.Instance.OnSelectionAreaEnd -= UnitSelectionManager_OnSelectionAreaEnd;
        }
    }
}