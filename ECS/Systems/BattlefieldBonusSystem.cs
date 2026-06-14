using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectDawn.Navigation;
using TJ.Morale;

partial struct BattlefieldBonusSystem : ISystem
{
    // Remove tag lookups (read-only — only checking presence)
    private ComponentLookup<RemoveBattlefieldBonusRain> _removeRainLookup;
    private ComponentLookup<RemoveBattlefieldBonusSnow> _removeSnowLookup;
    private ComponentLookup<RemoveBattlefieldBonusFog> _removeFogLookup;
    private ComponentLookup<RemoveChargeBonusTag> _removeChargeLookup;
    private ComponentLookup<RemoveSwampTag> _removeSwampLookup;
    private ComponentLookup<RemoveForestTag> _removeForestLookup;
    private ComponentLookup<RemoveBloodFrenzyTag> _removeBloodFrenzyLookup;
    private ComponentLookup<RemoveRageTag> _removeRageLookup;
    private ComponentLookup<RemoveEmblazingTag> _removeEmblazingLookup;
    // Squad state lookups (read-only)
    private ComponentLookup<InSwampTag> _inSwampLookup;
    private ComponentLookup<InRainTag> _inRainLookup;
    private ComponentLookup<InSnowTag> _inSnowLookup;
    private ComponentLookup<InForestTag> _inForestLookup;
    private ComponentLookup<LargeTag> _largeTagLookup;
    private ComponentLookup<LocalTransform> _existsLookup;
    // Unit + squad component writes
    private ComponentLookup<AgentLocomotion> _agentLocomotionLookup;
    private ComponentLookup<MeleeAttack> _meleeAttackLookup;
    private ComponentLookup<MeleeDefense> _meleeDefenseLookup;
    private ComponentLookup<ShootAttack> _shootAttackLookup;
    private ComponentLookup<ArmoredTag> _armoredTagLookup;
    private ComponentLookup<MoraleComponent> _moraleComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SquadStatsData>();
        _removeRainLookup        = state.GetComponentLookup<RemoveBattlefieldBonusRain>(true);
        _removeSnowLookup        = state.GetComponentLookup<RemoveBattlefieldBonusSnow>(true);
        _removeFogLookup         = state.GetComponentLookup<RemoveBattlefieldBonusFog>(true);
        _removeChargeLookup      = state.GetComponentLookup<RemoveChargeBonusTag>(true);
        _removeSwampLookup       = state.GetComponentLookup<RemoveSwampTag>(true);
        _removeForestLookup      = state.GetComponentLookup<RemoveForestTag>(true);
        _removeBloodFrenzyLookup = state.GetComponentLookup<RemoveBloodFrenzyTag>(true);
        _removeRageLookup        = state.GetComponentLookup<RemoveRageTag>(true);
        _removeEmblazingLookup   = state.GetComponentLookup<RemoveEmblazingTag>(true);
        _inSwampLookup           = state.GetComponentLookup<InSwampTag>(true);
        _inRainLookup            = state.GetComponentLookup<InRainTag>(true);
        _inSnowLookup            = state.GetComponentLookup<InSnowTag>(true);
        _inForestLookup          = state.GetComponentLookup<InForestTag>(true);
        _largeTagLookup          = state.GetComponentLookup<LargeTag>(true);
        _existsLookup            = state.GetComponentLookup<LocalTransform>(true);
        _agentLocomotionLookup   = state.GetComponentLookup<AgentLocomotion>(false);
        _meleeAttackLookup       = state.GetComponentLookup<MeleeAttack>(false);
        _meleeDefenseLookup      = state.GetComponentLookup<MeleeDefense>(false);
        _shootAttackLookup       = state.GetComponentLookup<ShootAttack>(false);
        _armoredTagLookup        = state.GetComponentLookup<ArmoredTag>(false);
        _moraleComponentLookup   = state.GetComponentLookup<MoraleComponent>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _removeRainLookup.Update(ref state);
        _removeSnowLookup.Update(ref state);
        _removeFogLookup.Update(ref state);
        _removeChargeLookup.Update(ref state);
        _removeSwampLookup.Update(ref state);
        _removeForestLookup.Update(ref state);
        _removeBloodFrenzyLookup.Update(ref state);
        _removeRageLookup.Update(ref state);
        _removeEmblazingLookup.Update(ref state);
        _inSwampLookup.Update(ref state);
        _inRainLookup.Update(ref state);
        _inSnowLookup.Update(ref state);
        _inForestLookup.Update(ref state);
        _largeTagLookup.Update(ref state);
        _existsLookup.Update(ref state);
        _agentLocomotionLookup.Update(ref state);
        _meleeAttackLookup.Update(ref state);
        _meleeDefenseLookup.Update(ref state);
        _shootAttackLookup.Update(ref state);
        _armoredTagLookup.Update(ref state);
        _moraleComponentLookup.Update(ref state);

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new BattlefieldBonusJob
        {
            RemoveRainLookup        = _removeRainLookup,
            RemoveSnowLookup        = _removeSnowLookup,
            RemoveFogLookup         = _removeFogLookup,
            RemoveChargeLookup      = _removeChargeLookup,
            RemoveSwampLookup       = _removeSwampLookup,
            RemoveForestLookup      = _removeForestLookup,
            RemoveBloodFrenzyLookup = _removeBloodFrenzyLookup,
            RemoveRageLookup        = _removeRageLookup,
            RemoveEmblazingLookup   = _removeEmblazingLookup,
            InSwampLookup           = _inSwampLookup,
            InRainLookup            = _inRainLookup,
            InSnowLookup            = _inSnowLookup,
            InForestLookup          = _inForestLookup,
            LargeTagLookup          = _largeTagLookup,
            ExistsLookup            = _existsLookup,
            AgentLocomotionLookup   = _agentLocomotionLookup,
            MeleeAttackLookup       = _meleeAttackLookup,
            MeleeDefenseLookup      = _meleeDefenseLookup,
            ShootAttackLookup       = _shootAttackLookup,
            ArmoredTagLookup        = _armoredTagLookup,
            MoraleComponentLookup   = _moraleComponentLookup,
            Ecb                     = ecb
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
partial struct BattlefieldBonusJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<RemoveBattlefieldBonusRain>  RemoveRainLookup;
    [ReadOnly] public ComponentLookup<RemoveBattlefieldBonusSnow>  RemoveSnowLookup;
    [ReadOnly] public ComponentLookup<RemoveBattlefieldBonusFog>   RemoveFogLookup;
    [ReadOnly] public ComponentLookup<RemoveChargeBonusTag>        RemoveChargeLookup;
    [ReadOnly] public ComponentLookup<RemoveSwampTag>              RemoveSwampLookup;
    [ReadOnly] public ComponentLookup<RemoveForestTag>             RemoveForestLookup;
    [ReadOnly] public ComponentLookup<RemoveBloodFrenzyTag>        RemoveBloodFrenzyLookup;
    [ReadOnly] public ComponentLookup<RemoveRageTag>               RemoveRageLookup;
    [ReadOnly] public ComponentLookup<RemoveEmblazingTag>          RemoveEmblazingLookup;
    [ReadOnly] public ComponentLookup<InSwampTag>                  InSwampLookup;
    [ReadOnly] public ComponentLookup<InRainTag>                   InRainLookup;
    [ReadOnly] public ComponentLookup<InSnowTag>                   InSnowLookup;
    [ReadOnly] public ComponentLookup<InForestTag>                 InForestLookup;
    [ReadOnly] public ComponentLookup<LargeTag>                    LargeTagLookup;
    [ReadOnly] public ComponentLookup<LocalTransform>              ExistsLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<AgentLocomotion> AgentLocomotionLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MeleeAttack>     MeleeAttackLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MeleeDefense>    MeleeDefenseLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<ShootAttack>     ShootAttackLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<ArmoredTag>      ArmoredTagLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MoraleComponent> MoraleComponentLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey, Entity entity,
        in SquadMovementComponent squadMovement,
        in SquadEntity squad,
        DynamicBuffer<BattlefieldBonusBufferElement> bonusBuffer,
        DynamicBuffer<EntityReferenceBufferElement> entityBuffer)
    {
        bool hasRemoveRain        = RemoveRainLookup.HasComponent(entity);
        bool hasRemoveSnow        = RemoveSnowLookup.HasComponent(entity);
        bool hasRemoveFog         = RemoveFogLookup.HasComponent(entity);
        bool hasRemoveCharge      = RemoveChargeLookup.HasComponent(entity);
        bool hasRemoveSwamp       = RemoveSwampLookup.HasComponent(entity);
        bool hasRemoveForest      = RemoveForestLookup.HasComponent(entity);
        bool hasRemoveBloodFrenzy = RemoveBloodFrenzyLookup.HasComponent(entity);
        bool hasRemoveRage        = RemoveRageLookup.HasComponent(entity);
        bool hasRemoveEmblazing   = RemoveEmblazingLookup.HasComponent(entity);

        for (int i = 0; i < bonusBuffer.Length; i++)
        {
            BattlefieldBonus bonus = bonusBuffer[i].Value;
            if (bonus.TargetedUnit != 0 && bonus.TargetedUnit != squad.SquadId) continue;

            if (hasRemoveRain && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Rain)
            {
                Ecb.RemoveComponent<InRainTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    var loc = AgentLocomotionLookup[unitEntity];
                    loc.Speed /= TabletopTavernConstants.RAIN_SPEED_MODIFIER;
                    loc.Acceleration /= TabletopTavernConstants.RAIN_SPEED_MODIFIER;
                    AgentLocomotionLookup[unitEntity] = loc;
                    Ecb.RemoveComponent<InRainTag>(sortKey, unitEntity);
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveSnow && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Snow)
            {
                var morale = MoraleComponentLookup[entity];
                morale.MaxMorale -= TabletopTavernConstants.SNOW_MORALE_PENALTY;
                morale.CurrentMorale -= TabletopTavernConstants.SNOW_MORALE_PENALTY;
                MoraleComponentLookup[entity] = morale;
                Ecb.RemoveComponent<InSnowTag>(sortKey, entity);
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveFog && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Fog)
            {
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    switch (bonus.UnitStat)
                    {
                        case UnitStat.Accuracy:
                            if (ShootAttackLookup.HasComponent(unitEntity))
                            {
                                var sa = ShootAttackLookup[unitEntity];
                                sa.Accuracy *= 2;
                                ShootAttackLookup[unitEntity] = sa;
                            }
                            break;
                        case UnitStat.Range:
                            if (ShootAttackLookup.HasComponent(unitEntity))
                            {
                                var sa = ShootAttackLookup[unitEntity];
                                sa.Range *= 2;
                                ShootAttackLookup[unitEntity] = sa;
                            }
                            break;
                    }
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveCharge && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.ChargeBonus)
            {
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    switch (bonus.UnitStat)
                    {
                        case UnitStat.WeaponStrength:
                            if (MeleeAttackLookup.HasComponent(unitEntity))
                            {
                                var ma = MeleeAttackLookup[unitEntity];
                                ma.WeaponStrength -= (int)bonus.Value;
                                MeleeAttackLookup[unitEntity] = ma;
                            }
                            break;
                        case UnitStat.MeleeAttack:
                            if (MeleeAttackLookup.HasComponent(unitEntity))
                            {
                                var ma = MeleeAttackLookup[unitEntity];
                                ma.MeleeAttackValue -= (int)bonus.Value;
                                MeleeAttackLookup[unitEntity] = ma;
                            }
                            break;
                    }
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveSwamp && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Swamp)
            {
                Ecb.RemoveComponent<InSwampTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (ExistsLookup.HasComponent(unitEntity))
                        Ecb.RemoveComponent<InSwampTag>(sortKey, unitEntity);
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveForest && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Forest)
            {
                Ecb.RemoveComponent<InForestTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (ExistsLookup.HasComponent(unitEntity))
                        Ecb.RemoveComponent<InForestTag>(sortKey, unitEntity);
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveBloodFrenzy && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.BloodFrenzy)
            {
                Ecb.RemoveComponent<RemoveBloodFrenzyTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    if (bonus.UnitStat == UnitStat.WeaponStrength && MeleeAttackLookup.HasComponent(unitEntity))
                    {
                        var ma = MeleeAttackLookup[unitEntity];
                        ma.WeaponStrength -= (int)bonus.Value;
                        MeleeAttackLookup[unitEntity] = ma;
                    }
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveRage && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Rage)
            {
                Ecb.RemoveComponent<RemoveRageTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    if (bonus.UnitStat == UnitStat.MeleeAttack && MeleeAttackLookup.HasComponent(unitEntity))
                    {
                        var ma = MeleeAttackLookup[unitEntity];
                        ma.WeaponStrength -= (int)bonus.Value;
                        MeleeAttackLookup[unitEntity] = ma;
                    }
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (hasRemoveEmblazing && bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Emblazing)
            {
                Ecb.RemoveComponent<RemoveEmblazingTag>(sortKey, entity);
                for (int j = 0; j < entityBuffer.Length; j++)
                {
                    Entity unitEntity = entityBuffer[j].Entity;
                    if (!ExistsLookup.HasComponent(unitEntity)) continue;
                    if (bonus.UnitStat == UnitStat.Armor && ArmoredTagLookup.HasComponent(unitEntity))
                    {
                        var at = ArmoredTagLookup[unitEntity];
                        at.ArmorMitigation -= bonus.Value;
                        ArmoredTagLookup[unitEntity] = at;
                    }
                }
                bonusBuffer.RemoveAt(i--);
                continue;
            }

            if (!bonus.Applied)
            {
                if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Forest)
                {
                    if (bonus.TargetedUnit == squad.SquadId && !InSwampLookup.HasComponent(entity))
                    {
                        Ecb.AddComponent<InForestTag>(sortKey, entity);
                        Ecb.AddComponent<RemoveChargeBonusTag>(sortKey, entity);
                        for (int j = 0; j < entityBuffer.Length; j++)
                        {
                            Entity unitEntity = entityBuffer[j].Entity;
                            if (ExistsLookup.HasComponent(unitEntity))
                                Ecb.AddComponent<InForestTag>(sortKey, unitEntity);
                        }
                        bonus.Applied = true;
                        bonusBuffer.RemoveAt(i--);
                        bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                        continue;
                    }
                }
                else if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Swamp)
                {
                    if (bonus.TargetedUnit == squad.SquadId && !InSwampLookup.HasComponent(entity))
                    {
                        Ecb.AddComponent<InSwampTag>(sortKey, entity);
                        Ecb.AddComponent<RemoveChargeBonusTag>(sortKey, entity);
                        for (int j = 0; j < entityBuffer.Length; j++)
                        {
                            Entity unitEntity = entityBuffer[j].Entity;
                            if (ExistsLookup.HasComponent(unitEntity))
                                Ecb.AddComponent<InSwampTag>(sortKey, unitEntity);
                        }
                        bonus.Applied = true;
                        bonusBuffer.RemoveAt(i--);
                        bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                        continue;
                    }
                }
                else if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Rain)
                {
                    bonus.Applied = true;
                    bonusBuffer.RemoveAt(i--);
                    if (!InRainLookup.HasComponent(entity) && LargeTagLookup.HasComponent(entity))
                    {
                        Ecb.AddComponent<InRainTag>(sortKey, entity);
                        Ecb.AddComponent<RemoveChargeBonusTag>(sortKey, entity);
                        for (int j = 0; j < entityBuffer.Length; j++)
                        {
                            Entity unitEntity = entityBuffer[j].Entity;
                            if (!ExistsLookup.HasComponent(unitEntity)) continue;
                            Ecb.AddComponent<InRainTag>(sortKey, unitEntity);
                            var loc = AgentLocomotionLookup[unitEntity];
                            loc.Speed *= TabletopTavernConstants.RAIN_SPEED_MODIFIER;
                            loc.Acceleration *= TabletopTavernConstants.RAIN_SPEED_MODIFIER;
                            AgentLocomotionLookup[unitEntity] = loc;
                        }
                        bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                    }
                }
                else if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Snow)
                {
                    bonus.Applied = true;
                    bonusBuffer.RemoveAt(i--);
                    bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                    if (!InSnowLookup.HasComponent(entity))
                    {
                        var morale = MoraleComponentLookup[entity];
                        morale.MaxMorale += TabletopTavernConstants.SNOW_MORALE_PENALTY;
                        morale.CurrentMorale += TabletopTavernConstants.SNOW_MORALE_PENALTY;
                        MoraleComponentLookup[entity] = morale;
                    }
                }
                else if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Fog)
                {
                    bonus.Applied = true;
                    bonusBuffer.RemoveAt(i--);
                    bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                    for (int j = 0; j < entityBuffer.Length; j++)
                    {
                        Entity unitEntity = entityBuffer[j].Entity;
                        if (!ExistsLookup.HasComponent(unitEntity)) continue;
                        switch (bonus.UnitStat)
                        {
                            case UnitStat.Accuracy:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Accuracy = (int)(sa.Accuracy * 0.5f);
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                            case UnitStat.Range:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Range = (int)(sa.Range * 0.5f);
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    bonus.Applied = true;
                    bonusBuffer.RemoveAt(i--);
                    bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = bonus });
                    for (int j = 0; j < entityBuffer.Length; j++)
                    {
                        Entity unitEntity = entityBuffer[j].Entity;
                        if (!ExistsLookup.HasComponent(unitEntity)) continue;
                        switch (bonus.UnitStat)
                        {
                            case UnitStat.MeleeAttack:
                                if (MeleeAttackLookup.HasComponent(unitEntity))
                                {
                                    var ma = MeleeAttackLookup[unitEntity];
                                    ma.MeleeAttackValue += (int)bonus.Value;
                                    MeleeAttackLookup[unitEntity] = ma;
                                }
                                break;
                            case UnitStat.MeleeDefense:
                                if (MeleeDefenseLookup.HasComponent(unitEntity))
                                {
                                    var md = MeleeDefenseLookup[unitEntity];
                                    md.Value += (int)bonus.Value;
                                    MeleeDefenseLookup[unitEntity] = md;
                                }
                                break;
                            case UnitStat.WeaponStrength:
                                if (MeleeAttackLookup.HasComponent(unitEntity))
                                {
                                    var ma = MeleeAttackLookup[unitEntity];
                                    ma.WeaponStrength += (int)bonus.Value;
                                    MeleeAttackLookup[unitEntity] = ma;
                                }
                                break;
                            case UnitStat.Accuracy:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Accuracy += (int)bonus.Value;
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                            case UnitStat.Range:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Range += (int)bonus.Value;
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                            case UnitStat.Armor:
                                if (ArmoredTagLookup.HasComponent(unitEntity))
                                {
                                    var at = ArmoredTagLookup[unitEntity];
                                    at.ArmorMitigation += bonus.Value;
                                    ArmoredTagLookup[unitEntity] = at;
                                }
                                break;
                        }
                    }
                }
            }

            // Distance-based removal
            float distance = math.distance(squadMovement.SquadCenter, bonus.OriginationPoint);
            if (distance - 5 > bonus.Range)
            {
                if (bonus.Applied)
                {
                    if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Forest && InForestLookup.HasComponent(entity))
                        Ecb.RemoveComponent<InForestTag>(sortKey, entity);

                    for (int j = 0; j < entityBuffer.Length; j++)
                    {
                        Entity unitEntity = entityBuffer[j].Entity;
                        if (!ExistsLookup.HasComponent(unitEntity)) continue;

                        if (bonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Forest && InForestLookup.HasComponent(entity))
                        {
                            Ecb.RemoveComponent<InForestTag>(sortKey, unitEntity);
                            continue;
                        }

                        switch (bonus.UnitStat)
                        {
                            case UnitStat.MeleeAttack:
                                if (MeleeAttackLookup.HasComponent(unitEntity))
                                {
                                    var ma = MeleeAttackLookup[unitEntity];
                                    ma.MeleeAttackValue -= (int)bonus.Value;
                                    MeleeAttackLookup[unitEntity] = ma;
                                }
                                break;
                            case UnitStat.MeleeDefense:
                                if (MeleeDefenseLookup.HasComponent(unitEntity))
                                {
                                    var md = MeleeDefenseLookup[unitEntity];
                                    md.Value -= (int)bonus.Value;
                                    MeleeDefenseLookup[unitEntity] = md;
                                }
                                break;
                            case UnitStat.WeaponStrength:
                                if (MeleeAttackLookup.HasComponent(unitEntity))
                                {
                                    var ma = MeleeAttackLookup[unitEntity];
                                    ma.WeaponStrength -= (int)bonus.Value;
                                    MeleeAttackLookup[unitEntity] = ma;
                                }
                                break;
                            case UnitStat.Accuracy:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Accuracy -= (int)bonus.Value;
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                            case UnitStat.Range:
                                if (ShootAttackLookup.HasComponent(unitEntity))
                                {
                                    var sa = ShootAttackLookup[unitEntity];
                                    sa.Range -= (int)bonus.Value;
                                    ShootAttackLookup[unitEntity] = sa;
                                }
                                break;
                        }
                    }
                }
                bonusBuffer.RemoveAt(i--);
            }
        }

        if (hasRemoveRain)   Ecb.RemoveComponent<RemoveBattlefieldBonusRain>(sortKey, entity);
        if (hasRemoveFog)    Ecb.RemoveComponent<RemoveBattlefieldBonusFog>(sortKey, entity);
        if (hasRemoveSnow)   Ecb.RemoveComponent<RemoveBattlefieldBonusSnow>(sortKey, entity);
        if (hasRemoveCharge) Ecb.RemoveComponent<RemoveChargeBonusTag>(sortKey, entity);
        if (hasRemoveSwamp)  Ecb.RemoveComponent<RemoveSwampTag>(sortKey, entity);
        if (hasRemoveForest) Ecb.RemoveComponent<RemoveForestTag>(sortKey, entity);
    }
}
