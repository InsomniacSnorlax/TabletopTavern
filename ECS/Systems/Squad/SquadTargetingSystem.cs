using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct SquadTargettingSystem : ISystem
{
    private Unity.Mathematics.Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
        _random = Unity.Mathematics.Random.CreateFromIndex(0);
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (squad, SquadTargettingComponent, ChargeSquad) in SystemAPI.Query<
            RefRW<SquadEntity>,
            RefRW<SquadTargettingComponent>,
            ChargeSquad>
        ().WithDisabled<SquadTargettingComponent, WaitingForCommand>()
        .WithAbsent<SquadMoveOverrideTag>().WithNone<BrokenSquadTag>()){
            SquadTargettingComponent.ValueRW.UpdateTargetDestinationRefreshRate -= SystemAPI.Time.DeltaTime;

            if(SquadTargettingComponent.ValueRO.UpdateTargetDestinationRefreshRate <= 0f) {
                entityCommandBuffer.SetComponentEnabled<SquadTargettingComponent>(squad.ValueRO.SelfEntity, true);
                SquadTargettingComponent.ValueRW.UpdateTargetDestinationRefreshRate = _random.NextFloat(0.2f, 0.25f);

                //no overrride nonsense for player squads
                if(squad.ValueRO.SquadId > 0) continue;
            }
        }
    }
}

