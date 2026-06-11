using UnityEngine;
using System.Collections.Generic;

namespace TJ.Map
{
    [System.Serializable]
    public struct Path { public Vector3 start; public Vector3 end; }

    public class TerrainTexturePainter : MonoBehaviour
    {
        [SerializeField] private Terrain terrain; // Reference to the Terrain
        [SerializeField] private GameObject[] gameObjects; // Game objects to paint under
        private int resetTextureLayerIndex = 1;
        private int nodePointLayerIndex = 26;
        private int pathPointLayerIndex = 9;
        [SerializeField] private float nodePaintRadius = 0.075f; // Radius of the painted area (in world units)
        [SerializeField] private float pathPaintRadius = 0.075f; // Radius of the painted area (in world units)
        [SerializeField] private float nodeFalloff = 0.05f; // Falloff distance for smooth blending (in world units)
        [SerializeField] private float pathFalloff = 0.2f; // Falloff distance for smooth blending (in world units)
        [SerializeField] private int grassDetailLayer = 0;
        List<Path> paths = new(); // List of paths to paint under
        private MapRegion _mapRegion;
        public void ClearTerrain()
        {
            // Clear the terrain textures and details
            TerrainData terrainData = terrain.terrainData;
            int alphamapWidth = terrainData.alphamapWidth;
            int alphamapHeight = terrainData.alphamapHeight;

            // Reset the splatmap
            float[,,] alphamaps = new float[alphamapHeight, alphamapWidth, terrainData.alphamapLayers];
            for (int y = 0; y < alphamapHeight; y++)
            {
                for (int x = 0; x < alphamapWidth; x++)
                {
                    for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                    {
                        alphamaps[y, x, layer] = 0f;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, alphamaps);

            // Reset the detail maps
            int detailWidth = terrainData.detailWidth;
            int detailHeight = terrainData.detailHeight;
            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = new int[detailHeight, detailWidth];
                terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }
        }
        public void PaintTextures(GameObject[] _gameObjects, List<Path> _paths, MapRegion mapRegion)
        {
            _mapRegion = mapRegion;
            gameObjects = _gameObjects;
            paths = _paths;
            resetTextureLayerIndex = _mapRegion.resetTextureLayerIndex;
            grassDetailLayer = _mapRegion.grassDetailLayer; // Add this line; assumes MapTheme has a public int grassDetailLayer field (0 for green grass, 1 for brown grass)
            nodePointLayerIndex = _mapRegion.nodePointLayerIndex;
            pathPointLayerIndex = _mapRegion.pathPointLayerIndex;

            // Get TerrainData
            TerrainData terrainData = terrain.terrainData;
            int alphamapWidth = terrainData.alphamapWidth;
            int alphamapHeight = terrainData.alphamapHeight;
            float[,,] alphamaps = new float[alphamapHeight, alphamapWidth, terrainData.alphamapLayers];

            // Get detail map data
            int detailWidth = terrainData.detailWidth;
            int detailHeight = terrainData.detailHeight;
            int[][,] detailLayers = new int[terrainData.detailPrototypes.Length][,];
            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                detailLayers[i] = terrainData.GetDetailLayer(0, 0, detailWidth, detailHeight, i);
            }

            // Step 1: Reset the detail map to grassDetailLayer (e.g., grass)
            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                detailLayers[i] = new int[detailHeight, detailWidth];
                if (i == grassDetailLayer)
                {
                    // Set uniform density for the chosen detail prototype
                    for (int y = 0; y < detailHeight; y++)
                    {
                        for (int x = 0; x < detailWidth; x++)
                        {
                            detailLayers[i][y, x] = 8; // Max density (adjust as needed)
                        }
                    }
                }
                // Other detail layers are initialized to 0
            }

            // Step 1: Reset the splatmap to resetTextureLayerIndex (e.g., texture 1)
            for (int y = 0; y < alphamapHeight; y++)
            {
                for (int x = 0; x < alphamapWidth; x++)
                {
                    for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                    {
                        alphamaps[y, x, layer] = (layer == resetTextureLayerIndex) ? 1f : 0f;
                    }
                }
            }

            // Step 2: Paint textures under game objects
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrainData.size; // Width, height, length of the terrain

