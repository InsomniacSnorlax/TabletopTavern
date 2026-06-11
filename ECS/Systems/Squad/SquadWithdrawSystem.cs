using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

partial struct SquadWithdrawSystem : ISystem
{
    private SpawnBox gameBox;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
        gameBox = new() {
            min = new float3(-185, 0, -150),
            max = new float3(185, 0, 150)
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new SquadWithdrawJob { GameBox = gameBox, Ecb = ecb }
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(WithdrawSquadTag))]
partial struct SquadWithdrawJob : IJobEntity
{
    [ReadOnly] public SpawnBox GameBox;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey, ref SquadEntity squad, ref SquadMovementComponent movement)
    {
        float3 c = movement.SquadCenter;
        if (c.x >= GameBox.min.x && c.x <= GameBox.max.x &&
            c.z >= GameBox.min.z && c.z <= GameBox.max.z)
            return;

        Ecb.RemoveComponent<WithdrawSquadTag>(sortKey, squad.SelfEntity);
        Ecb.AddComponent(sortKey, squad.SelfEntity, new WithdrawCompleteTag());
    }
}
