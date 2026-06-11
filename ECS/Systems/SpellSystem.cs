// using System.Collections.Generic;
// using Unity.Burst;
// using Unity.Entities;
// using ProjectDawn.Navigation;
// using GPUECSAnimationBaker.Engine.AnimatorSystem;

// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Collections;
// using Unity.Transforms;

// partial struct SpellSystem : ISystem
// {
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         EntityManager entityManager = state.EntityManager;
//         EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
//         PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
//         CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
//         NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

//         foreach (var (spellEntity, Entity) in SystemAPI.Query<
//             RefRO<SpellEntity>
//         >().WithEntityAccess())
//         {

//             DamageBufferElement damageBufferElement = spellEntity.ValueRO.DamageBufferElement;
//             float3 spellPosition = spellEntity.ValueRO.SpellPosition;
//             float spellRadius = spellEntity.ValueRO.SpellRadius;
//             distanceHitList.Clear();
//             CollisionFilter collisionFilter = new CollisionFilter
//             {
//                 BelongsTo = ~0u,
//                 CollidesWith = 1u << GameAssets.UNITS_LAYER,
//                 GroupIndex = 0,
//             };

//             if (collisionWorld.OverlapSphere(spellPosition, spellRadius, ref distanceHitList, collisionFilter))
//             {
//                 foreach (DistanceHit distanceHit in distanceHitList)
//                 {
//                     if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity))
//                     {
//                         continue;
//                     }

//                     Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
//                     DynamicBuffer<DamageBufferElement> damageBuffer = SystemAPI.GetBuffer<DamageBufferElement>(distanceHit.Entity);
//                     damageBuffer.Add(damageBufferElement);
//                     entityCommandBuffer.AddComponent(distanceHit.Entity, new UnitHitBySpell { SpellPosition = spellPosition, SpellForce = spellEntity.ValueRO.SpellForce, InitHitLocation = SystemAPI.GetComponent<LocalTransform>(distanceHit.Entity).Position });
//                     entityCommandBuffer.AddComponent(distanceHit.Entity, new ForceLifetime { RemainingTime = 1f, TotalTime = 2.5f });

//                     // #TODO fix this had to disable after adding assemblies
//                     // entityCommandBuffer.SetComponentEnabled<UnityEngine.AI.NavMeshPath>(distanceHit.Entity, false);
//                     AnimationDataHolder animationDataHolder = entityManager.GetComponentData<AnimationDataHolder>(distanceHit.Entity);
//                     Entity childEntity = animationDataHolder.gpuEcsAnimatorEntity;

//                     GpuEcsAnimatorControlComponent controlComp = entityManager.GetComponentData<GpuEcsAnimatorControlComponent>(animationDataHolder.gpuEcsAnimatorEntity);
//                     controlComp.transitionSpeed = 0f;
//                     controlComp.animatorInfo.animationID = animationDataHolder.thrownAnimationId;

//                     RefRW<AgentBody> agentBody = SystemAPI.GetComponentRW<AgentBody>(distanceHit.Entity);
//                     agentBody.ValueRW.IsStopped = true;
//                     agentBody.ValueRW.SetDestination(SystemAPI.GetComponent<LocalTransform>(distanceHit.Entity).Position);

//                     entityCommandBuffer.SetComponent(childEntity, controlComp);
//                 }
//             }

//             if (spellEntity.ValueRO.IsOneOff) entityCommandBuffer.DestroyEntity(Entity);
//         }

//         distanceHitList.Dispose();
//     }
// }