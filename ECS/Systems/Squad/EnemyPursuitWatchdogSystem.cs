using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Detects when a charging enemy squad (melee, ranged, or artillery) is making no net progress
// toward its target - i.e. the target is kiting / uncatchable - and abandons it: blacklists that
// target for a cooldown, drops the charge, and dazes briefly so re-acquisition picks a different
// squad. The no-progress signal is unit-type-agnostic: ranged/artillery lose ChargeSquad once
// they reach AttackRange (they engage), so a genuine approach never false-trips; only a target
// they can never close on does. Sibling to EnemyArmyTargetOverrideSystem (same throttle / ECB /
// Burst surface). Runs after it so a legitimate closer-target override this tick is seen as a
// target change and resets the progress timer rather than inheriting a stale give-up countdown.
[UpdateAfter(typeof(EnemyArmyTargetOverrideSystem))]
partial struct EnemyPursuitWatchdogSystem : ISystem
{
    private double lastExecutionTime;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
        lastExecutionTime = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // only fire twice per second (matches EnemyArmyTargetOverrideSystem)
        double now = SystemAPI.Time.ElapsedTime;
        if (now - lastExecutionTime < 0.5f) return;
        lastExecutionTime = now;

        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (squad, squadMovementComponent, blacklist) in SystemAPI.Query<
            RefRW<SquadEntity>,
            RefRO<SquadMovementComponent>,
            DynamicBuffer<TargetBlacklistElement>
        >()
        .WithAll<EnemySquad, ChargeSquad>()
        .WithNone<InCombat, BrokenSquadTag, CavalryFlankingTag>()
        .WithNone<WithdrawSquadTag, OpponentRanAwayTag, SquadMoveOverrideTag>())
        {
            Entity self = squad.ValueRO.SelfEntity;
            Entity target = squad.ValueRO.TargetSquadEntity;

            // No live target: the charge system handles ChargeSquad removal; just clear our state.
            if (target == Entity.Null || !entityManager.Exists(target))
            {
                if (entityManager.HasComponent<PursuitProgress>(self))
                    entityCommandBuffer.RemoveComponent<PursuitProgress>(self);
                continue;
            }

            float dist = math.distance(
                squadMovementComponent.ValueRO.SquadCenter,
                entityManager.GetComponentData<SquadMovementComponent>(target).SquadCenter);

            // First sample for this pursuit, or the target changed since last sample: (re)initialise.
            if (!entityManager.HasComponent<PursuitProgress>(self))
            {
                entityCommandBuffer.AddComponent(self, new PursuitProgress
                {
                    TrackedTarget = target,
                    BestDistance = dist,
                    TimeSinceImproved = 0f,
                    LastSampleTime = now
                });
                continue;
            }

            PursuitProgress progress = entityManager.GetComponentData<PursuitProgress>(self);
            if (progress.TrackedTarget != target)
            {
                progress.TrackedTarget = target;
                progress.BestDistance = dist;
                progress.TimeSinceImproved = 0f;
                progress.LastSampleTime = now;
                entityManager.SetComponentData(self, progress);
                continue;
            }

            float dt = (float)(now - progress.LastSampleTime);
            progress.LastSampleTime = now;

            if (dist < progress.BestDistance - TabletopTavernConstants.MELEE_PURSUIT_CLOSE_EPSILON)
            {
                // Genuine net progress toward the target: reset the give-up timer.
                progress.BestDistance = dist;
                progress.TimeSinceImproved = 0f;
            }
            else
            {
                progress.TimeSinceImproved += dt;
            }

            if (progress.TimeSinceImproved >= TabletopTavernConstants.MELEE_PURSUIT_GIVEUP_TIME)
            {
                // Give up on this uncatchable target.
                blacklist.Add(new TargetBlacklistElement
                {
                    Target = target,
                    ExpireTime = now + TabletopTavernConstants.MELEE_TARGET_BLACKLIST_COOLDOWN
                });

                // Reuse existing primitives: HaltCommandTag{DropTarget} removes ChargeSquad, nulls the
                // target, disengages and halts; OpponentRanAwayTag dazes so re-acquire is delayed and
                // won't instantly re-lock. Clear pursuit state.
                entityCommandBuffer.AddComponent(self, new HaltCommandTag { DropTarget = true });
                entityCommandBuffer.AddComponent(self, new OpponentRanAwayTag { DazedTime = TabletopTavernConstants.DAZED_ON_DISENGAGE_TIME });
                entityCommandBuffer.RemoveComponent<PursuitProgress>(self);
                continue;
            }

            entityManager.SetComponentData(self, progress);
        }
    }
}
