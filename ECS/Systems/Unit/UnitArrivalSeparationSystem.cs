using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectDawn.Navigation;

/// <summary>
/// Reduces AgentSeparation weight as a unit closes in on its formation slot so it can
/// squeeze past nearby squadmates to reach its destination. Full weight is restored once
/// the unit is far enough from its target to be in free movement.
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct UnitArrivalSeparationSystem : ISystem
{
    // Distance below which separation weight starts to taper. At exactly this distance
    // the weight is unchanged; at zero distance it equals ArrivalWeight.
    const float TaperStartDistance = 2.5f;

    // Minimum weight applied right at the destination — low enough to squeeze in,
    // but not zero so units still avoid fully stacking.
    const float ArrivalWeight = 0.15f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<Unit> unit,
            RefRO<SetDestination> setDestination,
            RefRW<AgentSeparation> agentSeparation,
            RefRO<BaseSeparationWeight> baseSeparationWeight,
            RefRO<LocalTransform> localTransform
            ) in SystemAPI.Query<
                RefRO<Unit>,
                RefRO<SetDestination>,
                RefRW<AgentSeparation>,
                RefRO<BaseSeparationWeight>,
                RefRO<LocalTransform>
            >().WithPresent<AgentSeparation>())
        {
            if (unit.ValueRO.unitState != UnitState.Moving)
            {
                // Restore full weight outside of formation movement so combat behaviour is unaffected.
                agentSeparation.ValueRW.Weight = baseSeparationWeight.ValueRO.Value;
                continue;
            }

            float3 toDestination = setDestination.ValueRO.destinationPosition - localTransform.ValueRO.Position;
            // Only care about XZ distance — Y differences from terrain are irrelevant.
            float distXZ = math.length(new float2(toDestination.x, toDestination.z));

            float t = math.saturate(distXZ / TaperStartDistance);
            agentSeparation.ValueRW.Weight = math.lerp(ArrivalWeight, baseSeparationWeight.ValueRO.Value, t);
        }
    }
}
