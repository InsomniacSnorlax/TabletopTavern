// using Unity.Burst;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Transforms;
// using ProjectDawn.Navigation;

// partial struct UnitHitBySpellSystem : ISystem
// {
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
//         PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
//         float deltaTime = SystemAPI.Time.DeltaTime;

//         foreach (var (unitHitBySpell, lifetime, localTransform, setDestination, agentBody, Entity) in SystemAPI.Query<
//             RefRO<UnitHitBySpell>,
//             RefRW<ForceLifetime>,
//             RefRW<LocalTransform>,
//             RefRW<SetDestination>,
//             RefRW<AgentBody>
//             >().WithEntityAccess())
//         {
//             lifetime.ValueRW.TotalTime -= deltaTime;
//             // Inside the update or effect loop:
//             if (lifetime.ValueRW.RemainingTime > 0)
//             {
//                 float totalLifetime = 1f; // Total time for the effect
//                 lifetime.ValueRW.RemainingTime -= deltaTime;

//                 // Calculate the normalized elapsed time (0 -> 1 over the total lifetime)
//                 float normalizedElapsedTime = math.saturate(1f - (lifetime.ValueRW.RemainingTime / totalLifetime));

//                 // Use initialPosition for direction calculation to avoid feedback loop
//                 float3 direction = math.normalize(unitHitBySpell.ValueRO.InitHitLocation - unitHitBySpell.ValueRO.SpellPosition);
//                 float3 totalDisplacement = direction * unitHitBySpell.ValueRO.SpellForce;
//                 float3 displacementPerFrame = totalDisplacement * deltaTime / totalLifetime;

//                 // Calculate Y-axis displacement to create an arc
//                 float yDisplacement = -4f * (normalizedElapsedTime - 0.5f) * (normalizedElapsedTime - 0.5f) + 1f;
//                 yDisplacement *= 0.25f; // Scale the arc height

//                 if (normalizedElapsedTime > 0.5f) {
//                     yDisplacement = -yDisplacement;
//                 }
//                 if(normalizedElapsedTime > 0.25f && normalizedElapsedTime < 0.75f) {
//                     yDisplacement *= 0.3f;
//                 }

//                 float3 arcDisplacement = new float3(displacementPerFrame.x, yDisplacement, displacementPerFrame.z);

//                 // Update the entity's position with displacement and Y arc
//                 localTransform.ValueRW.Position += arcDisplacement;

//                 //clamp it above the starting position
//                 if (localTransform.ValueRW.Position.y < unitHitBySpell.ValueRO.InitHitLocation.y) {
//                     localTransform.ValueRW.Position.y = unitHitBySpell.ValueRO.InitHitLocation.y;
//                 }

//                 // Rotate the unit to face the direction of the spell
//                 localTransform.ValueRW.Rotation = quaternion.LookRotation(direction, new float3(0, 1, 0));

//                 // Debug.Log($"normalizedElapsedTime: {normalizedElapsedTime}, Y position: {localTransform.ValueRW.Position.y}");
//             } else {
//                 localTransform.ValueRW.Position.y = unitHitBySpell.ValueRO.InitHitLocation.y;
//             }

//             // If the force duration ends, clean up components
//             if (lifetime.ValueRW.TotalTime <= 0) {
//                 ecb.RemoveComponent<UnitHitBySpell>(Entity);
//                 ecb.RemoveComponent<ForceLifetime>(Entity);
//                 ecb.SetComponentEnabled<NavMeshPath>(Entity, true);
//                 ecb.SetComponentEnabled<AgentSonarAvoid>(Entity, false);
//                 agentBody.ValueRW.SetDestination(setDestination.ValueRO.squadPosition);
//             }
//         }
//     }
// }