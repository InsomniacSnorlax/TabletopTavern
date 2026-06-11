using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventsSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        state.Dependency = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
        state.Dependency = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
        state.Dependency = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
        // new ResetMeleeEventsJob().ScheduleParallel();
        // new ResetSquadTargetDestroyed().ScheduleParallel();
        /*
        foreach (RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>()) {
            selected.ValueRW.onSelected = false;
            selected.ValueRW.onDeselected = false;
        }
        foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>()) {
            health.ValueRW.onHealthChanged = false;
        }
        foreach (RefRW<ShootAttack> shootAttack in SystemAPI.Query<RefRW<ShootAttack>>()) {
            shootAttack.ValueRW.onShoot.isTriggered = false;
        }
        */
    }
}

[BurstCompile]
public partial struct ResetShootAttackEventsJob : IJobEntity {
    public void Execute(ref ShootAttack shootAttack) {
        shootAttack.onShoot.isTriggered = false;
    }
}

[BurstCompile]
public partial struct ResetHealthEventsJob : IJobEntity {
    public void Execute(ref Health health) {
        health.onHealthChanged = false;
    }
}

[BurstCompile]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ResetSelectedEventsJob : IJobEntity {
    public void Execute(ref Selected selected) {
        selected.onSelected = false;
        selected.onDeselected = false;
    }
}
public partial struct ResetHoveredEventsJob : IJobEntity {
    public void Execute(ref Hovered hovered) {
        hovered.onHover = false;
        hovered.onUnhover = false;
        hovered.onDeselected = false;
        hovered.onSelected = false;
    }
}

// [BurstCompile]
// public partial struct ResetMeleeEventsJob : IJobEntity {
//     public void Execute(ref MeleeAttack attack) {
//         attack.onAttack = false;
//     }
// }

// [BurstCompile]
// public partial struct ResetSquadTargetDestroyed : IJobEntity {
//     public void Execute(ref SquadEntity squad) {
//         squad.onTargetDestroyed = false;
//     }
// }