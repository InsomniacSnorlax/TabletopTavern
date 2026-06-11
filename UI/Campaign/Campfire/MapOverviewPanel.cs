using System.Collections.Generic;
using Memori.SaveData;
using Memori.Utilities;
using TJ.Map;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Campfire
{
    [System.Serializable]
    public struct NodeTypeSpriteEntry
    {
        public NodeType type;
        public Sprite sprite;
    }
    [RequireComponent(typeof(MemoriCanvasGroup))]
    public class MapOverviewPanel : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform nodeContainer;
        [SerializeField] private float padding = 30f;

        [Header("Prefabs")]
        [SerializeField] private MapOverviewNodeUI nodePrefab;
        [SerializeField] private Image connectionLinePrefab;

        [Header("Node State Colors")]
        [SerializeField] private Color visitedColor = new Color(0.35f, 0.35f, 0.35f);
        [SerializeField] private Color currentColor = Color.yellow;
        [SerializeField] private Color hiddenColor  = new Color(0.25f, 0.25f, 0.25f);

        [Header("Connection Colors")]
        [SerializeField] private Color connectionTraveled = new Color(0.9f, 0.8f, 0.2f);
        [SerializeField] private Color connectionPassed   = new Color(0.55f, 0.55f, 0.55f, 0.5f);
        [SerializeField] private Color connectionDefault  = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        [SerializeField] private float lineThickness = 3f;

        [Header("Node Type Sprites")]
        [SerializeField] private NodeTypeSpriteEntry[] typeSprites;
        [SerializeField] private Sprite surpriseSprite;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private MemoriCanvasGroup canvasGroup;
        private readonly List<GameObject> spawned = new();

        private List<MapLayer> lastLayers;
        private CampaignSaveData lastSaveData;
        private int lastCurrentNodeIndex;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<MemoriCanvasGroup>();
            canvasGroup.CGDisable();
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Refresh()
        {
            if (IsOpen) Open(lastLayers, lastSaveData, lastCurrentNodeIndex);
        }

        public void Open(List<MapLayer> layers, CampaignSaveData saveData, int currentNodeIndex)
        {
            lastLayers           = layers;
            lastSaveData         = saveData;
            lastCurrentNodeIndex = currentNodeIndex;
            IsOpen               = true;
            ClearSpawned();

            if (layers == null || layers.Count == 0) return;

            // Compute world-space bounds across all nodes
            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);
            foreach (var layer in layers)
                foreach (var node in layer.LayerNodes)
                {
                    min = Vector2.Min(min, node.position);
                    max = Vector2.Max(max, node.position);
                }

            Rect r        = nodeContainer.rect;
            float usableW = r.width  - padding * 2f;
            float usableH = r.height - padding * 2f;
            float ox      = r.xMin + padding;
            float oy      = r.yMin + padding;
            float rangeX  = max.x - min.x;
            float rangeY  = max.y - min.y;

            Vector2 WorldToUI(Vector2 p)
            {
                float nx = rangeX > 0.001f ? (p.x - min.x) / rangeX : 0.5f;
                float ny = rangeY > 0.001f ? (p.y - min.y) / rangeY : 0.5f;
                return new Vector2(ox + (1f - ny) * usableW, oy + nx * usableH);
            }

            List<int> nodePath = saveData.nodePath;

            // Build node index → UI position lookup
            var uiPos = new Dictionary<int, Vector2>();
            foreach (var layer in layers)
                foreach (var node in layer.LayerNodes)
                    uiPos[node.index] = WorldToUI(node.position);

            // Find which layer the current node sits on
            int currentLayer = -1;
            foreach (var layer in layers)
            {
                foreach (var node in layer.LayerNodes)
                    if (node.index == currentNodeIndex) { currentLayer = node.layer; break; }
                if (currentLayer != -1) break;
            }

            // Spawn connection lines first so they render behind nodes
            for (int i = 0; i < layers.Count - 1; i++)
            {
                foreach (var node in layers[i].LayerNodes)
                {
                    if (!uiPos.TryGetValue(node.index, out Vector2 from)) continue;
                    foreach (int connectedIndex in node.connectedNodeIndexes)
                    {
                        if (!uiPos.TryGetValue(connectedIndex, out Vector2 to)) continue;
                        bool traveled = nodePath.Contains(node.index) &&
                                       (nodePath.Contains(connectedIndex) || connectedIndex == currentNodeIndex);
                        bool passed = !traveled && i < currentLayer;
                        Color lineColor = traveled ? connectionTraveled : passed ? connectionPassed : connectionDefault;
                        SpawnLine(from, to, lineColor);
                    }
                }
            }

            // Spawn node icons on top
            foreach (var layer in layers)
            {
                foreach (var node in layer.LayerNodes)
                {
                    if (!uiPos.TryGetValue(node.index, out Vector2 pos)) continue;

                    bool isCurrent  = node.index == currentNodeIndex;
                    bool isVisited  = !isCurrent && node.layer <= currentLayer;
                    bool isSurprise = node.mapNodeGameObject != null && node.mapNodeGameObject.Surprise;
                    bool wasHidden  = node.mapNodeGameObject != null && node.mapNodeGameObject.WasHidden;

                    Sprite sprite = isSurprise ? surpriseSprite : GetSprite(node.type);

                    MapOverviewNodeUI ui = Instantiate(nodePrefab, nodeContainer);
                    ui.GetComponent<RectTransform>().anchoredPosition = pos;
                    ui.Setup(node.type, isVisited, isCurrent, isSurprise, wasHidden,
                        visitedColor, currentColor, hiddenColor, sprite);
                    spawned.Add(ui.gameObject);
                }
            }

            canvasGroup.CGEnable();
            canvasGroup.FadeInAsync();
        }

        public void Close()
        {
            IsOpen = false;
            canvasGroup.FadeOutAsync();
        }

        private Sprite GetSprite(NodeType type)
        {
            if (typeSprites != null)
                foreach (var entry in typeSprites)
                    if (entry.type == type) return entry.sprite;
            return null;
        }

        private void SpawnLine(Vector2 from, Vector2 to, Color color)
        {
            Image line = Instantiate(connectionLinePrefab, nodeContainer);
            line.color = color;

            Vector2 diff  = to - from;
            float   dist  = diff.magnitude;
            float   angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            RectTransform rt = line.rectTransform;
            rt.anchoredPosition = (from + to) * 0.5f;
            rt.sizeDelta        = new Vector2(dist, lineThickness);
            rt.localEulerAngles = new Vector3(0f, 0f, angle);
            spawned.Add(line.gameObject);
        }

        private void ClearSpawned()
        {
            foreach (var obj in spawned)
                if (obj != null) Destroy(obj);
            spawned.Clear();
        }
    }
}
