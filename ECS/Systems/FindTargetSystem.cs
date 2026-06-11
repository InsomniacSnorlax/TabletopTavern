using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

partial struct FindTargetSystem : ISystem {
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private ComponentLookup<Unit> unitComponentLookup;
    private ComponentLookup<SetDestination> setDestinationComponentLookup;
    private ComponentLookup<MoveOverride> moveOverrideComponentLookup;
    private ComponentLookup<SquadMovementComponent> SquadMovementComponentLookup;
    private ComponentLookup<RotateUnit> rotateUnitComponentLookup;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FindTargets>();
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
        unitComponentLookup = state.GetComponentLookup<Unit>(true);
        setDestinationComponentLookup = state.GetComponentLookup<SetDestination>(false);
        moveOverrideComponentLookup = state.GetComponentLookup<MoveOverride>(true);
        SquadMovementComponentLookup = state.GetComponentLookup<SquadMovementComponent>(true);
        rotateUnitComponentLookup = state.GetComponentLookup<RotateUnit>(false);
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        localTransformComponentLookup.Update(ref state);
        setDestinationComponentLookup.Update(ref state);
        unitComponentLookup.Update(ref state);
        moveOverrideComponentLookup.Update(ref state);
        SquadMovementComponentLookup.Update(ref state);
        rotateUnitComponentLookup.Update(ref state);

        FindTargetJob squadCenterJob = new FindTargetJob {
            LocalTransformComponentLookup = localTransformComponentLookup,
            setDestinationComponentLookup = setDestinationComponentLookup,
            UnitComponentLookup = unitComponentLookup,
            MoveOverrideComponentLookup = moveOverrideComponentLookup,
            DeltaTime = SystemAPI.Time.DeltaTime,
            physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>(),
            SquadMovementComponentLookup = SquadMovementComponentLookup,
            RotateUnitComponentLookup = rotateUnitComponentLookup
        };
        state.Dependency = squadCenterJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithPresent(typeof(InCombat))]
public partial struct FindTargetJob : IJobEntity {
    [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformComponentLookup;
    [ReadOnly] public ComponentLookup<Unit> UnitComponentLookup;
    [ReadOnly] public ComponentLookup<MoveOverride> MoveOverrideComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<SetDestination> setDestinationComponentLookup;
    [ReadOnly] public ComponentLookup<SquadMovementComponent> SquadMovementComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<RotateUnit> RotateUnitComponentLookup;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public PhysicsWorldSingleton physicsWorldSingleton;

    [BurstCompile]
    public void Execute (ref UnitFindTarget findTarget, ref Target target, Entity entity)
    {
        findTarget.timer -= DeltaTime;
        if (findTarget.timer > 0f) {
            return;
        }

        LocalTransform localTransform = LocalTransformComponentLookup[entity];
        findTarget.timer = findTarget.timerMax;

        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        distanceHitList.Clear();
        CollisionFilter collisionFilter = new CollisionFilter {
            BelongsTo = ~0u,
            CollidesWith = 1u << TabletopTavernConstants.UNITS_LAYER,
            GroupIndex = 0,
        };
        Entity closestEntity = Entity.Null;
        float closestDistance = float.MaxValue;

        if(target.targetEntity != Entity.Null) {
            if (!LocalTransformComponentLookup.HasComponent(target.targetEntity)) {
                target.targetEntity = Entity.Null;
            } else {
                closestEntity = target.targetEntity;
                closestDistance = math.distance(localTransform.Position, LocalTransformComponentLookup[target.targetEntity].Position);
                if(MoveOverrideComponentLookup.IsComponentEnabled(target.targetEntity)) {
                    target.targetEntity = Entity.Null;
                    return;
                }
            }
        }

        if (collisionWorld.OverlapSphere(localTransform.Position, findTarget.sightRange, ref distanceHitList, collisionFilter)) {
            foreach (DistanceHit distanceHit in distanceHitList) {
                if (!LocalTransformComponentLookup.EntityExists(distanceHit.Entity) || 
                    !UnitComponentLookup.HasComponent(distanceHit.Entity) || 
                    MoveOverrideComponentLookup.IsComponentEnabled(distanceHit.Entity)) {
                    continue;
                }

                Unit targetUnit = UnitComponentLookup[distanceHit.Entity];
                if (targetUnit.Team == findTarget.TargetTeam) {

                    if(closestEntity == Entity.Null) {
                        closestEntity = distanceHit.Entity;
                        closestDistance = distanceHit.Distance;
                    } else {
                        //only swap if new target is closer by 1 
                        if(distanceHit.Distance + 1 < closestDistance) {
                            closestEntity = distanceHit.Entity;
                            closestDistance = distanceHit.Distance;
                        }
                    }
                }
            }
        }

        if (closestEntity != Entity.Null)
        {
            if (target.targetEntity != closestEntity)
            {
                // Face the new target
                float3 toTarget = LocalTransformComponentLookup[closestEntity].Position - localTransform.Position;
                if (math.lengthsq(toTarget) > 0.0001f && RotateUnitComponentLookup.HasComponent(entity))
                {
                    RotateUnitComponentLookup[entity] = new RotateUnit { targetRotation = quaternion.LookRotation(math.normalizesafe(toTarget), math.up()) };
                    RotateUnitComponentLookup.SetComponentEnabled(entity, true);
                }
            }
            target.targetEntity = closestEntity;
            // entityManager.SetComponentEnabled<FindTarget>(entity, false);
        }
        else
        {
            //just move towards the squad center
            if (LocalTransformComponentLookup.EntityExists(findTarget.TargetedSquadEntity))
            {
                SetDestination setDestination = setDestinationComponentLookup[entity];
                setDestination.destinationPosition = SquadMovementComponentLookup[findTarget.TargetedSquadEntity].SquadCenter;
                setDestinationComponentLookup[entity] = setDestination;
            }

            findTarget.timer = findTarget.timerMax;
            // Debug.Log($"No target found for entity ");
        }
        distanceHitList.Dispose();
    }
}