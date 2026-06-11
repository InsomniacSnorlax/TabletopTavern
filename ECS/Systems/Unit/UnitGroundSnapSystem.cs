using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct UnitGroundSnapSystem : ISystem
{
    const float EXPECTED_Y = 0.25f;
    const float TOLERANCE = 0.025f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
            .WithAll<Unit>()
            .WithNone<ThrowUnit, KillUnitTag>()
            .WithEntityAccess())
        {
            if (math.abs(localTransform.ValueRO.Position.y - EXPECTED_Y) > TOLERANCE)
            {
                // Debug.LogError($"UnitGroundSnapSystem: Entity {entity} has Y={localTransform.ValueRO.Position.y}, expected {EXPECTED_Y}. Snapping.");
                // localTransform.ValueRW.Position.y = EXPECTED_Y;
            }
        }
    }
}
