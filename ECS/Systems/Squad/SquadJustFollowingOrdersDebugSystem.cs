// using Unity.Burst;
// using Unity.Entities;
// using UnityEngine;

// [BurstCompile]
// public partial struct SquadJustFollowingOrdersDebugSystem : ISystem
// {
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<BattlePhase>();
//     }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (justFollowingOrders, entity) in SystemAPI.Query<RefRW<JustFollowingOrders>>()
//             .WithPresent<JustFollowingOrders, PlayerSquad>()
//             .WithAbsent<BrokenSquadTag>()
//             .WithEntityAccess())
//         {
//             Debug.Log($"{entity.Index} is {(SystemAPI.IsComponentEnabled<JustFollowingOrders>(entity) ? "" : "NOT ")}just following orders.");

//             // if disabled continue
//             // if (!SystemAPI.IsComponentEnabled<JustFollowingOrders>(entity)) continue;
//         }
//     }
// }