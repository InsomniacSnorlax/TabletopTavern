using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
// using Unity.Logging;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

// [UpdateInGroup(typeof(BeginSimulationMainThreadGroup))]
public partial struct GameInitializeSystem : ISystem
{
    private uint _nonDeterministicSeed;

    public void OnCreate(ref SystemState state)
    {
        // state.RequireForUpdate<Config>();
        // state.RequireForUpdate<ShipCollection>();
        // state.RequireForUpdate<BuildingCollection>();
        // state.RequireForUpdate<ShipSpawnParams>();
        // state.RequireForUpdate<GameCamera>();

        // _nonDeterministicSeed = GameUtilities.GetUniqueUIntFromInt(DateTime.Now.Millisecond);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Initialize the game when we find a config singleton with disabled initialized state
        // if (SystemAPI.TryGetSingleton(out Config config))
        // {
        //     if (!config.MustInitializeGame)
        //         return;

        //     // Set state to initialized
        //     config.MustInitializeGame = false;
        //     SystemAPI.SetSingleton(config);

            
        // }
    }

    // private void PlaceCamera(ref SystemState state, in Config config)
    // {
    //     Entity cameraEntity = SystemAPI.GetSingletonEntity<GameCamera>();
    //     LocalTransform cameraTransform = state.EntityManager.GetComponentData<LocalTransform>(cameraEntity);
    //     cameraTransform.Position.z = -config.HomePlanetSpawnRadius * config.StartCameraDistanceRatio;
    //     state.EntityManager.SetComponentData<LocalTransform>(cameraEntity, cameraTransform);
    // }
}