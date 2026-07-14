using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using TJ;
using Unity.Mathematics;
using Unity.Transforms;
using Memori.Audio;
using ProjectDawn.Navigation;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct UnitSetUpSystem : ISystem
{
    private Unity.Mathematics.Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CampaignSaveDataHolder>();
        state.RequireForUpdate<SquadStatsData>();
        _random = new Unity.Mathematics.Random(1);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        CampaignSaveDataHolder campaignSaveDataHolder = SystemAPI.GetSingleton<CampaignSaveDataHolder>();
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        var statsData = SystemAPI.GetSingleton<SquadStatsData>();
        ref var statsBlob = ref statsData.StatsBlob.Value; 

        foreach (var ( 
            unit,
            UnitStatsSetUpTag,
            entity
        ) in SystemAPI.Query<
            RefRO<Unit>,
            RefRO<UnitStatsSetUpTag>
        >().WithEntityAccess()) {
            
            SquadStats squadStats = statsBlob.GetStats(unit.ValueRO.unitName);
            UnitAttributeSerialized unitAttributes = new();

            unitAttributes.ArmorPiercing = squadStats.SquadAttributes.ArmorPiercing;
            unitAttributes.AntiInfantry = squadStats.SquadAttributes.AntiInfantry;
            unitAttributes.AntiLarge = squadStats.SquadAttributes.AntiLarge;
            unitAttributes.Armored = squadStats.Armor > 0;
            unitAttributes.Large = squadStats.unitSize != UnitSize.Infantry;

            Team team = unit.ValueRO.Team;

            int meleeAttack = squadStats.MeleeAttack;
            int meleeDefense = squadStats.MeleeDefense;
            float accuracy = squadStats.attackAccuracy;
            float range = squadStats.BaseRange;
            int maxHitPoints = squadStats.HitPointsPerUnit;
            if(squadStats.unitType == UnitType.Structure) {
                maxHitPoints = UnitStatsSetUpTag.ValueRO.HealthOverride;
            }

            entityCommandBuffer.RemoveComponent<UnitStatsSetUpTag>(entity);

            float GetShieldBlockChance() {
                if (squadStats.SquadAttributes.StandardShields)
                {
                    return 0.35f; // 35% base block chance for shielded units
                }
                else if (squadStats.SquadAttributes.HeavyShields)
                {
                    return 0.70f; // 70% base block chance for heavy shielded units
                }
                // else if (squadStats.SquadAttributes.TowerShields)
                // {
                //     return 1f; // 100% block chance for tower shielded units
                // }

                return 0;
            }
            float shieldBlockChance = GetShieldBlockChance();
            int missileStrength = squadStats.MissileStrength;
            int weaponStrength = squadStats.WeaponStrength;
            float armor = squadStats.Armor;
            
            GearIDsSerialized gear = campaignSaveDataHolder.Gear;

            bool Contains(GearID gearID) {
                if(gear.gearID1 == gearID || gear.gearID2 == gearID || gear.gearID3 == gearID || gear.gearID4 == gearID) return true;
                return false;
            }

            //hero stuff
            if(campaignSaveDataHolder.ActiveHeroID != -1 && team == Team.Player)
            {
                switch(campaignSaveDataHolder.ActiveHeroID)
                {
                    case 2:
                        //Dúnedain Captain: Deepwood Rangers gain +10 [Accuracy] and +4 [Missile Strength]
                        if(unit.ValueRO.unitName == UnitName.DeepwoodRangers) {
                            accuracy += 10;
                            missileStrength += 4;
                        }
                        //The Everyman: [Common] units gain +10 [Leadership] and +4 [Melee Defense]
                        if((int)squadStats.RarityTier+1 == 1) {
                            meleeDefense += 4;
                        }
                        break;
                    case 3:
                        //go forth my hordes: Goblins gain +6 [Melee Defense] and +6 [Melee Attack]
                        if(TabletopTavernConstants.IsAGoblinUnit(unit.ValueRO.unitName)) {
                            meleeDefense += 6;
                            meleeAttack += 6;
                        }
                        break;
                    case 4:
                        //A taste for blood: Orc Ravagers cause [Terror] and gain +10 [Melee Attack] and +4 [Weapon Strength]
                        if(unit.ValueRO.unitName == UnitName.OrcRavagers) {
                            meleeAttack += 10;
                            weaponStrength += 4;
                        }
                        break;
                    case 5:
                        //Ironskin: Melee Infantry gain +4 [Melee Defense], Armored Units gain +10 [Armor]
                        if(UnitType.Melee == squadStats.unitType) {
                            meleeDefense += 4;
                            armor += 10;
                        }
                        break;
                    case 6:
                        //With me sisters!: Shieldmaiden units gain +10 [Leadership] and +4 [Melee Defense]
                        if(unit.ValueRO.unitName == UnitName.Shieldmaidens) {
                            meleeDefense += 4;
                        }
                        break;
                    case 7:
                        //Supernova of the West: All units gain +5 [Charge Bonus] and +4 [Melee Attack]
                        meleeAttack += 4;
                        break;
                    case 8:
                        //The Forest Walks: Forest Spirits and Treants gain +5 [Melee Defense] and +4 [Weapon Strength]
                        if(unit.ValueRO.unitName == UnitName.ForestSpirits || unit.ValueRO.unitName == UnitName.Treants) {
                            meleeDefense += 5;
                            weaponStrength += 4;
                        }
                        break;
                    case 10:
                        // Bloodsworn Prince: Bloodsworn and Bloodsworn Knights gain +15 [Leadership] and +4 [Melee Attack]
                        if(unit.ValueRO.unitName == UnitName.Bloodsworn || unit.ValueRO.unitName == UnitName.BloodswornKnights) {
                            meleeAttack += 8;
                        }
                        break;
                    case 11:
                        //Nagoya Steel: All units gain +4 [Weapon Strength]
                        weaponStrength += 4;
                        //Bushido Discipline: If army contains only Sakura Dynasty units, all units gain +20 [Leadership] and +4 [Melee Attack]
                        if (campaignSaveDataHolder.OnlySakuraUnits)
                            meleeAttack += 4;
                        break;
                    case 12:
                        //Innovator's Legacy: EmperorsArquebusiers Gain +10 [Accuracy] and +4 [Missile Strength]
                        if(unit.ValueRO.unitName == UnitName.EmperorsArquebusiers) {
                            accuracy += 10;
                            missileStrength += 4;
                        }
                        //Bushido Discipline: If army contains only Sakura Dynasty units, all units gain +20 [Leadership] and +4 [Melee Attack]
                        if (campaignSaveDataHolder.OnlySakuraUnits)
                            meleeAttack += 4;
                        break;
                    case 13:
                        // Ancestral Hatred: All units gain +10 [Melee Attack] and +4 [Weapon Strength] when fignting the Gruntkin
                        if(campaignSaveDataHolder.EnemyRace == Race.Gruntkin) {
                            meleeAttack += 10;
                            weaponStrength += 4;
                        }
                        break;
                    case 14:
                         //Blasting Barrels: Artillery units gain +10 [Accuracy] and +10 [Range]. 
                        if(unit.ValueRO.unitType == UnitType.Artillery) {
                            accuracy += 10;
                            range += 10;
                        }
                        break;
                    case 15:
                    //Kobold Kammandos: Kobold units gain +10 [Leadership] and +4 [Melee Attack]
                        if(unit.ValueRO.unitName == UnitName.KoboldBrawlers || unit.ValueRO.unitName == UnitName.ScalebowKobolds) {
                            meleeAttack += 4;
                        }
                        break;
                    case 16:
                         //Beastmaster: Large units gain +10 [Leadership] and +4 [Melee Defense], // Sacred Guard: StegoplateGuard gain +15 [Armor] and +4 [Weapon Strength]
                        if(squadStats.unitSize == UnitSize.Monstrous || squadStats.unitSize == UnitSize.Cavalry || squadStats.unitSize == UnitSize.SingleUnit) {
                            meleeDefense += 4;
                        }
                        if(unit.ValueRO.unitName == UnitName.StegoplateGuard) {
                            armor += 15;
                            weaponStrength += 4;
                        }
                        break;
                }
            }

            if(!campaignSaveDataHolder.IsCustomBattle && team == Team.Player)
            {
                if(Contains(GearID.DiamondTippedArrows) && squadStats.unitType == UnitType.Ranged) 
                    unitAttributes.ArmorPiercing = true;
                if(Contains(GearID.Turkey) && squadStats.unitType == UnitType.Ranged) 
                    unitAttributes.AntiLarge = true;
                if(Contains(GearID.HeavyWeapons) && squadStats.RarityTier == UnitRarity.Rare) 
                    squadStats.SquadAttributes.ArmorPiercing = true; 

                if(Contains(GearID.ArmingSwords) && squadStats.unitType == UnitType.Melee)
                    meleeAttack += GearData.GEAR_ARMINGSWORDS_MODIFIER;
                if(Contains(GearID.BucklerShields) && (squadStats.SquadAttributes.StandardShields || squadStats.SquadAttributes.HeavyShields))
                    meleeDefense += GearData.GEAR_BUCKLERSHIELDS_MODIFIER;
                if(Contains(GearID.TowerShields) && (squadStats.SquadAttributes.StandardShields || squadStats.SquadAttributes.HeavyShields))
                    shieldBlockChance = (float)GearData.GEAR_TOWERSHIELDS_MODIFIER/100f;
                if(Contains(GearID.Longbows) && squadStats.unitType == UnitType.Ranged)
                    range += GearData.GEAR_LONGBOWS_MODIFIER;
                if(Contains(GearID.Glaives) && squadStats.SquadAttributes.AntiLarge)
                    weaponStrength += GearData.GEAR_GLAIVES_MODIFIER;
                if(Contains(GearID.TexanBBQ) && squadStats.unitType == UnitType.Melee)
                    maxHitPoints += GearData.GEAR_TEXANBBQ_MODIFIER;
                if(Contains(GearID.BallisticCharts))
                    accuracy += GearData.GEAR_BALLISTICCHARTS_MODIFIER;
                if(Contains(GearID.ConscriptionOrders) && squadStats.RarityTier == UnitRarity.Common) {
                    meleeAttack += GearData.GEAR_CONSCRIPTIONORDERS_MODIFIER;
                    meleeDefense += GearData.GEAR_CONSCRIPTIONORDERS_MODIFIER;
                }
                if(Contains(GearID.JoustingLances) && squadStats.unitSize == UnitSize.Cavalry)
                {
                    weaponStrength += GearData.GEAR_JOUSTINGLANCES_MODIFIER;
                }
                if(Contains(GearID.GnomishArmorers) && squadStats.RarityTier == UnitRarity.Rare)
                    meleeDefense += GearData.GEAR_GNOMISHARMORERS_MODIFIER;
                if(Contains(GearID.WellHonedAxes) && unitAttributes.ArmorPiercing) //must be after diamond tipped arrows
                    meleeAttack += GearData.GEAR_WELLHONEDAXES_MODIFIER;
                if(Contains(GearID.RavensEye) && squadStats.RarityTier != UnitRarity.Common && squadStats.unitType == UnitType.Ranged)
                    accuracy += GearData.GEAR_RAVENSEY_MODIFIER;
                if(Contains(GearID.RingoftheElvenKing) && squadStats.unitType == UnitType.Ranged)
                    missileStrength += GearData.GEAR_RINGOFTHEELVENKING_MODIFIER;
            } 

            entityCommandBuffer.AddComponent(entity, new EntityTeam {
                Value = team
            });

            if(squadStats.unitType != UnitType.Melee)
            {
                float rangeMultiplier = 1;
                float accuracyMultiplier = 1;

                LocalToWorld localTransform = SystemAPI.GetComponent<LocalToWorld>(entity);
                float timerMax = squadStats.unitType == UnitType.Artillery || squadStats.unitType == UnitType.Structure ? squadStats.rateOfFire : TabletopTavernConstants.RANGED_ATTACK_COOLDOWN;

                entityCommandBuffer.AddComponent<RangedMeleeConverter>(entity);
                entityCommandBuffer.AddComponent<RangedUnitTag>(entity);
                bool isArtillery = squadStats.unitType == UnitType.Artillery;
                entityCommandBuffer.AddComponent(entity, new ShootAttack {
                    timer = 0,
                    timerMax = timerMax,
                    damageAmount = missileStrength,
                    Range = range * rangeMultiplier,
                    Accuracy = (int)(accuracy * accuracyMultiplier),
                    ProjectileEntity = entitiesReferences.GetProjectileEntityForUnitName(unit.ValueRO.unitName),
                    shootAnimationDelay = isArtillery ? 0.25f : 0.5f,
                });

                if(squadStats.unitType == UnitType.Ranged || squadStats.unitType == UnitType.Artillery) {
                    entityCommandBuffer.AddComponent(entity, new RangedFireModeUnitComponent { FireMode = RangedFireMode.Volley });
                }

            } else {
                entityCommandBuffer.AddComponent<MeleeUnitTag>(entity);
            }
  
            entityCommandBuffer.AddComponent(entity, new MeleeAttack {
                timerMax = squadStats.attackCooldown,
                MeleeAttackValue = meleeAttack,
                WeaponStrength = weaponStrength,
                timer = _random.NextFloat(0f, squadStats.attackCooldown)
            });

            entityCommandBuffer.AddComponent(entity, new MaxHitPoints
            {
                Value = maxHitPoints
            });
            if(entityManager.HasComponent<ModifyHealthOnSpawn>(entity)) {
                int healthValue = entityManager.GetComponentData<ModifyHealthOnSpawn>(entity).Value;
                entityCommandBuffer.AddComponent(entity, new Health {
                    Value = healthValue,
                    onHealthChanged = true,
                });
            } else {
                entityCommandBuffer.AddComponent(entity, new Health {
                    Value = maxHitPoints,
                    onHealthChanged = true,
                });
            }
  
            entityCommandBuffer.AddBuffer<DamageBufferElement>(entity);
            entityCommandBuffer.AddBuffer<SFXBufferElement>(entity);
            

            //Tags for damage multipliers
            if(squadStats.unitSize == UnitSize.Monstrous || squadStats.unitSize == UnitSize.Cavalry || squadStats.unitSize == UnitSize.SingleUnit) {
                if(squadStats.unitSize != UnitSize.Cavalry && squadStats.unitType != UnitType.Structure) {
                    entityCommandBuffer.AddComponent(entity, new MonsterTag { KnockbackRange = squadStats.ExplosionRange, KnockbackInitialDamage = squadStats.ExplosionDamage });
                }
                if(squadStats.unitType != UnitType.Structure) {
                    entityCommandBuffer.AddComponent<LargeTag>(entity);
                }
            }
            if(squadStats.unitSize == UnitSize.Infantry || squadStats.unitSize == UnitSize.Artillery) {
                entityCommandBuffer.AddComponent<InfantryTag>(entity);
            }

            if(squadStats.SquadAttributes.StandardShields || squadStats.SquadAttributes.HeavyShields)//|| squadStats.SquadAttributes.TowerShields
            {
                entityCommandBuffer.AddComponent(entity, new Shield {
                    ShieldBlockChance = shieldBlockChance,
                });
                entityCommandBuffer.AddComponent(entity, new ShieldedStanceUnitComponent { Stance = ShieldedStance.Balanced });
            }
            else
            {
                entityCommandBuffer.AddComponent(entity, new ShieldedStanceUnitComponent { Stance = ShieldedStance.None });
                
            }
            
            entityCommandBuffer.AddComponent(entity, new MeleeDefense {
                Value = meleeDefense
            });

            //has rider
            if( unit.ValueRO.unitName == UnitName.BorderlandRiders || 
                unit.ValueRO.unitName == UnitName.HearthboundKnights || 
                unit.ValueRO.unitName == UnitName.RoyalCavaliers ||
                unit.ValueRO.unitName == UnitName.StarStriders ||
                unit.ValueRO.unitName == UnitName.VeilkinDrakes ||
                unit.ValueRO.unitName == UnitName.Direriders ||
                unit.ValueRO.unitName == UnitName.Nightriders ||
                unit.ValueRO.unitName == UnitName.BloodswornKnights ||
                unit.ValueRO.unitName == UnitName.DragonTachi ||
                unit.ValueRO.unitName == UnitName.AngelsOfDeath ||
                unit.ValueRO.unitName == UnitName.ThunderhoofChargers ||
                unit.ValueRO.unitName == UnitName.RaptorRiders
            ) {
                entityCommandBuffer.AddComponent(entity, new Cavalry {
                    unitName = unit.ValueRO.unitName
                });
            }

            // entityCommandBuffer.AddComponent<DamageRecievedFrom>(entity, new DamageRecievedFrom { SquadId = 99 });
            // entityCommandBuffer.AddComponent<CatchUpTag>(entity);

            // entityCommandBuffer.AddComponent(entity, new SFXIDHolder
            // {
            //     deathSFXID = DataTypes.GetDeathSFXID(unit.ValueRO.unitName),
            //     idleSFXID = DataTypes.GetIdleSFXID(unit.ValueRO.unitName),
            // });

            float sightRange = squadStats.unitSize switch
            {
                UnitSize.SingleUnit => TabletopTavernConstants.MELEE_DETECTION_RANGE * 2f,
                UnitSize.Monstrous  => TabletopTavernConstants.MELEE_DETECTION_RANGE * 1.5f,
                UnitSize.Cavalry    => TabletopTavernConstants.MELEE_DETECTION_RANGE * 1.5f,
                _                   => TabletopTavernConstants.MELEE_DETECTION_RANGE,
            };

            entityCommandBuffer.AddComponent(entity, new UnitFindTarget {
                sightRange = sightRange,
                TargetTeam = team == Team.Player ? Team.Enemy : Team.Player,
                timerMax = 0.5f,
                TargetedSquadEntity = Entity.Null
            });
            entityCommandBuffer.SetComponentEnabled<UnitFindTarget>(entity, false);

            Color hoveredColor = team == Team.Player ? TabletopTavernConstants.PLAYER_TRIANGLE_COLOR: TabletopTavernConstants.ENEMY_TRIANGLE_COLOR;
            Color selectedColor = team == Team.Player ? TabletopTavernConstants.PLAYER_TRIANGLE_COLOR : TabletopTavernConstants.ENEMY_TRIANGLE_COLOR;

            entityCommandBuffer.AddComponent(entity, new TriangleEntity { 
                hoverColor = new Vector4(hoveredColor.r, hoveredColor.g, hoveredColor.b, TabletopTavernConstants.TRIANGLE_HOVER_BLOOM),
                selectedColor = new Vector4(selectedColor.r, selectedColor.g, selectedColor.b, TabletopTavernConstants.TRIANGLE_SELECTED_BLOOM),
                disabledColor = TabletopTavernConstants.DISABLED_TRIANGLE_COLOR,
                activeColor = TabletopTavernConstants.DISABLED_TRIANGLE_COLOR,
            });
            entityCommandBuffer.SetComponentEnabled<TriangleEntity>(entity, true);

            // if(team != team.Player) continue;

            // BattleFieldPreset.Weather weather = SaveDataHandler.Load().battleFieldPreset.weather;

            if(unitAttributes.ArmorPiercing) {
                entityCommandBuffer.AddComponent<ArmorPiercingTag>(entity);
            }
            if(unitAttributes.AntiInfantry) {
                entityCommandBuffer.AddComponent<AntiInfantryTag>(entity);
            }
            if(unitAttributes.AntiLarge) {
                entityCommandBuffer.AddComponent<AntiLargeTag>(entity);
            }
            if (unitAttributes.Armored)
            {
                entityCommandBuffer.AddComponent(entity, new ArmoredTag
                {
                    ArmorMitigation = armor / (armor + 100f)
                });
            }
            if (squadStats.SquadAttributes.Ethereal)
            {
                entityCommandBuffer.AddComponent(entity, new PhysicalDamageMultiplier { Value = 0.5f });
                entityCommandBuffer.AddComponent(entity, new MagicalDamageMultiplier { Value = 0.5f });
            }
            if (squadStats.SquadAttributes.ThickScales)
            {
                entityCommandBuffer.AddComponent(entity, new MissileResistance { DamageMultiplier = 0.75f });
            }

            if (squadStats.unitType == UnitType.Artillery)
            {
                entityCommandBuffer.AddComponent(entity, new ArtilleryUnit
                {
                    SquadID = unit.ValueRO.squadId,
                    ExplosionDamage = squadStats.ExplosionDamage,
                    ExplosionRange = squadStats.ExplosionRange,
                    ExplosionForce = squadStats.ExplosionForce
                });
                entityCommandBuffer.AddComponent(entity, new ResistKnockbackTag { });
            }
            else if (squadStats.unitSize == UnitSize.SingleUnit)
            {
                entityCommandBuffer.AddComponent(entity, new ResistKnockbackTag { });
            }
            else if (squadStats.unitSize == UnitSize.Monstrous)
            {
                entityCommandBuffer.AddComponent(entity, new ResistKnockbackTag { });
            }

            // Size-based separation: larger units have low Weight (barely yield) and wide Radius
            // (detect smaller units early). Smaller units have high Weight (move away quickly).
            // Net effect: large units push through small ones without needing reciprocal force logic.
            // Separation.Radius must be > AgentShape.Radius (0.75) so the spatial query finds
            // neighbours before they are already overlapping (two touching infantry are 1.5 apart).
            // Large units only separate from other large units (Layer1/Layer2), not from infantry
            // (NavigationLayers.Default). Infantry keeps Everything so they still yield to large
            // units when both are moving — the weight asymmetry (0.2-0.7 vs 1.5) ensures infantry
            // scatter away while the large unit holds its course.
            AgentSeparation separation = squadStats.unitSize switch
            {
                UnitSize.SingleUnit => new AgentSeparation { Radius = 2.5f, Weight = 0.2f, Layers = NavigationLayers.Layer1 | NavigationLayers.Layer2 },
                UnitSize.Monstrous  => new AgentSeparation { Radius = 2.0f, Weight = 0.3f, Layers = NavigationLayers.Layer1 | NavigationLayers.Layer2 },
                UnitSize.Cavalry    => new AgentSeparation { Radius = 1.8f, Weight = 0.7f, Layers = NavigationLayers.Layer1 | NavigationLayers.Layer2 },
                UnitSize.Artillery  => new AgentSeparation { Radius = 1.6f, Weight = 1.2f, Layers = NavigationLayers.Everything },
                _                   => new AgentSeparation { Radius = 1.6f, Weight = 1.5f, Layers = NavigationLayers.Everything }, // Infantry
            };
            if (squadStats.unitType == UnitType.Structure)
            {
                // separation.Radius = 10f;
                if (entityManager.HasComponent<AgentShape>(entity))
                {
                    AgentShape agentShape = entityManager.GetComponentData<AgentShape>(entity);
                    agentShape.Radius = 2.5f;
                    entityCommandBuffer.SetComponent(entity, agentShape);
                }
            }
            entityCommandBuffer.SetComponent(entity, separation);
            entityCommandBuffer.AddComponent(entity, new BaseSeparationWeight { Value = separation.Weight });
            // AgentSonarAvoid.Radius on the prefab is 0.25 — smaller than AgentShape.Radius (0.75),
            // so sonar steering fires after units are already overlapping. Set it to match the shape
            // radius so steering kicks in as soon as another unit enters personal space.
            if (entityManager.HasComponent<AgentSonarAvoid>(entity))
            {
                AgentSonarAvoid sonarAvoid = entityManager.GetComponentData<AgentSonarAvoid>(entity);
                sonarAvoid.Radius = squadStats.unitSize switch
                {
                    UnitSize.SingleUnit => 2.0f,
                    UnitSize.Monstrous  => 1.5f,
                    UnitSize.Cavalry    => 1.2f,
                    _                   => 0.85f, // Infantry / Artillery
                };
                // Large units ignore infantry (Default layer) so sonar does not steer them around
                // infantry walls — LargeUnitPushSystem handles the actual physical displacement.
                sonarAvoid.Layers = squadStats.unitSize switch
                {
                    UnitSize.SingleUnit or UnitSize.Monstrous or UnitSize.Cavalry =>
                        NavigationLayers.Layer1 | NavigationLayers.Layer2,
                    _ => NavigationLayers.Everything,
                };
                entityCommandBuffer.SetComponent(entity, sonarAvoid);
            }

            // Assign each unit to a navigation layer by size so spatial queries can discriminate.
            // Infantry/Artillery → Default (bit 0), Cavalry → Layer1 (bit 1), Large → Layer2 (bit 2).
            if (entityManager.HasComponent<Agent>(entity))
            {
                Agent agent = entityManager.GetComponentData<Agent>(entity);
                agent.Layers = squadStats.unitSize switch
                {
                    UnitSize.Cavalry    => NavigationLayers.Layer1,
                    UnitSize.Monstrous  => NavigationLayers.Layer2,
                    UnitSize.SingleUnit => NavigationLayers.Layer2,
                    _                   => NavigationLayers.Default, // Infantry, Artillery
                };
                entityManager.SetComponentData(entity, agent);
            }

            // Large units only hard-collide with other large units (Layer1/Layer2).
            // Infantry is displaced by LargeUnitPushSystem instead, not AgentCollider.
            if (entityManager.HasComponent<AgentCollider>(entity))
            {
                AgentCollider collider = entityManager.GetComponentData<AgentCollider>(entity);
                collider.Layers = squadStats.unitSize switch
                {
                    UnitSize.Cavalry    => NavigationLayers.Layer1 | NavigationLayers.Layer2,
                    UnitSize.Monstrous  => NavigationLayers.Layer1 | NavigationLayers.Layer2,
                    UnitSize.SingleUnit => NavigationLayers.Layer1 | NavigationLayers.Layer2,
                    _                   => NavigationLayers.Default,
                };
                entityCommandBuffer.SetComponent(entity, collider);
            }

            entityCommandBuffer.AddComponent(entity, new RetreatingUnit { });
            entityCommandBuffer.SetComponentEnabled<RetreatingUnit>(entity, false);

            entityCommandBuffer.AddComponent<ApplyKnockbackOnContact>(entity);
            entityCommandBuffer.SetComponentEnabled<ApplyKnockbackOnContact>(entity, false);

            
// #if UNITY_EDITOR
            // entityManager.SetName(entity, $"Unit - {entityManager.GetName(unit.ValueRO.squadEntity)}");
// #endif
        }
    }
}