            // Step 3: Paint textures at 5 points along each path
            foreach (Path path in paths)
            {
                // Calculate 5 evenly spaced points along the path (including start and end)
                for (int i = 0; i <= 6; i++) // 0, 1, 2, 3, 4 (5 points)
                {
                    float t = i / 6f; // Interpolation parameter (0 to 1)
                    Vector3 point = Vector3.Lerp(path.start, path.end, t);

                    // Convert world position to terrain coordinates (0 to 1)
                    float u = (point.x - terrainPos.x) / terrainSize.x; // Normalized X
                    float v = (point.z - terrainPos.z) / terrainSize.z; // Normalized Z

                    //randomize the point a bit
                    u += Random.Range(-0.005f, 0.005f);
                    v += Random.Range(-0.005f, 0.005f);

                    // Convert to alphamap indices
                    int centerX = Mathf.FloorToInt(u * alphamapWidth);
                    int centerY = Mathf.FloorToInt(v * alphamapHeight);

                    // Convert to detail map indices
                    int detailCenterX = Mathf.FloorToInt(u * detailWidth);
                    int detailCenterY = Mathf.FloorToInt(v * detailHeight);

                    // Calculate radius in alphamap units for X and Z separately
                    float radiusInAlphamapX = pathPaintRadius * alphamapWidth / terrainSize.x; // X-axis scaling
                    float radiusInAlphamapZ = pathPaintRadius * alphamapHeight / terrainSize.z; // Z-axis scaling
                    float falloffInAlphamapX = pathFalloff * alphamapWidth / terrainSize.x; // X-axis falloff
                    float falloffInAlphamapZ = pathFalloff * alphamapHeight / terrainSize.z; // Z-axis falloff
                    float radiusInDetailX = pathPaintRadius * detailWidth / terrainSize.x; // Detail X-axis scaling
                    float radiusInDetailZ = pathPaintRadius * detailHeight / terrainSize.z; // Detail Z-axis scaling
                    float falloffInDetailX = pathFalloff * detailWidth / terrainSize.x; // Detail X-axis falloff


                    // Paint in a square area around the center
                    int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radiusInAlphamapX));
                    int maxX = Mathf.Min(alphamapWidth - 1, Mathf.CeilToInt(centerX + radiusInAlphamapX));
                    int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radiusInAlphamapZ));
                    int maxY = Mathf.Min(alphamapHeight - 1, Mathf.CeilToInt(centerY + radiusInAlphamapZ));

                    // Detail map bounds
                    int detailMinX = Mathf.Max(0, Mathf.FloorToInt(detailCenterX - radiusInDetailX));
                    int detailMaxX = Mathf.Min(detailWidth - 1, Mathf.CeilToInt(detailCenterX + radiusInDetailX));
                    int detailMinY = Mathf.Max(0, Mathf.FloorToInt(detailCenterY - radiusInDetailZ));
                    int detailMaxY = Mathf.Min(detailHeight - 1, Mathf.CeilToInt(detailCenterY + radiusInDetailZ));


                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int x = minX; x <= maxX; x++)
                        {
                            // Calculate distance from center in alphamap units, normalized for non-square terrain
                            float dx = (x - centerX) / radiusInAlphamapX; // Normalize by X radius
                            float dy = (y - centerY) / radiusInAlphamapZ; // Normalize by Z radius
                            float distance = Mathf.Sqrt(dx * dx + dy * dy); // Distance in normalized space

                            // Skip if outside radius (distance > 1 in normalized space)
                            if (distance > 1f) continue;

                            // Calculate paint strength (1 at center, 0 at radius + falloff)
                            float falloffDistance = 1 - (distance - 1f + falloffInAlphamapX / radiusInAlphamapX) / (falloffInAlphamapX / radiusInAlphamapX);
                            float strength = Mathf.Clamp01(falloffDistance);
                            if (strength <= 0) continue;

                            // Update alphamap: Increase weight of target texture, reduce others
                            for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                            {
                                float currentWeight = alphamaps[y, x, layer];
                                if (layer == pathPointLayerIndex)
                                {
                                    // Increase target texture weight
                                    alphamaps[y, x, layer] = Mathf.Lerp(currentWeight, 1, strength);
                                    // Debug.Log($"2 Painted texture {layer} at ({x}, {y}) with strength {strength}");
                                }
                                else
                                {
                                    // Reduce other texture weights
                                    alphamaps[y, x, layer] = Mathf.Lerp(currentWeight, 0, strength);
                                }
                            }
                        }
                    }

                    // Remove details in the same area
                    for (int y = detailMinY; y <= detailMaxY; y++)
                    {
                        for (int x = detailMinX; x <= detailMaxX; x++)
                        {
                            // Calculate distance from center in detail map units, normalized
                            float dx = (x - detailCenterX) / radiusInDetailX;
                            float dy = (y - detailCenterY) / radiusInDetailZ;
                            float distance = Mathf.Sqrt(dx * dx + dy * dy);

                            // Skip if outside radius
                            if (distance > 1f) continue;

                            // Calculate strength (same as for alphamap)
                            float falloffDistance = 1 - (distance - 1f + falloffInDetailX / radiusInDetailX) / (falloffInDetailX / radiusInDetailX);
                            float strength = Mathf.Clamp01(falloffDistance);
                            if (strength <= 0) continue;

                            // Set detail density to 0 for all detail layers
                            for (int layer = 0; layer < terrainData.detailPrototypes.Length; layer++)
                            {
                                detailLayers[layer][y, x] = Mathf.FloorToInt(Mathf.Lerp(detailLayers[layer][y, x], 0, strength));
                            }
                        }
                    }
                }
            }

            foreach (GameObject obj in gameObjects)
            {
                // Convert world position to terrain coordinates (0 to 1)
                Vector3 objPos = obj.transform.position;
                float u = (objPos.x - terrainPos.x) / terrainSize.x; // Normalized X
                float v = (objPos.z - terrainPos.z) / terrainSize.z; // Normalized Z

                // Convert to alphamap indices
                int centerX = Mathf.FloorToInt(u * alphamapWidth);
                int centerY = Mathf.FloorToInt(v * alphamapHeight);

                // Convert to detail map indices
                int detailCenterX = Mathf.FloorToInt(u * detailWidth);
                int detailCenterY = Mathf.FloorToInt(v * detailHeight);

                // Calculate radius in alphamap units for X and Z separately
                float radiusInAlphamapX = nodePaintRadius * alphamapWidth / terrainSize.x; // X-axis scaling
                float radiusInAlphamapZ = nodePaintRadius * alphamapHeight / terrainSize.z; // Z-axis scaling
                float falloffInAlphamapX = nodeFalloff * alphamapWidth / terrainSize.x; // X-axis falloff
                float falloffInAlphamapZ = nodeFalloff * alphamapHeight / terrainSize.z; // Z-axis falloff
                float radiusInDetailX = nodePaintRadius * detailWidth / terrainSize.x; // Detail X-axis scaling
                float radiusInDetailZ = nodePaintRadius * detailHeight / terrainSize.z; // Detail Z-axis scaling
                float falloffInDetailX = nodeFalloff * detailWidth / terrainSize.x; // Detail X-axis falloff


                // Paint in a square area around the center
                int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radiusInAlphamapX));
                int maxX = Mathf.Min(alphamapWidth - 1, Mathf.CeilToInt(centerX + radiusInAlphamapX));
                int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radiusInAlphamapZ));
                int maxY = Mathf.Min(alphamapHeight - 1, Mathf.CeilToInt(centerY + radiusInAlphamapZ));

                // Detail map bounds
                int detailMinX = Mathf.Max(0, Mathf.FloorToInt(detailCenterX - radiusInDetailX));
                int detailMaxX = Mathf.Min(detailWidth - 1, Mathf.CeilToInt(detailCenterX + radiusInDetailX));
                int detailMinY = Mathf.Max(0, Mathf.FloorToInt(detailCenterY - radiusInDetailZ));
                int detailMaxY = Mathf.Min(detailHeight - 1, Mathf.CeilToInt(detailCenterY + radiusInDetailZ));


                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        // Calculate distance from center in alphamap units, normalized for non-square terrain
                        float dx = (x - centerX) / radiusInAlphamapX; // Normalize by X radius
                        float dy = (y - centerY) / radiusInAlphamapZ; // Normalize by Z radius
                        float distance = Mathf.Sqrt(dx * dx + dy * dy); // Distance in normalized space

                        // Skip if outside radius (distance > 1 in normalized space)
                        if (distance > 1f) continue;

                        // Calculate paint strength (1 at center, 0 at radius + falloff)
                        float falloffDistance = 1 - (distance - 1f + falloffInAlphamapX / radiusInAlphamapX) / (falloffInAlphamapX / radiusInAlphamapX);
                        float strength = Mathf.Clamp01(falloffDistance);
                        if (strength <= 0) continue;

                        // Update Alphamap: Increase weight of target texture, reduce others
                        for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                        {
                            float currentWeight = alphamaps[y, x, layer];
                            if (layer == nodePointLayerIndex)
                            {
                                // Increase target texture weight
                                alphamaps[y, x, layer] = Mathf.Lerp(currentWeight, 1, strength);
                                // Debug.Log($"Painted texture {layer} at ({x}, {y}) with strength {strength}");
                            }
                            else
                            {
                                // Reduce other texture weights
                                alphamaps[y, x, layer] = Mathf.Lerp(currentWeight, 0, strength);
                            }
                        }
                    }
                }

                // Remove details in the same area
                for (int y = detailMinY; y <= detailMaxY; y++)
                {
                    for (int x = detailMinX; x <= detailMaxX; x++)
                    {
                        // Calculate distance from center in detail map units, normalized
                        float dx = (x - detailCenterX) / radiusInDetailX;
                        float dy = (y - detailCenterY) / radiusInDetailZ;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);

                        // Skip if outside radius
                        if (distance > 1f) continue;

                        // Calculate strength (same as for alphamap)
                        float falloffDistance = 1 - (distance - 1f + falloffInDetailX / radiusInDetailX) / (falloffInDetailX / radiusInDetailX);
                        float strength = Mathf.Clamp01(falloffDistance);
                        if (strength <= 0) continue;

                        // Set detail density to 0 for all detail layers
                        for (int layer = 0; layer < terrainData.detailPrototypes.Length; layer++)
                        {
                            detailLayers[layer][y, x] = Mathf.FloorToInt(Mathf.Lerp(detailLayers[layer][y, x], 0, strength));
                        }
                    }
                }
            }

            // Apply the modified alphamap
            terrainData.SetAlphamaps(0, 0, alphamaps);

            // Apply the modified detail layers
            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                terrainData.SetDetailLayer(0, 0, i, detailLayers[i]);
            }
            // Debug.Log($"Painted textures under {gameObjects.Length} game objects and along {paths.Count} paths.");
            paths.Clear();
        }
    }
}