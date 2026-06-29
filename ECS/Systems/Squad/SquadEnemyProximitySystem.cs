using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct SquadEnemyProximitySystem : ISystem
{
    const float TRIGGER_DISTANCE_SQ = 40f * 40f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var playerPositions = new NativeList<float3>(Allocator.Temp);

        foreach (var movement in SystemAPI.Query<SquadMovementComponent>()
            .WithAll<PlayerSquad>().WithNone<BrokenSquadTag>())
            playerPositions.Add(movement.SquadCenter);

        foreach (var (movement, entity) in SystemAPI.Query<SquadMovementComponent>()
            .WithAll<EnemySquad, WaitingForCommand>().WithNone<BrokenSquadTag>().WithEntityAccess())
        {
            float3 center = movement.SquadCenter;

            for (int i = 0; i < playerPositions.Length; i++)
            {
                if (math.distancesq(center, playerPositions[i]) <= TRIGGER_DISTANCE_SQ)
                {
                    ecb.SetComponentEnabled<WaitingForCommand>(entity, false);
                    break;
                }
            }
        }

        playerPositions.Dispose();
    }
}
