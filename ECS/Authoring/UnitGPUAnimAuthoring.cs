using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Stores weak references (GUIDs) to the three GPU anim prefab variants for a unit type,
/// plus an optional rider prefab for cavalry units.
/// The Baker does NOT call GetEntity() — no baking of the prefabs happens at subscene load.
/// Prefabs are loaded on demand by UnitGPUAnimLoader using RequestEntityPrefabLoaded.
///
/// Setup:
///   1. Add this MonoBehaviour to a GameObject in the subscene (one per unit type).
///   2. Assign unitName and the three GPU anim prefab variants.
///   3. For cavalry, also assign riderGPUAnim. Leave it empty for infantry/non-cavalry.
///   4. Mark each prefab as Addressable in the Addressables window.
///   5. Before battle, call UnitGPUAnimLoader.PreloadUnitsAsync() with the needed unit names.
/// </summary>
public class UnitGPUAnimAuthoring : MonoBehaviour
{
    public UnitName unitName;
    public GameObject gpuAnim1, gpuAnim2, gpuAnim3;
    [Tooltip("Cavalry only — the mount/rider prefab. Leave empty for non-cavalry units.")]
    public GameObject riderGPUAnim;

#if UNITY_EDITOR
    // Baker runs only in the editor (during subscene baking and builds).
    // The EntityPrefabReference(GameObject) constructor uses AssetDatabase, so it is editor-only.
    // The baked UnitGPUAnimRefs entities (GUIDs) are serialized into the subscene and shipped in builds.
    public class Baker : Baker<UnitGPUAnimAuthoring>
    {
        public override void Bake(UnitGPUAnimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new UnitGPUAnimRefs
            {
                unitName = authoring.unitName,
                gpuAnim1 = new EntityPrefabReference(authoring.gpuAnim1),
                gpuAnim2 = new EntityPrefabReference(authoring.gpuAnim2),
                gpuAnim3 = new EntityPrefabReference(authoring.gpuAnim3),
                riderGPUAnim = authoring.riderGPUAnim != null
                    ? new EntityPrefabReference(authoring.riderGPUAnim)
                    : default,
            });
        }
    }
#endif
}

/// <summary>
/// Baked component: holds EntityPrefabReferences (GUIDs only, no loaded data).
/// Queried by UnitGPUAnimLoader to know which prefabs to load for a given unit.
/// riderGPUAnim.IsReferenceValid is false for non-cavalry units.
/// </summary>
public struct UnitGPUAnimRefs : IComponentData
{
    public UnitName unitName;
    public EntityPrefabReference gpuAnim1, gpuAnim2, gpuAnim3;
    public EntityPrefabReference riderGPUAnim;

    public readonly EntityPrefabReference Get(int index) => (index % 3) switch
    {
        0 => gpuAnim1,
        1 => gpuAnim2,
        2 => gpuAnim3,
        _ => default,
    };
}

/// <summary>
/// Runtime component: populated by UnitGPUAnimLoader once prefabs have finished loading.
/// SpawnManager and ArmySpawnManager query this via Find() to get the actual Entity prefabs.
/// riderEntity is Entity.Null for non-cavalry units.
/// Destroyed by UnitGPUAnimLoader.UnloadUnits() after the battle ends.
/// </summary>
public struct UnitGPUAnimPrefabs : IComponentData
{
    public UnitName unitName;
    public Entity gpuAnim1, gpuAnim2, gpuAnim3;
    public Entity riderEntity;

    public readonly Entity Get(int index) => (index % 3) switch
    {
        0 => gpuAnim1,
        1 => gpuAnim2,
        2 => gpuAnim3,
        _ => Entity.Null,
    };

    public readonly bool HasRider => riderEntity != Entity.Null;

    public static UnitGPUAnimPrefabs? Find(EntityManager entityManager, UnitName unitName)
    {
        using EntityQuery q = entityManager.CreateEntityQuery(ComponentType.ReadOnly<UnitGPUAnimPrefabs>());
        if (q.IsEmpty) return null;
        using NativeArray<UnitGPUAnimPrefabs> all = q.ToComponentDataArray<UnitGPUAnimPrefabs>(Allocator.Temp);
        foreach (var entry in all)
        {
            if (entry.unitName == unitName) return entry;
        }
        return null;
    }
}
