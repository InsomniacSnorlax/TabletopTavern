// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Physics;
// using Unity.Transforms;
// using UnityEngine;
// using Unity.Mathematics;

// partial struct FindTargetSystem : ISystem {

//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<FindTargets>();
//     }
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state) {
//         PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
//         CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
//         NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
//         EntityManager entityManager = state.EntityManager;

//         foreach ((
//             RefRO<LocalTransform> localTransform,
//             RefRW<FindTarget> findTarget,
//             RefRW<Target> target,
//             RefRO<Unit> unit,
//             RefRO<SetDestination> unitDestination,
//             InCombat inCombat,
//             Entity entity)
//             in SystemAPI.Query<
//                 RefRO<LocalTransform>,
//                 RefRW<FindTarget>,
//                 RefRW<Target>,
//                 RefRO<Unit>,
//                 RefRO<SetDestination>,
//                 InCombat
//                 >().WithEntityAccess()) {

//             findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
//             if (findTarget.ValueRO.timer > 0f) {
//                 continue;
//             }
//             findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

//             //dont swap target if current target is not null
//             // if(target.ValueRO.targetEntity != Entity.Null){
//             //     continue;
//             // }

//             // if(targetOverride.ValueRO.targetEntity != Entity.Null) {
//             //     target.ValueRW.targetEntity = targetOverride.ValueRO.targetEntity;
//             //     continue;
//             // }

//             distanceHitList.Clear();
//             CollisionFilter collisionFilter = new CollisionFilter {
//                 BelongsTo = ~0u,
//                 CollidesWith = 1u << GameAssets.UNITS_LAYER,
//                 GroupIndex = 0,
//             };
//             Entity closestEntity = Entity.Null;
//             float closestDistance = float.MaxValue;
//             float currentTargetDistanceOffset = 0f;

//             if(target.ValueRO.targetEntity != Entity.Null) {
//                 closestEntity = target.ValueRO.targetEntity;
//                 closestDistance = math.distance(localTransform.ValueRO.Position, SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity).Position);
//                 currentTargetDistanceOffset = 0.5f; //only swap if new target is closer by 2 units

//                 if(SystemAPI.IsComponentEnabled<MoveOverride>(target.ValueRO.targetEntity)) {
//                     target.ValueRW.targetEntity = Entity.Null;
//                     continue;
//                 }
//             }

//             if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.sightRange, ref distanceHitList, collisionFilter)) {
//                 foreach (DistanceHit distanceHit in distanceHitList) {
//                     if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity) || SystemAPI.IsComponentEnabled<MoveOverride>(distanceHit.Entity)) {
//                         continue;
//                     }

//                     Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
//                     if (targetUnit.team == findTarget.ValueRO.targetteam) {

//                         if(closestEntity == Entity.Null) {
//                             closestEntity = distanceHit.Entity;
//                             closestDistance = distanceHit.Distance;
//                         } else {
//                             if(distanceHit.Distance + currentTargetDistanceOffset < closestDistance) {
//                                 closestEntity = distanceHit.Entity;
//                                 closestDistance = distanceHit.Distance;
//                             }
//                         }
//                     }
//                 }
//             }

//             if(closestEntity != Entity.Null){
//                 target.ValueRW.targetEntity = closestEntity;
//                 // entityManager.SetComponentEnabled<FindTarget>(entity, false);
//             } else {
//                 //just move towards the squad center
//                 if(entityManager.Exists(findTarget.ValueRO.TargetedSquadEntity)){
//                     SetDestination setDestination = entityManager.GetComponentData<SetDestination>(entity);
//                     setDestination.destinationPosition = entityManager.GetComponentData<SquadMovementComponent>(findTarget.ValueRO.TargetedSquadEntity).SquadCenter;
//                     entityManager.SetComponentData(entity, setDestination);
//                 }

//                 findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;
//                 // Debug.Log($"No target found for entity ");
//             }
//         }
//     }
// }