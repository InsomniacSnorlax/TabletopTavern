using UnityEngine;
using System.Collections.Generic;

namespace TJ
{
    [CreateAssetMenu(fileName = "GarrisonWalls", menuName = "GameData/GarrisonWalls", order = 2)]
    public class GarrisonWallsSO : ScriptableObject
    {
        [Header("Wall Prefabs")]
        [Tooltip("Modular wall segment. Must have a BoxCollider and NavMeshObstacle (Carve = true).")]
        public List<GameObject> wallSegmentPrefabs;
        [Tooltip("Passable gate/arch opening. No NavMeshObstacle — units walk through.")]
        public GameObject gatePrefab;
        [Tooltip("Optional tower placed at each end of the wall line.")]
        public GameObject towerPrefab;

        [Header("Wall Layout")]
        [Tooltip("Z world position of the wall line. Enemy third is roughly z=45.")]
        public float wallLineZ = 45f;
        [Tooltip("Width of each wall segment prefab in world units.")]
        public float wallSegmentWidth = 10f;
        [Tooltip("Number of passable gate openings to leave in the wall.")]
        public int numberOfGates = 2;
        [Tooltip("How far in +Z the middle section is recessed from the flanks. 0 = flat wall.")]
        public float wallInwardDepth = 0f;
        [Tooltip("Village layout: only the centre third of the wall spawns as the front face; perpendicular side walls run from each end all the way to the back of the battlefield.")]
        public bool useStraightSideWalls = false;
        [Tooltip("Convex layout: middle section sits at wallLineZ (forward); left and right flanks are recessed inward by wallInwardDepth. Requires wallInwardDepth > 0.")]
        public bool useConvexWalls = false;

        [Header("Gate")]
        [Tooltip("Starting (and max) health for each gate squad spawned from this wall config.")]
        public int gateStartingHealth = 100;
    }
}
