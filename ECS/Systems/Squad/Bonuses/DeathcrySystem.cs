using Unity.Entities;
using Unity.Collections;

// RavenHost passive — "Deathcry"
// When a RavenHost squad falls (destroyed, broken, or withdrawn), all surviving RavenHost squads
// gain +20 Attack for 20 seconds. A second fall resets the timer but does not stack the bonus.
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(DestroySquadSystem))]
partial struct DeathcrySystem : ISystem
{
    public readonly void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;
        float deltaTime = SystemAPI.Time.DeltaTime;

        bool anyJustFell = false;
        var toTag = new NativeList<Entity>(Allocator.Temp);

        foreach (var (entityBuffer, entity) in SystemAPI.Query<
            DynamicBuffer<EntityReferenceBufferElement>>()
            .WithAll<RavenHostRaceTag>()
            .WithNone<DeathcryTriggeredTag>()
            .WithEntityAccess())
        {
            if (entityBuffer.Length > 0) continue;
            toTag.Add(entity);
            anyJustFell = true;
        }

        foreach (var (_, entity) in SystemAPI.Query<RavenHostRaceTag>()
            .WithAll<BrokenSquadTag>()
            .WithNone<DeathcryTriggeredTag>()
            .WithEntityAccess())
        {
            toTag.Add(entity);
            anyJustFell = true;
        }

        foreach (var (_, entity) in SystemAPI.Query<RavenHostRaceTag>()
            .WithAll<WithdrawSquadTag>()
            .WithNone<DeathcryTriggeredTag>()
            .WithEntityAccess())
        {
            toTag.Add(entity);
            anyJustFell = true;
        }

        foreach (Entity e in toTag)
            entityManager.AddComponent<DeathcryTriggeredTag>(e);
        toTag.Dispose();

        if (anyJustFell)
        {
            foreach (var (entityBuffer, bonusBuffer, deathcry) in SystemAPI.Query<
                DynamicBuffer<EntityReferenceBufferElement>,
                DynamicBuffer<BattlefieldBonusBufferElement>,
                RefRW<DeathcryComponent>>()
                .WithAll<RavenHostRaceTag>()
                .WithNone<DeathcryTriggeredTag>())
            {
                if (entityBuffer.Length == 0) continue;

                if (deathcry.ValueRO.AppliedBonus == 0)
                {
                    for (int i = 0; i < entityBuffer.Length; i++)
                    {
                        Entity unitEntity = entityBuffer[i].Entity;
                        if (!entityManager.HasComponent<MeleeAttack>(unitEntity)) continue;
                        MeleeAttack attack = entityManager.GetComponentData<MeleeAttack>(unitEntity);
                        attack.MeleeAttackValue += 20;
                        entityManager.SetComponentData(unitEntity, attack);
                    }
                    deathcry.ValueRW.AppliedBonus = 20;
                    SyncBonusBuffer(bonusBuffer, 20);
                }

                deathcry.ValueRW.TimeRemaining = 20f;
            }
        }

        foreach (var (entityBuffer, bonusBuffer, deathcry) in SystemAPI.Query<
            DynamicBuffer<EntityReferenceBufferElement>,
            DynamicBuffer<BattlefieldBonusBufferElement>,
            RefRW<DeathcryComponent>>()
            .WithAll<RavenHostRaceTag>())
        {
            if (deathcry.ValueRO.AppliedBonus <= 0 || entityBuffer.Length == 0) continue;

            deathcry.ValueRW.TimeRemaining -= deltaTime;
            if (deathcry.ValueRO.TimeRemaining > 0f) continue;

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity unitEntity = entityBuffer[i].Entity;
                if (!entityManager.HasComponent<MeleeAttack>(unitEntity)) continue;
                MeleeAttack attack = entityManager.GetComponentData<MeleeAttack>(unitEntity);
                attack.MeleeAttackValue -= deathcry.ValueRO.AppliedBonus;
                entityManager.SetComponentData(unitEntity, attack);
            }

            deathcry.ValueRW.AppliedBonus = 0;
            deathcry.ValueRW.TimeRemaining = 0f;
            SyncBonusBuffer(bonusBuffer, 0);
        }
    }

    private static void SyncBonusBuffer(DynamicBuffer<BattlefieldBonusBufferElement> bonusBuffer, int totalBonus)
    {
        for (int i = bonusBuffer.Length - 1; i >= 0; i--)
        {
            if (bonusBuffer[i].Value.BattlefieldBonusEnum == BattlefieldBonusEnum.Deathcry)
                bonusBuffer.RemoveAt(i);
        }
        if (totalBonus > 0)
        {
            bonusBuffer.Add(new BattlefieldBonusBufferElement
            {
                Value = new BattlefieldBonus
                {
                    UnitStat = UnitStat.MeleeAttack,
                    BattlefieldBonusEnum = BattlefieldBonusEnum.Deathcry,
                    Value = totalBonus,
                    Applied = true,
                    Range = 999999f
                }
            });
        }
    }
}
