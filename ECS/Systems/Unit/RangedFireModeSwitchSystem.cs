using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

partial struct RangedFireModeSwitchSystem : ISystem
{
    private ComponentLookup<RangedFireModeUnitComponent> _fireModeUnitLookup;
    private ComponentLookup<ShootAttack> _shootAttackLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _fireModeUnitLookup = state.GetComponentLookup<RangedFireModeUnitComponent>(false);
        _shootAttackLookup  = state.GetComponentLookup<ShootAttack>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _fireModeUnitLookup.Update(ref state);
        _shootAttackLookup.Update(ref state);

        state.Dependency = new RangedFireModeSwitchJob
        {
            FireModeUnitLookup = _fireModeUnitLookup,
            ShootAttackLookup  = _shootAttackLookup
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
partial struct RangedFireModeSwitchJob : IJobEntity
{
    public ComponentLookup<RangedFireModeUnitComponent> FireModeUnitLookup;
    public ComponentLookup<ShootAttack> ShootAttackLookup;

    public void Execute(Entity entity, ref RangedFireModeSquadComponent fireModeSquad,
        DynamicBuffer<EntityReferenceBufferElement> entityBuffer)
    {
        if (!fireModeSquad.SwitchRequested) return;
        fireModeSquad.SwitchRequested = false;

        var random = new Random((uint)(entity.Index + 1));

        for (int i = 0; i < entityBuffer.Length; i++)
        {
            Entity unitEntity = entityBuffer[i].Entity;

            if (FireModeUnitLookup.HasComponent(unitEntity))
            {
                var unit = FireModeUnitLookup[unitEntity];
                unit.FireMode = fireModeSquad.FireMode;
                FireModeUnitLookup[unitEntity] = unit;
            }

            if (ShootAttackLookup.HasComponent(unitEntity))
            {
                var shoot = ShootAttackLookup[unitEntity];
                shoot.timer = fireModeSquad.FireMode == RangedFireMode.FireAtWill
                    ? random.NextFloat(0f, 0.5f)
                    : shoot.timerMax + random.NextFloat(0f, 0.5f);
                ShootAttackLookup[unitEntity] = shoot;
            }
        }
    }
}
