using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
partial struct SquadCommandChangedSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new InitSquadCommandJob { Ecb = ecb }
            .ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSquadCommandChangeJob { Ecb = ecb }
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(PreviousSquadCommandComponent))]
partial struct InitSquadCommandJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in SquadEntity squad)
    {
        Ecb.AddComponent(sortKey, entity, new PreviousSquadCommandComponent
        {
            Command = squad.SquadCommand
        });
    }
}

[BurstCompile]
partial struct DetectSquadCommandChangeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in SquadEntity squad, ref PreviousSquadCommandComponent prev)
    {
        SquadCommand current = squad.SquadCommand;
        if (current == prev.Command) return;

        Ecb.AddComponent(sortKey, entity, new SquadCommandChangedTag
        {
            OldCommand = prev.Command,
            NewCommand = current
        });
        prev.Command = current;
    }
}
