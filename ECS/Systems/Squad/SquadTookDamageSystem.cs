using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct SquadTookDamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (squadState, entity) in SystemAPI.Query<SquadStateComponent>()
            .WithAll<WaitingForCommand, EnemySquad>()
            .WithPresent<HasTakenDamage>()
            .WithAbsent<GarrisonDefenderComponent>()
            .WithEntityAccess())
        {
            if (squadState.CurrentHealthValue < squadState.MaxHealthValue)
            {
                ecb.SetComponentEnabled<WaitingForCommand>(entity, false);
                ecb.SetComponentEnabled<HasTakenDamage>(entity, true);
            }
        }
    }
}
