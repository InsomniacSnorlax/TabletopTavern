using UnityEngine;
using UnityEngine.UI;

public class UILineDrawer : MonoBehaviour
{
    public RectTransform pointA;
    public RectTransform pointB;
    public RectTransform lineRect;
    public float lineWidth = 5f;
    public float scrollSpeed = 1f;

    [SerializeField] private Image lineImage;
    [SerializeField] private Image linePointer;
    public Image LinePointer => linePointer;

    [SerializeField] private float offset = 0;
    [SerializeField] private float bonusLength = 0.2f; // Extra length added to the line for better pointer visibility

    private Material lineMaterial;
    private bool isSetup;

    private void Start()
    {
        TurnOff();
    }

    public void SetUp(RectTransform _pointA)
    {
        pointA = _pointA;
        pointB = linePointer.GetComponent<RectTransform>();
        lineImage.material = new Material(lineImage.material);
        lineMaterial = lineImage.material;
        lineImage.enabled = true;
        linePointer.enabled = true;
        isSetup = true;
    }

    public void TurnOff()
    {
        linePointer.enabled = false;
        lineImage.enabled = false;
        isSetup = false;
    }

    void LateUpdate()
    {
        if (!isSetup || pointA == null || pointB == null || lineRect == null)
            return;

        Vector3 worldStart = pointA.position;
        Vector3 worldEnd = pointB.position;

        // Convert to lineRect's parent local space so sizeDelta units match exactly
        Vector3 localStart = lineRect.parent.InverseTransformPoint(worldStart);
        Vector3 localEnd = lineRect.parent.InverseTransformPoint(worldEnd);

        Vector3 localEndExtended = localEnd + (localEnd - localStart) * bonusLength;

        float distance = Vector2.Distance(localStart, localEndExtended);
        float angle = Mathf.Atan2(localEnd.y - localStart.y, localEnd.x - localStart.x) * Mathf.Rad2Deg;

        lineRect.localPosition = (localStart + localEndExtended) / 2f;
        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        linePointer.transform.SetPositionAndRotation(worldEnd, Quaternion.Euler(0, 0, angle + offset));

        AdjustTextureTiling(distance);
        ScrollTexture();
    }

    private void AdjustTextureTiling(float distance)
    {
        if (lineMaterial == null)
            return;

        Vector2 tiling = lineMaterial.mainTextureScale;
        tiling.x = distance / lineWidth;
        lineMaterial.mainTextureScale = tiling;
    }

    private void ScrollTexture()
    {
        if (lineMaterial == null)
            return;

        Vector2 texOffset = lineMaterial.mainTextureOffset;
        texOffset.x += Time.deltaTime * scrollSpeed;
        lineMaterial.mainTextureOffset = texOffset;
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
            Destroy(lineMaterial);
    }
}
