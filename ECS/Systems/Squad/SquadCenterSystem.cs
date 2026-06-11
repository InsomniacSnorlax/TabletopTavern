using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

partial struct SquadCenterSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private ComponentLookup<SetDestination> setDestinationComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(false);
        setDestinationComponentLookup = state.GetComponentLookup<SetDestination>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        localTransformComponentLookup.Update(ref state);
        setDestinationComponentLookup.Update(ref state);

        SquadCenterJob squadCenterJob = new SquadCenterJob {
            LocalTransformLookup = localTransformComponentLookup,
            SetDestinationLookup = setDestinationComponentLookup
        };
        state.Dependency = squadCenterJob.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAbsent(typeof(GarrisonGateSquadTag))]
public partial struct SquadCenterJob : IJobEntity {
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    [ReadOnly] public ComponentLookup<SetDestination> SetDestinationLookup;
    public void Execute (
        in DynamicBuffer<EntityReferenceBufferElement> entityBuffer,
        // ref LocalTransform localTransform, 
        ref SquadMovementComponent squadMovement
    ) {

        if (entityBuffer.Length == 0) return;

        // ---------- SINGLE PASS: sum + min + max ----------
        float3 sum   = float3.zero;
        float3 min   = new float3(float.MaxValue);
        float3 max   = new float3(float.MinValue);

        for (int i = 0; i < entityBuffer.Length; i++)
        {
            var unitEntity = entityBuffer[i].Entity;
            var lt = LocalTransformLookup[unitEntity];
            var pos = lt.Position;

            sum += pos;
            min  = math.min(min, pos);
            max  = math.max(max, pos);

            var debugEntity = entityBuffer[i].DebugEntity;
            var offsetLt = LocalTransformLookup[debugEntity];
            offsetLt.Position = SetDestinationLookup[unitEntity].squadPosition;
            LocalTransformLookup[debugEntity] = offsetLt;
        }

        float3 center = sum / entityBuffer.Length;
        float3 size   = max - min;
        float radius  = math.length(size) * 0.5f; // quick approx sphere

        //check for NaN
        if (!math.all(math.isfinite(center)))
        {
            // Debug.LogError($"SquadCenterJob: NaN/Inf detected in squad center. Resetting bounds to zero.");
            return;
        }
        squadMovement.SquadCenter  = new float3(center.x, 0, center.z);
        squadMovement.BoundsMin    = min;
        squadMovement.BoundsMax    = max;
        squadMovement.BoundsRadius = radius;
    }
}