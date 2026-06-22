using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(SquadTotalHealthSystem))]
partial struct LowHealthSquadCleanupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        NativeList<Entity> unitsToKill = new NativeList<Entity>(Allocator.Temp);

        foreach (var (squadState, entityBuffer) in SystemAPI.Query<
            RefRO<SquadStateComponent>,
            DynamicBuffer<EntityReferenceBufferElement>>())
        {
            if (squadState.ValueRO.MaxHealthValue <= 0) continue;
            if (squadState.ValueRO.CurrentHealthValue <= 0) continue;

            float healthPercent = (float)squadState.ValueRO.CurrentHealthValue / squadState.ValueRO.MaxHealthValue;
            if (healthPercent >= 0.05f) continue;

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity unitEntity = entityBuffer[i].Entity;
                if (!entityManager.HasComponent<Health>(unitEntity)) continue;
                if (entityManager.HasComponent<UnitRemovedFromSquad>(unitEntity)) continue;
                Health health = entityManager.GetComponentData<Health>(unitEntity);
                if (health.Value <= 0) continue;
                unitsToKill.Add(unitEntity);
            }
        }

        for (int i = 0; i < unitsToKill.Length; i++)
        {
            Entity unitEntity = unitsToKill[i];
            if (!entityManager.Exists(unitEntity)) continue;

            Health health = entityManager.GetComponentData<Health>(unitEntity);
            health.Value = 0;
            entityManager.SetComponentData(unitEntity, health);

            Unit unit = entityManager.GetComponentData<Unit>(unitEntity);
            entityManager.AddComponentData(unitEntity, new UnitRemovedFromSquad
            {
                Entity = unitEntity,
                SquadId = unit.squadId,
                KilledBySquadId = 100,
                DeleteCorpse = false,
            });
        }

        unitsToKill.Dispose();
    }
}
